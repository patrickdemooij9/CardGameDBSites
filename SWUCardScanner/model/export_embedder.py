"""Export the card embedding model to ONNX.

Milestone 2 identifies a card by turning its un-warped image into a vector and finding
the nearest vector among all known cards. This script produces the model that makes
those vectors.

The same ONNX file is used on both sides - `build_index.py` embeds the reference art
with it in Python, and the browser embeds camera crops with it at run time. Using one
artifact for both is deliberate: any difference in preprocessing between the index and
the query would quietly wreck the matching, and there is no way to have a difference
when it is literally the same graph.

For that reason the ImageNet normalisation is baked *into* the graph. Callers hand it
RGB in [0,1] and get back an already L2-normalised embedding, so cosine similarity is
a plain dot product and nothing is left for a caller to get wrong.

Why MobileNetV3-Small: the query is already aligned to the reference by the un-warp, so
the embedding does not need rotation or scale invariance - it needs to survive lighting,
blur, glare and JPEG, plus the few percent of misalignment left by imperfect corner
predictions. Convolutional features pool over small spatial neighbourhoods, which is
exactly the tolerance that misalignment needs, and the model is small enough to ship.

Usage:
    python export_embedder.py                 # 576-d, global pooling
    python export_embedder.py --pool 2        # 2304-d, keeps 2x2 spatial layout
"""

from __future__ import annotations

import argparse
import os
import sys

import torch
import torch.nn as nn
import torch.nn.functional as F
from torchvision.models import MobileNet_V3_Small_Weights, mobilenet_v3_small

HERE = os.path.dirname(os.path.abspath(__file__))

IMAGENET_MEAN = (0.485, 0.456, 0.406)
IMAGENET_STD = (0.229, 0.224, 0.225)


class CardEmbedder(nn.Module):
    """MobileNetV3-Small trunk, pooled and L2-normalised.

    Input:  float32 [N, 3, size, size], RGB, values in [0, 1]
    Output: float32 [N, dim], unit length
    """

    def __init__(self, pool: int = 1, size: int = 224):
        super().__init__()
        backbone = mobilenet_v3_small(weights=MobileNet_V3_Small_Weights.IMAGENET1K_V1)
        self.features = backbone.features

        # The trunk downsamples by 32, and the pooling grid has to divide that evenly.
        # AdaptiveAvgPool2d only exports to ONNX cleanly when it collapses to 1x1 - for
        # anything coarser torch cannot express it as a static AveragePool and the export
        # fails - so the kernel is computed here instead.
        feature_size = size // 32
        if feature_size % pool != 0:
            raise SystemExit(
                f"--size {size} gives a {feature_size}x{feature_size} feature map, which "
                f"does not divide into a {pool}x{pool} grid. Try --size "
                f"{32 * pool * max(1, round(feature_size / pool))}."
            )
        kernel = feature_size // pool
        self.pool = nn.AvgPool2d(kernel_size=kernel, stride=kernel)

        self.register_buffer("mean", torch.tensor(IMAGENET_MEAN).view(1, 3, 1, 1))
        self.register_buffer("std", torch.tensor(IMAGENET_STD).view(1, 3, 1, 1))

    def forward(self, x: torch.Tensor) -> torch.Tensor:
        x = (x - self.mean) / self.std
        x = self.features(x)
        x = self.pool(x).flatten(1)
        return F.normalize(x, p=2, dim=1)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--out", default=None, help="output .onnx path")
    parser.add_argument("--size", type=int, default=224, help="square input resolution")
    parser.add_argument("--pool", type=int, default=1,
                        help="spatial pooling grid: 1 = global (576-d), 2 = 2x2 (2304-d)")
    parser.add_argument("--opset", type=int, default=12)
    args = parser.parse_args()

    out = args.out or os.path.join(HERE, f"card-embedder-s{args.size}-p{args.pool}.onnx")

    model = CardEmbedder(args.pool, args.size).eval()
    dummy = torch.rand(1, 3, args.size, args.size)

    with torch.no_grad():
        dim = model(dummy).shape[1]

    torch.onnx.export(
        model,
        dummy,
        out,
        input_names=["images"],
        output_names=["embedding"],
        # Batch stays dynamic so build_index.py can push many cards through at once
        # while the browser sends one crop at a time.
        dynamic_axes={"images": {0: "batch"}, "embedding": {0: "batch"}},
        opset_version=args.opset,
        do_constant_folding=True,
    )

    size_mb = os.path.getsize(out) / 1e6
    print(f"Wrote {out}")
    print(f"  input     [batch, 3, {args.size}, {args.size}]  RGB in [0,1]")
    print(f"  output    [batch, {dim}]  L2-normalised")
    print(f"  file size {size_mb:.1f} MB")

    # Verify the exported graph agrees with PyTorch. A silent divergence here would
    # produce an index that does not match what the browser computes.
    try:
        import numpy as np
        import onnxruntime as ort

        sess = ort.InferenceSession(out, providers=["CPUExecutionProvider"])
        probe = torch.rand(2, 3, args.size, args.size)
        with torch.no_grad():
            expected = model(probe).numpy()
        actual = sess.run(None, {"images": probe.numpy()})[0]

        drift = float(np.abs(expected - actual).max())
        norms = np.linalg.norm(actual, axis=1)
        print(f"  torch vs onnx max drift {drift:.2e}")
        print(f"  output norms {norms.min():.4f}..{norms.max():.4f} (want 1.0)")
        if drift > 1e-4:
            print("  WARNING: exported graph diverges from PyTorch")
    except ImportError:
        print("  (install onnxruntime to verify the export)")

    return 0


if __name__ == "__main__":
    sys.exit(main())
