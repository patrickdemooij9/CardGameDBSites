"""Build the card index the browser searches to identify a detection.

Every card becomes a unit-length vector; identification is then a dot product against
all of them and taking the best. The descriptor and its dimensionality were chosen by
measurement in compare_descriptors.py rather than by intuition - see the README.

Writes three files into the web app's public folder:

    card-index.bin     int8 [count x pcaDim], L2-normalised, row-major (see index_store.py)
    card-pca.bin       mean, whitening scale and int8 components (see index_store.py)
    card-index.json    per-card metadata plus the config the browser needs to agree

The embedder ONNX is copied in alongside them, so the index directory is self-contained
and there is no way to pair an index with the wrong model.

Usage:
    python build_index.py
    python build_index.py --pca 512 --pixel-grid 48
"""

from __future__ import annotations

import argparse
import json
import os
import random
import shutil
import sys

import cv2
import numpy as np
from tqdm import tqdm

from descriptors import HybridDescriptor, apply_pca, fit_pca
from index_store import STORAGE, write_index

HERE = os.path.dirname(os.path.abspath(__file__))
DEFAULT_OUT = os.path.abspath(os.path.join(HERE, "..", "web", "public", "index"))

# Reference art is RGBA; only the rounded corners are transparent. Compositing over a
# mid grey rather than white keeps those corners closer to what a camera sees, and
# matches the border colour the browser's un-warp uses outside the frame.
MATTE = 128


def augment_reference(img: np.ndarray, rng: random.Random) -> np.ndarray:
    """Perturb a reference scan toward what a camera would have produced.

    References are clean scans; queries are photographs. Embedding each card under a
    spread of plausible camera conditions and averaging gives a centroid that sits
    nearer to any particular photo than the pristine scan does.

    Deliberately *not* build_dataset.degrade(): if references were augmented with the
    same code that renders the evaluation queries, the benchmark would reward the index
    for memorising the generator rather than for genuine robustness. These are the same
    physical effects expressed differently - different parameterisation, different order,
    different implementation.
    """
    out = img.astype(np.float32)

    # White balance: a warm bulb or a cool screen shifts the channels apart.
    for c in range(3):
        out[:, :, c] *= rng.uniform(0.88, 1.12)

    # Exposure, as gamma rather than a linear scale.
    gamma = rng.uniform(0.72, 1.38)
    out = 255.0 * np.power(np.clip(out, 0, 255) / 255.0, gamma)

    # Residual misalignment left by imperfect corners, as a small affine nudge.
    h, w = out.shape[:2]
    m = cv2.getRotationMatrix2D((w / 2, h / 2), rng.uniform(-1.8, 1.8),
                                rng.uniform(0.975, 1.025))
    m[0, 2] += rng.uniform(-0.02, 0.02) * w
    m[1, 2] += rng.uniform(-0.02, 0.02) * h
    out = cv2.warpAffine(out, m, (w, h), flags=cv2.INTER_LINEAR,
                         borderMode=cv2.BORDER_REPLICATE)

    # A soft directional highlight, as if lit from one side.
    if rng.random() < 0.5:
        ax = np.linspace(0, 1, w, dtype=np.float32)[None, :]
        ay = np.linspace(0, 1, h, dtype=np.float32)[:, None]
        ramp = ax * rng.uniform(-1, 1) + ay * rng.uniform(-1, 1)
        ramp = (ramp - ramp.min()) / (np.ptp(ramp) + 1e-6)
        out += (ramp[..., None] * rng.uniform(12, 55))

    if rng.random() < 0.7:
        out = cv2.GaussianBlur(out, (0, 0), rng.uniform(0.3, 1.6))

    out = np.clip(out + np.random.normal(0, rng.uniform(1.5, 7.0), out.shape), 0, 255)
    out = out.astype(np.uint8)

    if rng.random() < 0.6:
        ok, buf = cv2.imencode(".jpg", out, [int(cv2.IMWRITE_JPEG_QUALITY),
                                             rng.randint(45, 92)])
        if ok:
            out = cv2.imdecode(buf, cv2.IMREAD_COLOR)

    return out


def load_card(path: str, size: int) -> np.ndarray | None:
    """Load a card PNG as BGR uint8, resized to size x size."""
    img = cv2.imread(path, cv2.IMREAD_UNCHANGED)
    if img is None:
        return None
    if img.ndim == 2:
        img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)
    if img.shape[2] == 4:
        alpha = img[:, :, 3:4].astype(np.float32) / 255.0
        img = (img[:, :, :3].astype(np.float32) * alpha + MATTE * (1 - alpha)).astype(np.uint8)
    return cv2.resize(img[:, :, :3], (size, size), interpolation=cv2.INTER_AREA)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--cards", default=os.path.join(HERE, "cards"))
    parser.add_argument("--manifest", default=os.path.join(HERE, "cards.json"))
    parser.add_argument("--embedder", default=os.path.join(HERE, "card-embedder-s256-p2.onnx"))
    parser.add_argument("--out", default=DEFAULT_OUT)
    parser.add_argument("--unwarp-size", type=int, default=256,
                        help="must match the embedder's input size")
    parser.add_argument("--pixel-grid", type=int, default=32)
    parser.add_argument("--pixel-weight", type=float, default=0.5)
    parser.add_argument("--highpass", type=int, default=0,
                        help="binomial-blur passes subtracted from the pixel grid, to "
                             "cancel glare gradients. Off by default: measured no gain "
                             "even on dark cards under glare, and it costs accuracy "
                             "when corner error is high. Kept as a knob for real-world "
                             "glare that the synthetic model may not capture.")
    parser.add_argument("--pca", type=int, default=256)
    parser.add_argument("--batch", type=int, default=32)
    parser.add_argument("--limit", type=int, default=None)
    parser.add_argument("--augment", type=int, default=0,
                        help="average each card's descriptor over N camera-like "
                             "perturbations plus the clean scan (0 = clean only)")
    parser.add_argument("--seed", type=int, default=11)
    args = parser.parse_args()

    for path, what in [(args.manifest, "scrape_cards.py"), (args.embedder, "export_embedder.py")]:
        if not os.path.exists(path):
            raise SystemExit(f"Missing {path}. Run {what} first.")

    with open(args.manifest, encoding="utf-8") as fh:
        manifest = [c for c in json.load(fh)
                    if os.path.exists(os.path.join(args.cards, c["file"]))]
    if args.limit:
        manifest = manifest[: args.limit]

    # Constructed directly rather than through a spec string: the embedder path is
    # absolute here, and on Windows its drive-letter colon collides with the spec syntax.
    desc = HybridDescriptor(args.embedder, args.pixel_grid, args.pixel_weight,
                            highpass=args.highpass)
    if desc.deep.size != args.unwarp_size:
        raise SystemExit(
            f"Embedder expects {desc.deep.size}px but --unwarp-size is {args.unwarp_size}. "
            "They must match so the browser never resamples twice."
        )

    print(f"{len(manifest)} cards, descriptor {desc.dim}-d -> PCA {args.pca}-d")

    rng = random.Random(args.seed)
    np.random.seed(args.seed)

    raw = np.empty((len(manifest), desc.dim), np.float32)
    entries: list[dict] = []
    batch: list[np.ndarray] = []
    at = 0

    def describe_batch(images: list[np.ndarray]) -> np.ndarray:
        """Descriptor per image, averaged over augmentations when enabled."""
        if args.augment <= 0:
            return desc(images)
        # Clean scan plus N perturbations, averaged into a centroid and renormalised.
        total = desc(images).astype(np.float64)
        for _ in range(args.augment):
            total += desc([augment_reference(im, rng) for im in images])
        total /= args.augment + 1
        norms = np.linalg.norm(total, axis=1, keepdims=True)
        return (total / np.maximum(norms, 1e-9)).astype(np.float32)

    label = f"Describing (x{args.augment + 1})" if args.augment else "Describing"
    for card in tqdm(manifest, desc=label, unit="card"):
        img = load_card(os.path.join(args.cards, card["file"]), args.unwarp_size)
        if img is None:
            continue
        batch.append(img)
        entries.append({
            "file": card.get("file"),
            "name": card.get("name"),
            "set": card.get("setName"),
            "type": card.get("cardType"),
            "face": card.get("face"),
            "orientation": card.get("orientation"),
            "baseId": card.get("baseId"),
            "variantIds": card.get("variantIds", []),
            "url": card.get("url"),
            "urlSegment": card.get("urlSegment"),
        })
        if len(batch) >= args.batch:
            raw[at:at + len(batch)] = describe_batch(batch)
            at += len(batch)
            batch = []
    if batch:
        raw[at:at + len(batch)] = describe_batch(batch)
        at += len(batch)

    raw = raw[:at]
    entries = entries[:at]

    print(f"Fitting PCA on {raw.shape[0]}x{raw.shape[1]}...")
    mean, components, scale = fit_pca(raw, args.pca, whiten=True)
    index = apply_pca(raw, mean, components, scale).astype(np.float32)

    norms = np.linalg.norm(index, axis=1)
    if not np.allclose(norms, 1.0, atol=1e-3):
        raise SystemExit(f"Index rows are not unit length: {norms.min()}..{norms.max()}")

    os.makedirs(args.out, exist_ok=True)
    write_index(args.out, index, mean, components, scale)

    shutil.copy(args.embedder, os.path.join(args.out, os.path.basename(args.embedder)))

    with open(os.path.join(args.out, "card-index.json"), "w", encoding="utf-8") as fh:
        json.dump({
            "config": {
                "dim": desc.dim,
                "count": int(index.shape[0]),
                "unwarpSize": args.unwarp_size,
                "pixelGrid": args.pixel_grid,
                "pixelWeight": args.pixel_weight,
                "highpass": args.highpass,
                "embedSize": desc.deep.size,
                "embedDim": int(desc.deep.dim),
                "pcaDim": int(index.shape[1]),
                "embedder": os.path.basename(args.embedder),
                "augment": args.augment,
                "storage": STORAGE,
            },
            "cards": entries,
        }, fh, ensure_ascii=False)

    total = sum(os.path.getsize(os.path.join(args.out, f)) for f in os.listdir(args.out))
    portrait = sum(1 for e in entries if e["orientation"] == "portrait")
    print(f"\nIndexed {index.shape[0]} cards "
          f"({portrait} portrait, {len(entries) - portrait} landscape)")
    for name in sorted(os.listdir(args.out)):
        size = os.path.getsize(os.path.join(args.out, name)) / 1e6
        print(f"  {name:<34} {size:6.2f} MB")
    print(f"  {'total':<34} {total / 1e6:6.2f} MB")
    return 0


if __name__ == "__main__":
    sys.exit(main())
