"""Generate a synthetic YOLO-pose dataset for detecting SWU cards.

Cards are composited onto randomised backgrounds under random perspective, then the
whole scene gets photographic degradation (blur, noise, JPEG, colour cast) so the
model transfers to a real webcam feed.

The detector is deliberately *card-agnostic*: a single class, `card`. Identity comes
later from the cropped, un-warped art. That means the dataset does not need to cover
every card evenly - it needs visual variety. Each scene samples cards at random from
the whole pool.

Labels are YOLO-pose with `kpt_shape: [4, 2]`:

    0 cx cy w h  x1 y1  x2 y2  x3 y3  x4 y4      (all normalised to [0,1])

The four keypoints are the card's corners in the card's *own* frame, always ordered
top-left, top-right, bottom-right, bottom-left. That ordering is what lets the next
milestone un-warp a detection back to a flat, upright card image before matching. It
also means horizontal/vertical flip augmentation must stay off during training.

Usage:
    python build_dataset.py --scenes 8000
    python build_dataset.py --scenes 200 --out dataset_smoke     # quick check
"""

from __future__ import annotations

import argparse
import glob
import math
import multiprocessing as mp
import os
import random
import sys

import cv2
import numpy as np
from tqdm import tqdm

HERE = os.path.dirname(os.path.abspath(__file__))

# Fraction of a card that must remain unoccluded for it to keep its label.
MIN_VISIBLE_RATIO = 0.55
# How far outside the frame a corner may drift before the card is dropped. Corner
# regression is undefined for off-screen points, so we keep cards essentially inside.
CORNER_MARGIN_FRAC = 0.02


# --------------------------------------------------------------------------------------
# Card loading
# --------------------------------------------------------------------------------------

class CardPool:
    """Lazily-loaded, size-capped cache of RGBA card images."""

    def __init__(self, cards_dir: str, cache_size: int = 400):
        self.paths = sorted(
            glob.glob(os.path.join(cards_dir, "*.png"))
            + glob.glob(os.path.join(cards_dir, "*.webp"))
        )
        if not self.paths:
            raise SystemExit(
                f"No card images found in {cards_dir}. Run scrape_cards.py first."
            )
        self._cache: dict[str, np.ndarray] = {}
        self._cache_size = cache_size

    def __len__(self) -> int:
        return len(self.paths)

    def random(self) -> np.ndarray:
        path = random.choice(self.paths)
        img = self._cache.get(path)
        if img is None:
            img = cv2.imread(path, cv2.IMREAD_UNCHANGED)
            if img is None:
                # Corrupt download - drop it from the pool and try again.
                self.paths.remove(path)
                return self.random()
            if img.shape[2] == 3:
                img = cv2.cvtColor(img, cv2.COLOR_BGR2BGRA)
            if len(self._cache) >= self._cache_size:
                self._cache.pop(next(iter(self._cache)))
            self._cache[path] = img
        return img


# --------------------------------------------------------------------------------------
# Backgrounds
# --------------------------------------------------------------------------------------

def _noise_texture(size: int, scale: int, rng: random.Random) -> np.ndarray:
    """Cheap smooth noise field in [0,1], used for cloth / wood / mottling."""
    small = np.random.rand(max(2, size // scale), max(2, size // scale)).astype(np.float32)
    return cv2.resize(small, (size, size), interpolation=cv2.INTER_CUBIC)


def procedural_background(size: int, rng: random.Random) -> np.ndarray:
    kind = rng.choice(["gradient", "cloth", "wood", "desk", "mottled"])
    base = np.zeros((size, size, 3), np.float32)

    if kind == "gradient":
        c1 = np.array([rng.uniform(10, 230) for _ in range(3)], np.float32)
        c2 = np.array([rng.uniform(10, 230) for _ in range(3)], np.float32)
        line = np.linspace(0, 1, size, dtype=np.float32)
        ramp = np.tile(line[:, None], (1, size)) if rng.random() < 0.5 \
            else np.tile(line[None, :], (size, 1))
        base = c1 + (c2 - c1) * ramp[..., None]

    elif kind == "cloth":
        colour = np.array([rng.uniform(20, 160) for _ in range(3)], np.float32)
        weave = _noise_texture(size, rng.choice([2, 3, 4]), rng)
        base = colour * (0.75 + 0.5 * weave[..., None])

    elif kind == "wood":
        colour = np.array([rng.uniform(30, 90), rng.uniform(60, 130), rng.uniform(90, 180)],
                          np.float32)
        grain = _noise_texture(size, rng.choice([2, 3]), rng)
        streaks = np.sin(np.linspace(0, rng.uniform(20, 60), size))[:, None].astype(np.float32)
        base = colour * (0.8 + 0.25 * grain[..., None] + 0.12 * streaks[..., None])

    elif kind == "desk":
        colour = np.array([rng.uniform(150, 245) for _ in range(3)], np.float32)
        base = np.broadcast_to(colour, (size, size, 3)).copy()
        base += np.random.normal(0, 4, (size, size, 3)).astype(np.float32)

    else:  # mottled
        a = _noise_texture(size, rng.choice([8, 16, 32]), rng)
        c1 = np.array([rng.uniform(0, 200) for _ in range(3)], np.float32)
        c2 = np.array([rng.uniform(0, 200) for _ in range(3)], np.float32)
        base = c1 + (c2 - c1) * a[..., None]

    base = np.clip(base, 0, 255).astype(np.uint8)
    # Real tables and playmats are far more muted than a random RGB draw, so pull most
    # backgrounds part-way toward grey. A few stay vivid for the odd bright playmat.
    if rng.random() < 0.7:
        grey = cv2.cvtColor(cv2.cvtColor(base, cv2.COLOR_BGR2GRAY), cv2.COLOR_GRAY2BGR)
        t = rng.uniform(0.35, 0.85)
        base = cv2.addWeighted(base, 1 - t, grey, t, 0)
    return base


def real_background(paths: list[str], size: int, rng: random.Random) -> np.ndarray | None:
    for _ in range(3):
        img = cv2.imread(rng.choice(paths), cv2.IMREAD_COLOR)
        if img is None:
            continue
        h, w = img.shape[:2]
        side = min(h, w)
        if side < 64:
            continue
        crop = rng.randint(int(side * 0.5), side)
        x = rng.randint(0, w - crop)
        y = rng.randint(0, h - crop)
        return cv2.resize(img[y:y + crop, x:x + crop], (size, size),
                          interpolation=cv2.INTER_AREA)
    return None


def add_distractors(scene: np.ndarray, rng: random.Random) -> None:
    """Paint card-ish rectangles that are *not* cards.

    Without these the model learns "any bright quad is a card" and fires on phones,
    coasters and notebooks sitting on the table.
    """
    size = scene.shape[0]
    for _ in range(rng.randint(1, 3)):
        w = rng.randint(int(size * 0.08), int(size * 0.45))
        h = int(w * rng.uniform(0.4, 2.2))
        cx = rng.randint(0, size)
        cy = rng.randint(0, size)
        angle = rng.uniform(0, 360)
        box = cv2.boxPoints(((cx, cy), (w, h), angle)).astype(np.int32)
        colour = tuple(int(rng.uniform(0, 255)) for _ in range(3))
        cv2.fillConvexPoly(scene, box, colour, lineType=cv2.LINE_AA)
        if rng.random() < 0.5:
            cv2.polylines(scene, [box], True,
                          tuple(int(c * 0.6) for c in colour), rng.randint(1, 4),
                          cv2.LINE_AA)


# --------------------------------------------------------------------------------------
# Card rendering
# --------------------------------------------------------------------------------------

def rounded_mask(w: int, h: int, radius: int) -> np.ndarray:
    mask = np.zeros((h, w), np.uint8)
    radius = max(1, min(radius, min(w, h) // 2))
    cv2.rectangle(mask, (radius, 0), (w - radius, h), 255, -1)
    cv2.rectangle(mask, (0, radius), (w, h - radius), 255, -1)
    for cx, cy in ((radius, radius), (w - radius, radius),
                   (radius, h - radius), (w - radius, h - radius)):
        cv2.circle(mask, (cx, cy), radius, 255, -1)
    return mask


def prepare_card(card: np.ndarray, target_h: int, rng: random.Random
                 ) -> tuple[np.ndarray, np.ndarray]:
    """Resize a card and optionally sleeve it.

    Returns (rgba_image, card_corners_in_that_image). The corners describe the printed
    card, not the sleeve, so an occluding sleeve border never shifts the label.
    """
    h, w = card.shape[:2]
    scale = target_h / h
    cw, ch = max(8, int(round(w * scale))), max(8, int(round(h * scale)))
    card = cv2.resize(card, (cw, ch), interpolation=cv2.INTER_AREA)

    # Art-side glare: a soft diagonal highlight, as if under a ceiling light.
    if rng.random() < 0.45:
        gx = np.linspace(0, 1, cw, dtype=np.float32)[None, :]
        gy = np.linspace(0, 1, ch, dtype=np.float32)[:, None]
        band = gx * rng.choice([-1.0, 1.0]) + gy * rng.choice([-1.0, 1.0])
        band = (band - band.min()) / (np.ptp(band) + 1e-6)
        centre, width = rng.uniform(0.2, 0.8), rng.uniform(0.08, 0.35)
        glare = np.exp(-((band - centre) ** 2) / (2 * width ** 2)) * rng.uniform(40, 130)
        rgb = card[:, :, :3].astype(np.float32) + glare[..., None]
        card[:, :, :3] = np.clip(rgb, 0, 255).astype(np.uint8)

    corners = np.array([[0, 0], [cw, 0], [cw, ch], [0, ch]], np.float32)

    if rng.random() < 0.35:  # sleeve
        pad = max(2, int(round(ch * rng.uniform(0.015, 0.035))))
        sw, sh = cw + 2 * pad, ch + 2 * pad
        sleeve = np.zeros((sh, sw, 4), np.uint8)
        tint = rng.choice([(20, 20, 20), (60, 30, 30), (30, 50, 30), (40, 40, 80),
                           (150, 150, 150), (10, 10, 10)])
        sleeve[:, :, :3] = tint
        sleeve[:, :, 3] = rounded_mask(sw, sh, int(pad * 2.5))
        sleeve[pad:pad + ch, pad:pad + cw] = np.where(
            card[:, :, 3:4] > 0, card, sleeve[pad:pad + ch, pad:pad + cw]
        )
        card = sleeve
        corners = corners + np.float32([pad, pad])

    return card, corners


def random_quad(size: int, card_w: float, card_h: float, mode: str,
                rng: random.Random, shrink: float = 1.0) -> np.ndarray | None:
    """Pick a destination quad in scene space for a card of the given pixel size.

    `shrink` scales the card down on retry, for when a large rotated card cannot fit
    inside the frame at its first-choice size.
    """
    if mode == "closeup":
        target_h = size * rng.uniform(0.55, 0.92)
        tilt = rng.uniform(0.04, 0.16)
        rot = math.radians(rng.uniform(-25, 25))
    elif mode == "grid":
        target_h = size * rng.uniform(0.20, 0.30)
        tilt = rng.uniform(0.0, 0.04)
        rot = math.radians(rng.uniform(-6, 6))
    elif mode == "fan":
        target_h = size * rng.uniform(0.35, 0.55)
        tilt = rng.uniform(0.0, 0.07)
        rot = math.radians(rng.uniform(-45, 45))
    else:  # scatter
        target_h = size * rng.uniform(0.18, 0.55)
        tilt = rng.uniform(0.0, 0.12)
        rot = math.radians(rng.uniform(-180, 180))

    aspect = card_w / card_h
    hh = target_h * shrink / 2.0
    hw = hh * aspect

    quad = np.array([[-hw, -hh], [hw, -hh], [hw, hh], [-hw, hh]], np.float32)
    # Perspective: push corners around independently.
    quad += np.random.uniform(-tilt, tilt, (4, 2)).astype(np.float32) * np.float32([hw * 2, hh * 2])

    c, s = math.cos(rot), math.sin(rot)
    quad = quad @ np.array([[c, s], [-s, c]], np.float32)

    # Translate so the whole quad lands inside the frame. Corner regression is
    # undefined for off-screen points, so a card that cannot fit is rejected and the
    # caller retries at a smaller scale.
    lo = -quad.min(0)
    hi = size - 1 - quad.max(0)
    if (lo >= hi).any():
        return None
    quad += np.float32([rng.uniform(lo[0], hi[0]), rng.uniform(lo[1], hi[1])])
    return quad


def paste_card(scene: np.ndarray, card: np.ndarray, src_corners: np.ndarray,
               dst_quad: np.ndarray, rng: random.Random) -> tuple[np.ndarray, np.ndarray]:
    """Warp a card onto the scene. Returns (its alpha mask, its card corners in scene)."""
    size = scene.shape[0]
    ch, cw = card.shape[:2]
    src_full = np.array([[0, 0], [cw, 0], [cw, ch], [0, ch]], np.float32)

    # dst_quad describes the *card*; expand it to the full image (incl. sleeve padding)
    # by mapping through the homography that takes card corners to dst_quad.
    m_card = cv2.getPerspectiveTransform(src_corners, dst_quad)
    dst_full = cv2.perspectiveTransform(src_full.reshape(-1, 1, 2), m_card).reshape(-1, 2)

    m_full = cv2.getPerspectiveTransform(src_full, dst_full.astype(np.float32))
    warped = cv2.warpPerspective(card, m_full, (size, size), flags=cv2.INTER_LINEAR,
                                 borderValue=(0, 0, 0, 0))

    alpha = warped[:, :, 3].astype(np.float32) / 255.0

    if rng.random() < 0.6:  # contact shadow
        offset = rng.randint(2, 9)
        shadow = cv2.GaussianBlur(warped[:, :, 3], (0, 0), rng.uniform(3, 10))
        shadow = np.roll(np.roll(shadow, offset, axis=0), offset, axis=1)
        sa = (shadow.astype(np.float32) / 255.0 * rng.uniform(0.25, 0.6))[..., None]
        scene[:] = np.clip(scene.astype(np.float32) * (1 - sa), 0, 255).astype(np.uint8)

    a3 = alpha[..., None]
    scene[:] = np.clip(
        warped[:, :, :3].astype(np.float32) * a3 + scene.astype(np.float32) * (1 - a3),
        0, 255,
    ).astype(np.uint8)

    return (alpha > 0.5).astype(np.uint8), dst_quad


# --------------------------------------------------------------------------------------
# Whole-scene photographic degradation
# --------------------------------------------------------------------------------------

def degrade(scene: np.ndarray, rng: random.Random) -> np.ndarray:
    out = scene

    if rng.random() < 0.85:  # colour / exposure
        hsv = cv2.cvtColor(out, cv2.COLOR_BGR2HSV).astype(np.float32)
        hsv[:, :, 0] = (hsv[:, :, 0] + rng.uniform(-8, 8)) % 180
        hsv[:, :, 1] *= rng.uniform(0.55, 1.45)
        hsv[:, :, 2] *= rng.uniform(0.45, 1.45)
        out = cv2.cvtColor(np.clip(hsv, 0, 255).astype(np.uint8), cv2.COLOR_HSV2BGR)

    if rng.random() < 0.5:  # defocus or motion blur
        if rng.random() < 0.6:
            out = cv2.GaussianBlur(out, (0, 0), rng.uniform(0.4, 2.6))
        else:
            k = rng.choice([5, 7, 9, 11, 15])
            kernel = np.zeros((k, k), np.float32)
            if rng.random() < 0.5:
                kernel[k // 2, :] = 1.0
            else:
                kernel[:, k // 2] = 1.0
            m = cv2.getRotationMatrix2D((k / 2 - 0.5, k / 2 - 0.5), rng.uniform(0, 180), 1.0)
            kernel = cv2.warpAffine(kernel, m, (k, k))
            out = cv2.filter2D(out, -1, kernel / max(kernel.sum(), 1e-6))

    if rng.random() < 0.6:  # sensor noise
        out = np.clip(out.astype(np.float32)
                      + np.random.normal(0, rng.uniform(2, 12), out.shape), 0, 255
                      ).astype(np.uint8)

    if rng.random() < 0.4:  # vignette
        size = out.shape[0]
        yy, xx = np.mgrid[0:size, 0:size].astype(np.float32)
        r = np.sqrt((xx - size / 2) ** 2 + (yy - size / 2) ** 2) / (size / 2)
        v = np.clip(1.0 - rng.uniform(0.15, 0.55) * r ** 2, 0, 1)[..., None]
        out = np.clip(out.astype(np.float32) * v, 0, 255).astype(np.uint8)

    if rng.random() < 0.7:  # JPEG artefacts
        ok, buf = cv2.imencode(".jpg", out,
                               [int(cv2.IMWRITE_JPEG_QUALITY), rng.randint(35, 92)])
        if ok:
            out = cv2.imdecode(buf, cv2.IMREAD_COLOR)

    return out


# --------------------------------------------------------------------------------------
# Scene assembly
# --------------------------------------------------------------------------------------

def build_scene(pool: CardPool, size: int, bg_paths: list[str], rng: random.Random
                ) -> tuple[np.ndarray, list[np.ndarray]]:
    mode = rng.choices(["scatter", "closeup", "grid", "fan"],
                       weights=[0.45, 0.25, 0.18, 0.12])[0]

    scene = None
    if bg_paths and rng.random() < 0.55:
        scene = real_background(bg_paths, size, rng)
    if scene is None:
        scene = procedural_background(size, rng)
    if rng.random() < 0.35:
        add_distractors(scene, rng)

    n_cards = {"closeup": 1, "grid": rng.randint(4, 9), "fan": rng.randint(3, 6)}.get(
        mode, rng.randint(1, 5)
    )

    masks: list[np.ndarray] = []
    quads: list[np.ndarray] = []

    for _ in range(n_cards):
        raw = pool.random()
        quad = None
        for attempt in range(4):
            quad = random_quad(size, raw.shape[1], raw.shape[0], mode, rng,
                               shrink=0.82 ** attempt)
            if quad is not None:
                break
        if quad is None:
            continue
        target_h = float(np.linalg.norm(quad[3] - quad[0]))
        card, src_corners = prepare_card(raw, max(16, int(round(target_h * 1.15))), rng)
        mask, placed = paste_card(scene, card, src_corners, quad, rng)
        masks.append(mask)
        quads.append(placed)

    # Later cards occlude earlier ones; keep only those still mostly visible.
    labels: list[np.ndarray] = []
    margin = size * CORNER_MARGIN_FRAC
    for i, (mask, quad) in enumerate(zip(masks, quads)):
        area = int(mask.sum())
        if area < 200:
            continue
        occluded = np.zeros_like(mask)
        for later in masks[i + 1:]:
            occluded |= later
        visible = int((mask & ~occluded).sum())
        if visible / area < MIN_VISIBLE_RATIO:
            continue
        if (quad < -margin).any() or (quad > size + margin).any():
            continue
        labels.append(np.clip(quad, 0, size - 1))

    return degrade(scene, rng), labels


def to_label_line(quad: np.ndarray, size: int) -> str:
    q = quad / size
    x0, y0 = q.min(0)
    x1, y1 = q.max(0)
    cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    w, h = x1 - x0, y1 - y0
    parts = [f"{v:.6f}" for v in (cx, cy, w, h)]
    parts += [f"{v:.6f}" for pt in q for v in pt]
    return "0 " + " ".join(parts)


# --------------------------------------------------------------------------------------
# Worker plumbing (scene rendering is CPU-bound, so it fans out over processes)
# --------------------------------------------------------------------------------------

_W: dict = {}


def _worker_init(cards_dir: str, bg_paths: list[str], out: str, size: int,
                 n_val: int, seed: int) -> None:
    _W["pool"] = CardPool(cards_dir)
    _W["bg"] = bg_paths
    _W["out"] = out
    _W["size"] = size
    _W["n_val"] = n_val
    _W["seed"] = seed


def _render_index(i: int) -> int:
    # Seed per scene, not per worker, so output is reproducible regardless of how the
    # work happens to be distributed across processes.
    rng = random.Random(_W["seed"] * 1_000_003 + i)
    np.random.seed((_W["seed"] * 1_000_003 + i) % (2 ** 32 - 1))

    size = _W["size"]
    scene, labels = build_scene(_W["pool"], size, _W["bg"], rng)
    split = "val" if i < _W["n_val"] else "train"
    stem = f"scene_{i:06d}"

    cv2.imwrite(os.path.join(_W["out"], "images", split, stem + ".jpg"), scene,
                [int(cv2.IMWRITE_JPEG_QUALITY), 92])
    with open(os.path.join(_W["out"], "labels", split, stem + ".txt"), "w") as fh:
        fh.write("\n".join(to_label_line(q, size) for q in labels))
    return len(labels)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--cards", default=os.path.join(HERE, "cards"))
    parser.add_argument("--backgrounds", default=os.path.join(HERE, "backgrounds"),
                        help="optional folder of real photos to use as backgrounds")
    parser.add_argument("--out", default=os.path.join(HERE, "dataset"))
    parser.add_argument("--scenes", type=int, default=8000, help="total images to generate")
    parser.add_argument("--size", type=int, default=768, help="scene resolution (= imgsz)")
    parser.add_argument("--val-frac", type=float, default=0.1)
    parser.add_argument("--seed", type=int, default=1337)
    parser.add_argument("--workers", type=int, default=0,
                        help="render processes (0 = one per CPU core, 1 = no fan-out)")
    args = parser.parse_args()

    pool = CardPool(args.cards)
    bg_paths: list[str] = []
    if os.path.isdir(args.backgrounds):
        for ext in ("*.jpg", "*.jpeg", "*.png", "*.webp"):
            bg_paths += glob.glob(os.path.join(args.backgrounds, ext))
    print(f"{len(pool)} card images, {len(bg_paths)} real backgrounds")

    for split in ("train", "val"):
        os.makedirs(os.path.join(args.out, "images", split), exist_ok=True)
        os.makedirs(os.path.join(args.out, "labels", split), exist_ok=True)

    n_val = int(args.scenes * args.val_frac)
    empty = 0
    total_cards = 0

    init_args = (args.cards, bg_paths, args.out, args.size, n_val, args.seed)
    workers = args.workers if args.workers > 0 else (os.cpu_count() or 1)

    if workers > 1:
        ctx = mp.get_context("spawn")
        with ctx.Pool(workers, initializer=_worker_init, initargs=init_args) as pool_proc:
            for n in tqdm(pool_proc.imap_unordered(_render_index, range(args.scenes),
                                                   chunksize=16),
                          total=args.scenes, desc="Rendering scenes", unit="img"):
                total_cards += n
                empty += n == 0
    else:
        _worker_init(*init_args)
        for i in tqdm(range(args.scenes), desc="Rendering scenes", unit="img"):
            n = _render_index(i)
            total_cards += n
            empty += n == 0

    data_yaml = os.path.join(args.out, "data.yaml")
    with open(data_yaml, "w") as fh:
        fh.write(
            f"path: {os.path.abspath(args.out)}\n"
            "train: images/train\n"
            "val: images/val\n"
            "kpt_shape: [4, 2]\n"
            # Only consulted if fliplr > 0. Training keeps flips off so corner order
            # stays meaningful, but a correct mapping here avoids silent corruption.
            "flip_idx: [1, 0, 3, 2]\n"
            "names:\n"
            "  0: card\n"
        )

    print(f"\nWrote {args.scenes} scenes ({total_cards} card labels, "
          f"{total_cards / max(args.scenes, 1):.1f} per scene, {empty} empty) to {args.out}")
    print(f"data.yaml -> {data_yaml}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
