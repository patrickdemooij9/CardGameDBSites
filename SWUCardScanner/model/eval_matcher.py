"""Measure identification accuracy against the built index.

Takes cards, renders them the way a camera would see them (perspective, glare, blur,
noise, JPEG, sleeves), un-warps them back the way the app will, describes them, searches
the index, and reports how often the right card comes first.

It also simulates an *imperfect* detector. Corner predictions are never exact, so
`--jitter` displaces each corner by a percentage of the card size before un-warping.
Accuracy at zero jitter says whether the descriptor can do the job at all; accuracy at
the level eval_corner_error.py reports says what to expect in practice.

Correctness and margin are both judged by `baseId`, not by index row. Nearly half the
index shares a baseId with another row because promos and alternate printings reuse
artwork, and calling those a miss - or worse, rejecting them for being too similar to
each other - measures the wrong thing.

Usage:
    python eval_matcher.py --queries 300
    python eval_matcher.py --queries 300 --sweep
"""

from __future__ import annotations

import argparse
import json
import os
import random
import sys

import cv2
import numpy as np
from tqdm import tqdm

import build_dataset as bd
from build_index import MATTE
from descriptors import HybridDescriptor, apply_pca
from index_store import read_index

HERE = os.path.dirname(os.path.abspath(__file__))
DEFAULT_INDEX = os.path.abspath(os.path.join(HERE, "..", "web", "public", "index"))


def render_query(card_bgra: np.ndarray, rng: random.Random, scene_size: int = 768
                 ) -> tuple[np.ndarray, np.ndarray] | None:
    """Render one card as a degraded 'photo'. Returns (scene_bgr, true_corners)."""
    scene = bd.procedural_background(scene_size, rng)
    if rng.random() < 0.3:
        bd.add_distractors(scene, rng)

    quad = None
    for attempt in range(4):
        quad = bd.random_quad(scene_size, card_bgra.shape[1], card_bgra.shape[0],
                              rng.choice(["scatter", "closeup"]), rng, shrink=0.82 ** attempt)
        if quad is not None:
            break
    if quad is None:
        return None

    target_h = float(np.linalg.norm(quad[3] - quad[0]))
    card, src_corners = bd.prepare_card(card_bgra, max(16, int(round(target_h * 1.15))), rng)
    bd.paste_card(scene, card, src_corners, quad, rng)
    return bd.degrade(scene, rng), quad


def unwarp(scene: np.ndarray, corners: np.ndarray, size: int) -> np.ndarray:
    """Flatten a detected quad back to a square card image, mirroring the web app."""
    dst = np.array([[0, 0], [size, 0], [size, size], [0, size]], np.float32)
    m = cv2.getPerspectiveTransform(corners.astype(np.float32), dst)
    return cv2.warpPerspective(scene, m, (size, size), flags=cv2.INTER_LINEAR,
                               borderValue=(MATTE, MATTE, MATTE))


def jitter_corners(corners: np.ndarray, fraction: float, rng: random.Random) -> np.ndarray:
    if fraction <= 0:
        return corners
    extent = max(corners.max(0) - corners.min(0))
    noise = np.array([[rng.gauss(0, fraction * extent) for _ in range(2)] for _ in range(4)],
                     np.float32)
    return corners + noise


def add_glare(img: np.ndarray, rng: random.Random, amp: float) -> np.ndarray:
    """Additive low-frequency light ramp - reflection off a glossy card.

    Additive is the point: the same +40 is a fifth of a bright card's value and several
    times a dark card's, which is why dark cards fail first under a lamp.
    """
    h, w = img.shape[:2]
    yy, xx = np.mgrid[0:h, 0:w].astype(np.float32)
    angle = rng.uniform(0, 2 * np.pi)
    ramp = xx * np.cos(angle) + yy * np.sin(angle)
    ramp = (ramp - ramp.min()) / (np.ptp(ramp) + 1e-6)
    ramp = ramp ** rng.uniform(0.6, 2.0)
    tint = np.array([rng.uniform(0.8, 1.2) for _ in range(3)], np.float32)
    return np.clip(img.astype(np.float32) + ramp[..., None] * amp * tint,
                   0, 255).astype(np.uint8)


def evaluate(jitter: float, queries, desc, pca, index, base_ids, orientations,
             size: int, seed: int, glare: float = 0.0) -> dict:
    rng = random.Random(seed)
    crops = [unwarp(scene, jitter_corners(corners, jitter, rng), size)
             for scene, corners, _, _ in queries]
    if glare > 0:
        grng = random.Random(seed + 1)
        crops = [add_glare(c, grng, glare) for c in crops]
    truth_base = np.array([q[2] for q in queries])
    query_orient = np.array([q[3] for q in queries])

    vectors = np.concatenate([apply_pca(desc(crops[i:i + 32]), *pca)
                              for i in range(0, len(crops), 32)])
    sims = vectors @ index.T
    for row, want in enumerate(query_orient):
        sims[row, orientations != want] = -2.0

    order = np.argsort(-sims, axis=1)
    ranked_base = base_ids[order]

    ranks = np.empty(len(queries), int)
    margins = np.empty(len(queries), np.float32)
    tops = np.empty(len(queries), np.float32)

    for i in range(len(queries)):
        # Rank of the correct *card*, counting each card once however many rows it owns.
        seen, rank = set(), -1
        for pos, bid in enumerate(ranked_base[i]):
            if bid in seen:
                continue
            seen.add(bid)
            if bid == truth_base[i]:
                rank = len(seen) - 1
                break
        ranks[i] = rank if rank >= 0 else len(seen)

        best_row = order[i, 0]
        tops[i] = sims[i, best_row]
        rival = next((sims[i, r] for r in order[i, 1:] if base_ids[r] != base_ids[best_row]), 0.0)
        margins[i] = tops[i] - rival

    ok = ranks == 0
    return {
        "jitter": jitter,
        "top1": float(ok.mean()),
        "top5": float((ranks < 5).mean()),
        "mean_top_sim": float(tops.mean()),
        "margin_ok": float(margins[ok].mean()) if ok.any() else 0.0,
        "margin_bad": float(margins[~ok].mean()) if (~ok).any() else 0.0,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--cards", default=os.path.join(HERE, "cards"))
    parser.add_argument("--index", default=DEFAULT_INDEX)
    parser.add_argument("--queries", type=int, default=300)
    parser.add_argument("--jitter", type=float, default=0.0)
    parser.add_argument("--sweep", action="store_true")
    parser.add_argument("--glare", type=float, default=0.0,
                        help="veiling glare amplitude added to queries (0-80)")
    parser.add_argument("--darkest", type=float, default=0.0,
                        help="restrict queries to the darkest fraction of cards, e.g. "
                             "0.25. Glare is additive, so dark cards fail first and an "
                             "average over mostly-bright cards hides the effect.")
    parser.add_argument("--seed", type=int, default=7)
    args = parser.parse_args()

    meta, cfg, index, pca = read_index(args.index)
    cards = meta["cards"]
    base_ids = np.array([c["baseId"] for c in cards])
    orientations = np.array([c["orientation"] for c in cards])
    print(f"Index: {cfg['count']} rows, {len(set(base_ids.tolist()))} distinct cards, "
          f"{cfg['pcaDim']}-d")

    desc = HybridDescriptor(os.path.join(args.index, cfg["embedder"]),
                            cfg["pixelGrid"], cfg["pixelWeight"],
                            highpass=cfg.get("highpass", 0))

    rng = random.Random(args.seed)
    np.random.seed(args.seed)

    pool = list(range(cfg["count"]))
    if args.darkest > 0:
        lum = []
        for i in pool:
            im = cv2.imread(os.path.join(args.cards, cards[i]["file"]), cv2.IMREAD_COLOR)
            lum.append(float(im.mean()) if im is not None else 255.0)
        keep = int(len(pool) * args.darkest)
        pool = [i for _, i in sorted(zip(lum, pool))[:keep]]
        print(f"restricted to the {len(pool)} darkest cards "
              f"(mean luminance <= {sorted(lum)[keep - 1]:.1f})")

    queries = []
    with tqdm(total=args.queries, desc="Rendering queries", unit="q") as bar:
        guard = 0
        while len(queries) < args.queries and guard < args.queries * 6:
            guard += 1
            row = rng.choice(pool)
            path = os.path.join(args.cards, cards[row]["file"])
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
            queries.append((out[0], out[1], cards[row]["baseId"], cards[row]["orientation"]))
            bar.update(1)

    if not queries:
        raise SystemExit("Could not render any queries - does cards/ match the index?")

    levels = [0.0, 0.01, 0.02, 0.04, 0.06] if args.sweep else [args.jitter]
    rows = [evaluate(j, queries, desc, pca, index, base_ids, orientations,
                     cfg["unwarpSize"], args.seed, args.glare) for j in levels]

    print(f"\n{len(queries)} queries, scored by card (not index row)\n")
    print(f"{'jitter':>8} {'top-1':>8} {'top-5':>8} {'top sim':>9} "
          f"{'margin ok':>10} {'margin bad':>11}")
    for r in rows:
        print(f"{r['jitter'] * 100:7.0f}% {r['top1'] * 100:7.1f}% {r['top5'] * 100:7.1f}% "
              f"{r['mean_top_sim']:9.3f} {r['margin_ok']:10.3f} {r['margin_bad']:11.3f}")
    print("\nmargin is measured against the best *different* card, so a promo and its "
          "standard\nprinting no longer suppress each other.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
