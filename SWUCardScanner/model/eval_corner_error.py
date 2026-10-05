"""Measure how accurate the detector's corners really are.

compare_descriptors.py reports identification accuracy as a function of corner error, so
the only thing left to know is where on that curve the real detector sits. This runs the
trained ONNX detector over freshly rendered scenes with known ground truth and reports
corner displacement as a percentage of card size - the same unit as the `--jitter`
sweep, so the two tables line up directly.

Usage:
    python eval_corner_error.py --scenes 120
"""

from __future__ import annotations

import argparse
import os
import random
import sys

import cv2
import numpy as np
import onnxruntime as ort
from tqdm import tqdm

import build_dataset as bd

HERE = os.path.dirname(os.path.abspath(__file__))
DEFAULT_MODEL = os.path.abspath(
    os.path.join(HERE, "..", "web", "public", "models", "swu-detect.onnx"))
PAD = 114


def letterbox(scene: np.ndarray, size: int) -> tuple[np.ndarray, float, float, float]:
    h, w = scene.shape[:2]
    scale = min(size / w, size / h)
    dw, dh = int(round(w * scale)), int(round(h * scale))
    canvas = np.full((size, size, 3), PAD, np.uint8)
    px, py = (size - dw) // 2, (size - dh) // 2
    canvas[py:py + dh, px:px + dw] = cv2.resize(scene, (dw, dh), interpolation=cv2.INTER_AREA)
    return canvas, scale, float(px), float(py)


def detect(session: ort.InferenceSession, scene: np.ndarray, size: int, thresh: float):
    canvas, scale, px, py = letterbox(scene, size)
    rgb = cv2.cvtColor(canvas, cv2.COLOR_BGR2RGB).astype(np.float32) / 255.0
    tensor = np.ascontiguousarray(rgb.transpose(2, 0, 1)[None])
    out = session.run(None, {session.get_inputs()[0].name: tensor})[0][0]  # [13, N]

    keep = out[4] >= thresh
    if not keep.any():
        return []
    picks = out[:, keep]
    order = np.argsort(-picks[4])
    results = []
    for i in order[:16]:
        corners = picks[5:13, i].reshape(4, 2)
        corners = (corners - np.array([px, py], np.float32)) / scale
        cx, cy, bw, bh = (picks[:4, i] - np.array([px, py, 0, 0], np.float32)) / scale
        results.append((float(picks[4, i]), np.array([cx - bw / 2, cy - bh / 2,
                                                      cx + bw / 2, cy + bh / 2]), corners))
    return results


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--cards", default=os.path.join(HERE, "cards"))
    parser.add_argument("--model", default=DEFAULT_MODEL)
    parser.add_argument("--scenes", type=int, default=120)
    parser.add_argument("--size", type=int, default=768)
    parser.add_argument("--conf", type=float, default=0.35)
    parser.add_argument("--seed", type=int, default=99)
    args = parser.parse_args()

    if not os.path.exists(args.model):
        raise SystemExit(f"No detector at {args.model}")

    session = ort.InferenceSession(args.model, providers=["CPUExecutionProvider"])
    pool = bd.CardPool(args.cards)
    rng = random.Random(args.seed)
    np.random.seed(args.seed)

    errors: list[float] = []
    found = 0
    total = 0

    for _ in tqdm(range(args.scenes), desc="Detecting", unit="scene"):
        scene = bd.procedural_background(args.size, rng)
        raw = pool.random()
        quad = None
        for attempt in range(4):
            quad = bd.random_quad(args.size, raw.shape[1], raw.shape[0],
                                  rng.choice(["scatter", "closeup"]), rng, shrink=0.82 ** attempt)
            if quad is not None:
                break
        if quad is None:
            continue
        target_h = float(np.linalg.norm(quad[3] - quad[0]))
        card, src = bd.prepare_card(raw, max(16, int(round(target_h * 1.15))), rng)
        bd.paste_card(scene, card, src, quad, rng)
        scene = bd.degrade(scene, rng)
        total += 1

        dets = detect(session, scene, args.size, args.conf)
        if not dets:
            continue

        # Pick the detection whose centre is closest to the true card.
        truth_c = quad.mean(axis=0)
        best = min(dets, key=lambda d: np.linalg.norm(d[2].mean(axis=0) - truth_c))
        extent = float(max(quad.max(0) - quad.min(0)))
        # Mean corner displacement, normalised by card size == the jitter unit.
        err = float(np.linalg.norm(best[2] - quad, axis=1).mean() / extent)
        if err < 0.5:  # anything larger is a different card, not a corner error
            errors.append(err)
            found += 1

    if not errors:
        raise SystemExit("No cards detected - check the model path and confidence.")

    e = np.array(errors)
    print(f"\n{found}/{total} cards detected at conf {args.conf}")
    print("\nCorner error as a fraction of card size (the --jitter unit):")
    for label, value in [("mean", e.mean()), ("median", np.median(e)),
                         ("p75", np.percentile(e, 75)), ("p90", np.percentile(e, 90)),
                         ("p95", np.percentile(e, 95)), ("worst", e.max())]:
        print(f"  {label:>7}: {value * 100:5.2f}%")
    print("\nCompare against the jitter column in compare_descriptors.py to read off the")
    print("identification accuracy you should actually expect.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
