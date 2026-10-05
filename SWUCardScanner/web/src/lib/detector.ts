import * as ort from 'onnxruntime-web'

// Multi-threaded wasm needs SharedArrayBuffer, which needs cross-origin isolation
// (the COOP/COEP headers set in vite.config.ts). Fall back to a single thread when the
// page is not isolated, rather than letting onnxruntime fail to spin up its thread pool.
ort.env.wasm.numThreads = globalThis.crossOriginIsolated
  ? Math.min(4, navigator.hardwareConcurrency || 1)
  : 1

export interface Detection {
  /** Confidence in [0,1]. */
  score: number
  /** Axis-aligned box [x1, y1, x2, y2] in source-image pixels. */
  box: [number, number, number, number]
  /** Card corners in source-image pixels, ordered top-left, top-right, bottom-right, bottom-left. */
  corners: [number, number][]
}

export interface DetectOptions {
  /** Minimum confidence to keep a detection. */
  scoreThreshold: number
  /** IoU above which two boxes are considered the same card. */
  iouThreshold: number
  /** Hard cap on returned detections. */
  maxDetections: number
}

export const DEFAULT_OPTIONS: DetectOptions = {
  scoreThreshold: 0.35,
  iouThreshold: 0.45,
  maxDetections: 24,
}

/** Grey used by YOLO's letterbox padding; the model was trained against it. */
const PAD_VALUE = 114

/** Which onnxruntime backend actually ended up running the model. */
export type Backend = 'webgpu' | 'wasm (worker)' | 'wasm'

export class CardDetector {
  private session: ort.InferenceSession | null = null
  private inputName = 'images'
  private canvas: HTMLCanvasElement
  private ctx: CanvasRenderingContext2D
  private buffer: Float32Array | null = null

  /** Square input resolution the model was exported at. */
  readonly size: number
  /** The backend in use, once a model is loaded. */
  backend: Backend | null = null
  /** Milliseconds spent inside the last `detect()` call. */
  lastInferenceMs = 0

  constructor(size = 768) {
    this.size = size
    this.canvas = document.createElement('canvas')
    this.canvas.width = size
    this.canvas.height = size
    const ctx = this.canvas.getContext('2d', { willReadFrequently: true })
    if (!ctx) throw new Error('Could not create a 2D canvas context')
    this.ctx = ctx
  }

  get ready(): boolean {
    return this.session !== null
  }

  /**
   * Load a model from a URL or from raw bytes (a file the user picked).
   *
   * Backends are tried best-first:
   *   1. WebGPU        - roughly 7x faster than wasm here, at full precision.
   *   2. wasm (worker) - proxied off the UI thread so the video stays smooth.
   *   3. wasm          - same thread; last resort if the proxy worker won't spawn.
   *
   * WebGPU runs unproxied on purpose. The GPU does the work while JS only submits
   * commands and awaits, so the UI thread stays free without a worker, and the WebGPU
   * backend inside a proxy worker is considerably more fragile.
   */
  async load(source: string | ArrayBuffer): Promise<void> {
    await this.dispose()

    const create = (providers: ort.InferenceSession.SessionOptions['executionProviders'],
                    proxy: boolean) => {
      ort.env.wasm.proxy = proxy
      return ort.InferenceSession.create(source as string & ArrayBuffer, {
        executionProviders: providers,
        graphOptimizationLevel: 'all',
      })
    }

    const attempts: [Backend, () => Promise<ort.InferenceSession>][] = []
    if (typeof navigator !== 'undefined' && 'gpu' in navigator) {
      attempts.push(['webgpu', () => create(['webgpu'], false)])
    }
    attempts.push(['wasm (worker)', () => create(['wasm'], true)])
    attempts.push(['wasm', () => create(['wasm'], false)])

    const failures: string[] = []
    for (const [backend, start] of attempts) {
      try {
        this.session = await start()
        this.backend = backend
        break
      } catch (err) {
        failures.push(`${backend}: ${err instanceof Error ? err.message : String(err)}`)
      }
    }

    if (!this.session) {
      throw new Error(`No usable backend.\n${failures.join('\n')}`)
    }

    this.inputName = this.session.inputNames[0] ?? 'images'
    await this.warmUp()
  }

  /**
   * Run one throwaway inference on a blank frame.
   *
   * WebGPU compiles a shader per operator on first use, which costs several seconds.
   * Paying that here rather than on the first real camera frame is the difference
   * between a slow load and a scanner that appears to hang the moment you point it at
   * something.
   */
  private async warmUp(): Promise<void> {
    if (!this.session) return
    const pixels = this.size * this.size
    const blank = new Float32Array(pixels * 3).fill(PAD_VALUE / 255)
    try {
      await this.session.run({
        [this.inputName]: new ort.Tensor('float32', blank, [1, 3, this.size, this.size]),
      })
    } catch {
      // A warm-up failure is not fatal; the first real frame will surface any real problem.
    }
  }

  async dispose(): Promise<void> {
    await this.session?.release()
    this.session = null
  }

  /**
   * Letterbox a frame into the model's square input.
   *
   * Returns the scale and padding needed to map detections back to source pixels.
   */
  private preprocess(frame: CanvasImageSource, srcW: number, srcH: number) {
    const { size } = this
    const scale = Math.min(size / srcW, size / srcH)
    const drawW = Math.round(srcW * scale)
    const drawH = Math.round(srcH * scale)
    const padX = (size - drawW) / 2
    const padY = (size - drawH) / 2

    this.ctx.fillStyle = `rgb(${PAD_VALUE},${PAD_VALUE},${PAD_VALUE})`
    this.ctx.fillRect(0, 0, size, size)
    this.ctx.drawImage(frame, 0, 0, srcW, srcH, padX, padY, drawW, drawH)

    const { data } = this.ctx.getImageData(0, 0, size, size)
    const pixels = size * size
    const needed = pixels * 3

    // In proxy mode onnxruntime posts the input buffer to its worker as a transferable,
    // which detaches it here - a detached typed array reports length 0. So the buffer is
    // only reused while it is still intact, and silently reallocated once it has been
    // handed away. Checking length rather than a flag also covers a resolution change.
    if (!this.buffer || this.buffer.length !== needed) {
      this.buffer = new Float32Array(needed)
    }
    const out = this.buffer

    // RGBA interleaved -> planar RGB, scaled to [0,1].
    for (let i = 0; i < pixels; i++) {
      const j = i * 4
      out[i] = data[j] / 255
      out[pixels + i] = data[j + 1] / 255
      out[pixels * 2 + i] = data[j + 2] / 255
    }

    return { data: out, scale, padX, padY }
  }

  async detect(
    frame: CanvasImageSource,
    srcW: number,
    srcH: number,
    options: Partial<DetectOptions> = {},
  ): Promise<Detection[]> {
    if (!this.session) return []

    const opts = { ...DEFAULT_OPTIONS, ...options }
    const started = performance.now()

    const { data, scale, padX, padY } = this.preprocess(frame, srcW, srcH)
    const tensor = new ort.Tensor('float32', data, [1, 3, this.size, this.size])
    const results = await this.session.run({ [this.inputName]: tensor })
    const output = results[this.session.outputNames[0]]

    const detections = decode(
      output.data as Float32Array,
      output.dims as number[],
      opts,
      scale,
      padX,
      padY,
    )

    this.lastInferenceMs = performance.now() - started
    return detections
  }
}

/**
 * Turn a raw YOLO-pose head into detections in source-image pixels.
 *
 * Ultralytics emits `[1, channels, anchors]` where channels are
 * `[cx, cy, w, h, class scores..., keypoints...]` in model-input pixels. With one
 * class and four 2-D keypoints that is 13 channels. Both channel layouts are handled
 * because some export paths transpose the output.
 */
function decode(
  data: Float32Array,
  dims: number[],
  opts: DetectOptions,
  scale: number,
  padX: number,
  padY: number,
): Detection[] {
  if (dims.length !== 3) return []

  // Anchors vastly outnumber channels, so the larger dimension is the anchor axis.
  const transposed = dims[1] > dims[2]
  const channels = transposed ? dims[2] : dims[1]
  const anchors = transposed ? dims[1] : dims[2]

  // 4 box + 1 class + 4 keypoints of either 2 (x,y) or 3 (x,y,visibility) values.
  const kptDim = (channels - 5) / 4 === 2 ? 2 : (channels - 5) / 4 === 3 ? 3 : 0
  if (kptDim === 0) {
    throw new Error(
      `Unexpected model output with ${channels} channels; expected 13 (1 class, 4 corners).`,
    )
  }

  const at = transposed
    ? (c: number, i: number) => data[i * channels + c]
    : (c: number, i: number) => data[c * anchors + i]

  const candidates: Detection[] = []
  for (let i = 0; i < anchors; i++) {
    const score = at(4, i)
    if (score < opts.scoreThreshold) continue

    const cx = (at(0, i) - padX) / scale
    const cy = (at(1, i) - padY) / scale
    const w = at(2, i) / scale
    const h = at(3, i) / scale

    const corners: [number, number][] = []
    for (let k = 0; k < 4; k++) {
      const base = 5 + k * kptDim
      corners.push([(at(base, i) - padX) / scale, (at(base + 1, i) - padY) / scale])
    }

    candidates.push({
      score,
      box: [cx - w / 2, cy - h / 2, cx + w / 2, cy + h / 2],
      corners,
    })
  }

  return nms(candidates, opts.iouThreshold).slice(0, opts.maxDetections)
}

function iou(a: Detection['box'], b: Detection['box']): number {
  const x1 = Math.max(a[0], b[0])
  const y1 = Math.max(a[1], b[1])
  const x2 = Math.min(a[2], b[2])
  const y2 = Math.min(a[3], b[3])
  const inter = Math.max(0, x2 - x1) * Math.max(0, y2 - y1)
  if (inter <= 0) return 0
  const areaA = (a[2] - a[0]) * (a[3] - a[1])
  const areaB = (b[2] - b[0]) * (b[3] - b[1])
  return inter / (areaA + areaB - inter)
}

function nms(detections: Detection[], iouThreshold: number): Detection[] {
  const sorted = detections.sort((a, b) => b.score - a.score)
  const kept: Detection[] = []
  for (const candidate of sorted) {
    if (kept.every((k) => iou(k.box, candidate.box) < iouThreshold)) {
      kept.push(candidate)
    }
  }
  return kept
}
