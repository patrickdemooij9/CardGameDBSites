"""Draw the generated YOLO-pose labels back onto the scenes, as a sanity check.

Corners are colour-coded so you can confirm the ordering is consistent:
    top-left = red, top-right = green, bottom-right = blue, bottom-left = yellow.
If those colours ever appear in a different rotational order between two cards, the
corner ordering is broken and the un-warp step in the next milestone will misbehave.

Usage:
    python preview_dataset.py --n 24 --out preview.jpg
"""

from __future__ import annotations

import argparse
import glob
import os
import random
import sys

import cv2
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
CORNER_COLOURS = [(0, 0, 255), (0, 255, 0), (255, 0, 0), (0, 255, 255)]
CORNER_NAMES = ["TL", "TR", "BR", "BL"]


def draw(img_path: str, label_path: str) -> np.ndarray:
    img = cv2.imread(img_path)
    h, w = img.shape[:2]
    if not os.path.exists(label_path):
        return img

    with open(label_path) as fh:
        for line in fh:
            parts = line.split()
            if len(parts) != 13:
                continue
            vals = [float(v) for v in parts[1:]]
            cx, cy, bw, bh = vals[:4]
            pts = np.array(vals[4:], np.float32).reshape(4, 2) * [w, h]

            x0, y0 = int((cx - bw / 2) * w), int((cy - bh / 2) * h)
            x1, y1 = int((cx + bw / 2) * w), int((cy + bh / 2) * h)
            cv2.rectangle(img, (x0, y0), (x1, y1), (255, 255, 255), 1)
            cv2.polylines(img, [pts.astype(np.int32)], True, (255, 0, 255), 2, cv2.LINE_AA)
            for pt, colour, name in zip(pts, CORNER_COLOURS, CORNER_NAMES):
                cv2.circle(img, tuple(pt.astype(int)), 6, colour, -1, cv2.LINE_AA)
                cv2.putText(img, name, tuple((pt + 8).astype(int)),
                            cv2.FONT_HERSHEY_SIMPLEX, 0.45, colour, 1, cv2.LINE_AA)
    return img


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--dataset", default=os.path.join(HERE, "dataset"))
    parser.add_argument("--split", default="train", choices=["train", "val"])
    parser.add_argument("--n", type=int, default=12, help="how many scenes to show")
    parser.add_argument("--cols", type=int, default=4)
    parser.add_argument("--tile", type=int, default=420, help="px per tile in the contact sheet")
    parser.add_argument("--out", default=os.path.join(HERE, "preview.jpg"))
    parser.add_argument("--seed", type=int, default=None)
    args = parser.parse_args()

    images = sorted(glob.glob(os.path.join(args.dataset, "images", args.split, "*.jpg")))
    if not images:
        raise SystemExit(f"No images in {args.dataset}/images/{args.split}. "
                         "Run build_dataset.py first.")

    rng = random.Random(args.seed)
    picks = rng.sample(images, min(args.n, len(images)))

    tiles = []
    for img_path in picks:
        stem = os.path.splitext(os.path.basename(img_path))[0]
        label_path = os.path.join(args.dataset, "labels", args.split, stem + ".txt")
        tile = draw(img_path, label_path)
        tiles.append(cv2.resize(tile, (args.tile, args.tile), interpolation=cv2.INTER_AREA))

    cols = max(1, args.cols)
    rows = (len(tiles) + cols - 1) // cols
    blank = np.zeros((args.tile, args.tile, 3), np.uint8)
    tiles += [blank] * (rows * cols - len(tiles))

    sheet = np.vstack([np.hstack(tiles[r * cols:(r + 1) * cols]) for r in range(rows)])
    cv2.imwrite(args.out, sheet, [int(cv2.IMWRITE_JPEG_QUALITY), 90])
    print(f"Wrote {args.out} ({len(picks)} scenes from {args.split})")
    return 0


if __name__ == "__main__":
    sys.exit(main())
