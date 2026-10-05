"""Store the detector's weights as fp16 for the app, while it still computes in fp32.

Each weight is saved as fp16 behind a Cast to float, which onnxruntime constant-folds
at load. That halves the file without needing shader-f16: a fully fp16 graph ran
about 70% slower on a phone whose WebGPU lacks it.

Usage:
    python export_fp16.py
"""

import os
import sys

import numpy as np
import onnx
from onnx import TensorProto, helper, numpy_helper

HERE = os.path.dirname(os.path.abspath(__file__))
MODELS = os.path.abspath(os.path.join(HERE, "..", "web", "public", "models"))
MIN_ELEMENTS = 1024


def halve_weights(model: onnx.ModelProto) -> None:
    casts, kept = [], []
    for tensor in model.graph.initializer:
        values = numpy_helper.to_array(tensor)
        if tensor.data_type != TensorProto.FLOAT or values.size < MIN_ELEMENTS:
            kept.append(tensor)
            continue
        stored = f"{tensor.name}_fp16"
        kept.append(numpy_helper.from_array(values.astype(np.float16), stored))
        casts.append(helper.make_node("Cast", [stored], [tensor.name], to=TensorProto.FLOAT))

    del model.graph.initializer[:]
    model.graph.initializer.extend(kept)
    nodes = casts + list(model.graph.node)
    del model.graph.node[:]
    model.graph.node.extend(nodes)


def main() -> int:
    src = os.path.join(MODELS, "swu-detect.onnx")
    dst = os.path.join(MODELS, "swu-detect-fp16.onnx")
    model = onnx.load(src)
    halve_weights(model)
    onnx.checker.check_model(model)
    onnx.save(model, dst)
    print(f"{dst}: {os.path.getsize(dst) / 1e6:.1f} MB")
    return 0


if __name__ == "__main__":
    sys.exit(main())
