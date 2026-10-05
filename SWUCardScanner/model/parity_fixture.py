"""Generate a fixture for checking that the browser agrees with Python.

The index is built in Python and searched in JavaScript. If the two descriptor
implementations diverge even slightly - a different downscale filter, a different blur
border, swapped channels - matching degrades in a way that looks like "the model is
bad" rather than "the code is wrong", which is a miserable thing to debug.

So: render scenes here, record Python's answer for each, and let the browser run the
same images through its own pipeline and compare.

Writes web/public/__parity/{scene_N.png, parity.json}. Delete the folder when done.

Usage:
    python parity_fixture.py --scenes 8
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

import build_dataset as bd
from build_index import MATTE
from descriptors import HybridDescriptor, apply_pca
from eval_matcher import render_query, unwarp
from index_store import read_index

HERE = os.path.dirname(os.path.abspath(__file__))
INDEX_DIR = os.path.abspath(os.path.join(HERE, "..", "web", "public", "index"))
OUT_DIR = os.path.abspath(os.path.join(HERE, "..", "web", "public", "__parity"))


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--cards", default=os.path.join(HERE, "cards"))
    parser.add_argument("--scenes", type=int, default=8)
    parser.add_argument("--name", default=None,
                        help="only render cards whose name contains this (case-insensitive)")
    parser.add_argument("--seed", type=int, default=4242)
    args = parser.parse_args()

    meta, cfg, index, (mean, components, scale) = read_index(INDEX_DIR)
    orientations = np.array([c["orientation"] for c in meta["cards"]])

    desc = HybridDescriptor(os.path.join(INDEX_DIR, cfg["embedder"]),
                            cfg["pixelGrid"], cfg["pixelWeight"])

    if os.path.isdir(OUT_DIR):
        shutil.rmtree(OUT_DIR)
    os.makedirs(OUT_DIR)

    rng = random.Random(args.seed)
    np.random.seed(args.seed)
    cases = []

    pool = [i for i, c in enumerate(meta["cards"])
            if not args.name or (c["name"] and args.name.lower() in c["name"].lower())]
    if not pool:
        raise SystemExit(f"No cards matching {args.name!r}")

    while len(cases) < args.scenes:
        row = rng.choice(pool)
        path = os.path.join(args.cards, meta["cards"][row]["file"])
        if not os.path.exists(path):
            continue
        card = cv2.imread(path, cv2.IMREAD_UNCHANGED)
        if card is None:
            continue
        if card.shape[2] == 3:
            card = cv2.cvtColor(card, cv2.COLOR_BGR2BGRA)
        out = render_query(card, rng)
        if out is None:
            continue
        scene, corners = out

        crop = unwarp(scene, corners, cfg["unwarpSize"])
        vector = apply_pca(desc([crop]), mean, components, scale)[0]

        want = meta["cards"][row]["orientation"]
        sims = index @ vector
        sims[orientations != want] = -2.0
        order = np.argsort(-sims)

        name = f"scene_{len(cases)}.png"
        cv2.imwrite(os.path.join(OUT_DIR, name), scene)
        cases.append({
            "image": name,
            "corners": corners.tolist(),
            "truthFile": meta["cards"][row]["file"],
            "truthName": meta["cards"][row]["name"],
            "python": {
                "matchFile": meta["cards"][int(order[0])]["file"],
                "matchName": meta["cards"][int(order[0])]["name"],
                "score": float(sims[order[0]]),
                "margin": float(sims[order[0]] - sims[order[1]]),
                # A few vector entries make a mismatch obvious before the match flips.
                "vectorHead": [float(v) for v in vector[:8]],
            },
        })

    with open(os.path.join(OUT_DIR, "parity.json"), "w", encoding="utf-8") as fh:
        json.dump({"config": cfg, "cases": cases}, fh, indent=1)

    correct = sum(1 for c in cases if c["python"]["matchFile"] == c["truthFile"])
    print(f"Wrote {len(cases)} fixtures to {OUT_DIR}")
    print(f"Python got {correct}/{len(cases)} right")
    for c in cases:
        ok = "ok " if c["python"]["matchFile"] == c["truthFile"] else "MISS"
        print(f"  {ok} {c['truthName'][:38]:<38} -> {c['python']['matchName'][:34]:<34} "
              f"{c['python']['score']:.3f}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
