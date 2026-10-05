"""Read and write the int8 index files the browser loads.

    card-index.bin   float32 rowScale[count], then int8 rows[count x pcaDim]
    card-pca.bin     float32 mean[rawDim], float32 scale[pcaDim], then int8 components[pcaDim x rawDim]

Each row is quantised against its own max. The components' quantisation scale is folded
into the whitening scale, since both multiply the same projected value.
"""

from __future__ import annotations

import json
import os

import numpy as np

STORAGE = "int8"


def _quantise_rows(matrix: np.ndarray) -> tuple[np.ndarray, np.ndarray]:
    scale = np.abs(matrix).max(axis=1) / 127.0
    scale[scale == 0] = 1.0
    q = np.round(matrix / scale[:, None]).clip(-127, 127).astype(np.int8)
    return q, scale.astype(np.float32)


def write_index(out: str, index: np.ndarray, mean: np.ndarray, components: np.ndarray,
                scale: np.ndarray) -> None:
    rows, row_scale = _quantise_rows(index)
    with open(os.path.join(out, "card-index.bin"), "wb") as fh:
        fh.write(row_scale.tobytes())
        fh.write(rows.tobytes())

    comps, comp_scale = _quantise_rows(components)
    with open(os.path.join(out, "card-pca.bin"), "wb") as fh:
        fh.write(mean.astype(np.float32).tobytes())
        fh.write((scale * comp_scale).astype(np.float32).tobytes())
        fh.write(comps.tobytes())


def read_index(path: str):
    """Returns (meta, cfg, index, (mean, components, scale)), dequantised to float32."""
    with open(os.path.join(path, "card-index.json"), encoding="utf-8") as fh:
        meta = json.load(fh)
    cfg = meta["config"]
    if cfg.get("storage") != STORAGE:
        raise SystemExit(f"{path} is not an {STORAGE} index - rebuild it with build_index.py")

    count, pca_dim = cfg["count"], cfg["pcaDim"]
    raw_dim = cfg["pixelGrid"] ** 2 * 3 + cfg["embedDim"]

    blob = np.fromfile(os.path.join(path, "card-index.bin"), dtype=np.uint8)
    row_scale = blob[:count * 4].view(np.float32)
    rows = blob[count * 4:].view(np.int8).reshape(count, pca_dim)
    index = rows.astype(np.float32) * row_scale[:, None]

    blob = np.fromfile(os.path.join(path, "card-pca.bin"), dtype=np.uint8)
    floats = (raw_dim + pca_dim) * 4
    head = blob[:floats].view(np.float32)
    mean, scale = head[:raw_dim], head[raw_dim:]
    components = blob[floats:].view(np.int8).reshape(pca_dim, raw_dim).astype(np.float32)

    return meta, cfg, index, (mean, components, scale)
