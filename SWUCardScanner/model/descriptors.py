"""Pluggable card descriptors, shared by build_index.py and eval_matcher.py.

A descriptor turns an un-warped card crop into a unit-length vector so that matching is
a dot product. Both the index and the query side go through this same code, because any
asymmetry between them silently destroys accuracy.

Three are available, specified as a string:

    pixels:32               32x32 per-channel-normalised RGB grid  (3072-d)
    onnx:<file>             a model from export_embedder.py
    hybrid:<file>:32:0.5    both, L2-normalised separately then blended

`pixels` is deliberately included rather than assumed to be the weak option. Deep
features pooled over an image are semantic - they describe *what* is in a picture and
discard instance detail on purpose. Matching near-duplicate card art is the opposite
problem, and a dense grid of normalised colour keeps precisely the spatial layout that
distinguishes two cards sharing a frame. Which one actually wins is a question for
eval_matcher.py, not for intuition.
"""

from __future__ import annotations

import cv2
import numpy as np


def _l2(matrix: np.ndarray) -> np.ndarray:
    norms = np.linalg.norm(matrix, axis=1, keepdims=True)
    return matrix / np.maximum(norms, 1e-9)


def box_downscale(img: np.ndarray, grid: int) -> np.ndarray:
    """Average each block down to a grid x grid image.

    Equivalent to cv2.INTER_AREA when the ratio is a whole number, but written out
    explicitly because the browser has to reproduce it bit-for-bit. Canvas scaling uses
    an unspecified filter that varies between browsers, so it cannot be trusted here.
    """
    h, w = img.shape[:2]
    if h % grid or w % grid:
        raise ValueError(f"{w}x{h} does not divide evenly into a {grid}x{grid} grid")
    block_h, block_w = h // grid, w // grid
    return (img.astype(np.float32)
            .reshape(grid, block_h, grid, block_w, 3)
            .mean(axis=(1, 3)))


def binomial_blur(img: np.ndarray) -> np.ndarray:
    """Separable [1,2,1]/4 blur with edge clamping.

    A blur is what buys tolerance to corner error - without it a couple of percent of
    misalignment shifts every cell and the match collapses. This particular kernel is
    used because it is exact in integer arithmetic and trivially identical in Python and
    JavaScript, unlike a Gaussian whose kernel size and border handling are library
    conventions waiting to diverge.
    """
    for axis in (0, 1):
        padded = np.concatenate([
            np.take(img, [0], axis=axis), img, np.take(img, [-1], axis=axis),
        ], axis=axis)
        lo = np.take(padded, range(0, img.shape[axis]), axis=axis)
        mid = np.take(padded, range(1, img.shape[axis] + 1), axis=axis)
        hi = np.take(padded, range(2, img.shape[axis] + 2), axis=axis)
        img = (lo + 2.0 * mid + hi) / 4.0
    return img


class PixelDescriptor:
    """Downscaled, contrast-normalised RGB grid.

    Input arrives BGR because that is what OpenCV hands out, and is converted to RGB
    here so that the whole descriptor is defined in RGB - the order the browser has.
    Leaving it in BGR silently permutes red and blue in the query vector relative to the
    index, which mostly still matches but with badly degraded scores.
    """

    def __init__(self, grid: int = 32, blur: bool = True, highpass: int = 0):
        self.grid = grid
        self.blur = blur
        self.highpass = highpass
        self.dim = grid * grid * 3

    def __call__(self, images_bgr: list[np.ndarray]) -> np.ndarray:
        out = np.empty((len(images_bgr), self.dim), np.float32)
        for i, img in enumerate(images_bgr):
            small = box_downscale(cv2.cvtColor(img, cv2.COLOR_BGR2RGB), self.grid)
            if self.blur:
                small = binomial_blur(small)

            if self.highpass:
                # Subtracting a heavily smoothed copy removes any *gradient* across the
                # card. Global normalisation below only cancels a uniform colour cast;
                # veiling glare off a glossy card is a low-frequency ramp, and it wrecks
                # dark cards in particular because it adds light additively - the same
                # +40 is a 20% shift on a bright card and a 4x shift on a near-black one.
                smooth = small
                for _ in range(self.highpass):
                    smooth = binomial_blur(smooth)
                small = small - smooth

            # Per channel, so a warm or cold light cast cancels instead of dominating.
            for c in range(3):
                ch = small[:, :, c]
                small[:, :, c] = (ch - ch.mean()) / (ch.std() + 1e-6)
            out[i] = small.reshape(-1)
        return _l2(out)


class OnnxDescriptor:
    """A trunk exported by export_embedder.py. Already emits unit-length vectors."""

    def __init__(self, path: str, size: int | None = None):
        import onnxruntime as ort

        self.session = ort.InferenceSession(path, providers=["CPUExecutionProvider"])
        shape = self.session.get_inputs()[0].shape
        self.size = size or (shape[2] if isinstance(shape[2], int) else 224)
        self.dim = self.session.get_outputs()[0].shape[1]
        self.path = path

    def __call__(self, images_bgr: list[np.ndarray]) -> np.ndarray:
        batch = np.stack([
            cv2.cvtColor(cv2.resize(img, (self.size, self.size), interpolation=cv2.INTER_AREA),
                         cv2.COLOR_BGR2RGB)
            for img in images_bgr
        ]).astype(np.float32).transpose(0, 3, 1, 2) / 255.0
        return self.session.run(None, {"images": np.ascontiguousarray(batch)})[0]


class HybridDescriptor:
    """Pixel grid(s) plus the deep embedding, concatenated.

    With `dual=True` the pixel grid is included twice - once as-is and once high-passed.
    They fail in opposite directions: the plain grid keeps low-frequency layout, which
    survives corner error but is destroyed by glare across the card; the high-passed one
    ignores any gradient but is more sensitive to misalignment. Carrying both lets the
    whitened PCA decide how much to lean on each, instead of committing to one.
    """

    def __init__(self, onnx_path: str, grid: int = 32, weight: float = 0.5,
                 highpass: int = 0, dual: bool = False):
        self.pixels = PixelDescriptor(grid, highpass=0 if dual else highpass)
        self.pixels_hp = PixelDescriptor(grid, highpass=highpass or 6) if dual else None
        self.deep = OnnxDescriptor(onnx_path)
        self.weight = weight
        self.dual = dual
        self.dim = self.pixels.dim + self.deep.dim + (self.pixels.dim if dual else 0)

    def __call__(self, images_bgr: list[np.ndarray]) -> np.ndarray:
        parts = [self.pixels(images_bgr) * self.weight]
        if self.pixels_hp is not None:
            parts[0] *= 0.5
            parts.append(self.pixels_hp(images_bgr) * self.weight * 0.5)
        parts.append(self.deep(images_bgr) * (1.0 - self.weight))
        return _l2(np.concatenate(parts, axis=1))


def fit_pca(matrix: np.ndarray, k: int, whiten: bool = True
            ) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    """Fit a PCA basis on the reference descriptors.

    The hybrid descriptor is 5376-d, which would make a 3,118-card index 67 MB - too
    much to ship to a browser. Projecting onto the top `k` directions cuts that by an
    order of magnitude, and whitening (dividing each component by its standard
    deviation) usually *improves* retrieval as well: without it a handful of
    high-variance directions - overall brightness, the shared card frame - dominate
    every dot product and drown out the detail that separates one card from another.

    Returns (mean, components, scale) to be handed to `apply_pca`.
    """
    mean = matrix.mean(axis=0)
    centred = matrix - mean
    # Economy SVD: with fewer cards than dimensions this is far cheaper than forming
    # the 5376x5376 covariance matrix.
    _, singular, vt = np.linalg.svd(centred, full_matrices=False)
    components = vt[:k]
    if whiten:
        std = singular[:k] / np.sqrt(max(len(matrix) - 1, 1))
        scale = 1.0 / (std + 1e-8)
    else:
        scale = np.ones(k, np.float32)
    return (mean.astype(np.float32), components.astype(np.float32), scale.astype(np.float32))


def apply_pca(matrix: np.ndarray, mean: np.ndarray, components: np.ndarray,
              scale: np.ndarray) -> np.ndarray:
    """Project onto the PCA basis and renormalise, so matching stays a dot product."""
    return _l2(((matrix - mean) @ components.T) * scale)


def make_descriptor(spec: str):
    """Build a descriptor from a spec string. See the module docstring for the grammar."""
    kind, _, rest = spec.partition(":")

    if kind in ("pixels", "pixelshp"):
        parts = [p for p in rest.split(":") if p]
        grid = int(parts[0]) if parts else 32
        blur = parts[1] != "0" if len(parts) > 1 else True
        return PixelDescriptor(grid, blur, highpass=6 if kind == "pixelshp" else 0)

    if kind == "onnx":
        return OnnxDescriptor(rest)

    if kind in ("hybrid", "hybridhp", "hybrid2"):
        # Parse the numeric options off the *end*, because a Windows path contains a
        # drive-letter colon and would otherwise be split in half.
        parts = rest.split(":")
        grid, weight = 32, 0.5
        if len(parts) >= 3:
            try:
                grid, weight = int(parts[-2]), float(parts[-1])
                parts = parts[:-2]
            except ValueError:
                pass
        return HybridDescriptor(":".join(parts), grid, weight,
                                highpass=6 if kind in ("hybridhp", "hybrid2") else 0,
                                dual=kind == "hybrid2")

    raise SystemExit(f"Unknown descriptor spec {spec!r}. Use pixels:/ onnx:/ hybrid:")
