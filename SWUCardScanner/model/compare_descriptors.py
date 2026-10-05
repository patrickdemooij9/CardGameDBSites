"""Bench several descriptors against the same queries, to pick one on evidence.

Renders a single set of degraded card 'photos', then scores every descriptor against
the identical queries and the identical corner jitter. Differences in the table are
therefore attributable to the descriptor alone.

Usage:
    python compare_descriptors.py --queries 300 \
        --spec pixels:32 --spec onnx:card-embedder-s256-p2.onnx \
        --spec hybrid:card-embedder-s256-p2.onnx:32:0.5
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
from descriptors import apply_pca, fit_pca, make_descriptor
from eval_matcher import jitter_corners, render_query, unwarp

HERE = os.path.dirname(os.path.abspath(__file__))
UNWARP_SIZE = 256


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--cards", default=os.path.join(HERE, "cards"))
    parser.add_argument("--manifest", default=os.path.join(HERE, "cards.json"))
    parser.add_argument("--spec", action="append", required=True,
                        help="descriptor spec; repeat to compare several")
    parser.add_argument("--queries", type=int, default=300)
    parser.add_argument("--jitters", default="0,0.02,0.04")
    parser.add_argument("--pca", type=int, default=0,
                        help="project down to this many dimensions (0 = off)")
    parser.add_argument("--no-whiten", action="store_true")
    parser.add_argument("--glare", type=float, default=0.0,
                        help="add veiling glare to queries: a low-frequency light ramp "
                             "of this amplitude (0-80), as reflected off a glossy card")
    parser.add_argument("--seed", type=int, default=7)
    args = parser.parse_args()

    with open(args.manifest, encoding="utf-8") as fh:
        manifest = [c for c in json.load(fh)
                    if os.path.exists(os.path.join(args.cards, c["file"]))]
    orientations = np.array([c["orientation"] for c in manifest])
    print(f"{len(manifest)} reference cards")

    # ---- queries, rendered once and shared by every descriptor ----
    rng = random.Random(args.seed)
    np.random.seed(args.seed)
    queries = []
    with tqdm(total=args.queries, desc="Rendering queries", unit="q") as bar:
        guard = 0
        while len(queries) < args.queries and guard < args.queries * 6:
            guard += 1
            row = rng.randrange(len(manifest))
            card = cv2.imread(os.path.join(args.cards, manifest[row]["file"]),
                              cv2.IMREAD_UNCHANGED)
            if card is None:
                continue
            if card.shape[2] == 3:
                card = cv2.cvtColor(card, cv2.COLOR_BGR2BGRA)
            out = render_query(card, rng)
            if out is None:
                continue
            queries.append((out[0], out[1], row))
            bar.update(1)

    jitters = [float(j) for j in args.jitters.split(",")]
    truths = np.array([q[2] for q in queries])
    query_orient = orientations[truths]

    def add_glare(img, rng, amp):
        """Additive low-frequency light ramp, the signature of reflection off gloss."""
        h, w = img.shape[:2]
        yy, xx = np.mgrid[0:h, 0:w].astype(np.float32)
        angle = rng.uniform(0, 2 * np.pi)
        ramp = xx * np.cos(angle) + yy * np.sin(angle)
        ramp = (ramp - ramp.min()) / (np.ptp(ramp) + 1e-6)
        ramp = ramp ** rng.uniform(0.6, 2.0)
        tint = np.array([rng.uniform(0.8, 1.2) for _ in range(3)], np.float32)
        return np.clip(img.astype(np.float32) + ramp[..., None] * amp * tint,
                       0, 255).astype(np.uint8)

    # Crops are produced once per jitter level so every descriptor sees identical pixels.
    crops_by_jitter = {}
    for j in jitters:
        jrng = random.Random(args.seed)
        crops = [unwarp(scene, jitter_corners(corners, j, jrng), UNWARP_SIZE)
                 for scene, corners, _ in queries]
        if args.glare > 0:
            grng = random.Random(args.seed + 1)
            crops = [add_glare(c, grng, args.glare) for c in crops]
        crops_by_jitter[j] = crops

    rows = []
    for spec in args.spec:
        desc = make_descriptor(spec)

        refs = np.empty((len(manifest), desc.dim), np.float32)
        batch, at = [], 0
        for card in tqdm(manifest, desc=f"Encoding refs [{spec}]", unit="card", leave=False):
            img = cv2.imread(os.path.join(args.cards, card["file"]), cv2.IMREAD_UNCHANGED)
            if img.shape[2] == 4:
                a = img[:, :, 3:4].astype(np.float32) / 255.0
                img = (img[:, :, :3].astype(np.float32) * a + 128 * (1 - a)).astype(np.uint8)
            batch.append(cv2.resize(img, (UNWARP_SIZE, UNWARP_SIZE), interpolation=cv2.INTER_AREA))
            if len(batch) == 32:
                refs[at:at + len(batch)] = desc(batch)
                at += len(batch)
                batch = []
        if batch:
            refs[at:at + len(batch)] = desc(batch)

        pca = None
        label = spec
        if args.pca:
            pca = fit_pca(refs, args.pca, whiten=not args.no_whiten)
            refs = apply_pca(refs, *pca)
            label = f"{spec} +pca{args.pca}{'' if not args.no_whiten else ' nowhiten'}"

        for j in jitters:
            crops = crops_by_jitter[j]
            vecs = np.concatenate([desc(crops[i:i + 32]) for i in range(0, len(crops), 32)])
            if pca is not None:
                vecs = apply_pca(vecs, *pca)
            sims = vecs @ refs.T
            for r, want in enumerate(query_orient):
                sims[r, orientations != want] = -2.0

            order = np.argsort(-sims, axis=1)
            ranks = np.array([int(np.where(order[i] == truths[i])[0][0])
                              for i in range(len(truths))])
            best = sims[np.arange(len(truths)), order[:, 0]]
            second = sims[np.arange(len(truths)), order[:, 1]]
            ok = ranks == 0
            rows.append({
                "spec": label, "dim": refs.shape[1], "jitter": j,
                "top1": float(ok.mean()), "top5": float((ranks < 5).mean()),
                "m_ok": float((best - second)[ok].mean()) if ok.any() else 0.0,
                "m_bad": float((best - second)[~ok].mean()) if (~ok).any() else 0.0,
            })

    print(f"\n{len(queries)} queries vs {len(manifest)} cards, orientation filter on\n")
    print(f"{'descriptor':<44} {'dim':>6} {'jitter':>7} {'top-1':>7} {'top-5':>7} "
          f"{'m ok':>7} {'m bad':>7}")
    for r in rows:
        print(f"{r['spec']:<44} {r['dim']:>6} {r['jitter']*100:6.0f}% "
              f"{r['top1']*100:6.1f}% {r['top5']*100:6.1f}% "
              f"{r['m_ok']:7.3f} {r['m_bad']:7.3f}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
