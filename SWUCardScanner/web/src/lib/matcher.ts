import * as ort from 'onnxruntime-web'
import { quadOrientation, sharpness, unwarpCard } from './unwarp'

/**
 * Card identification: un-warp a detection, describe it as a vector, and find the
 * nearest reference.
 *
 * Every step here has an exact counterpart in model/descriptors.py, because the index
 * is built in Python and searched here. Where the two could plausibly disagree - image
 * downscaling, blur kernel, channel order - this file does the arithmetic explicitly
 * rather than delegating to the browser, whose filters are unspecified and vary.
 *
 * The descriptor is a hybrid, chosen on measurements in compare_descriptors.py:
 * a normalised pixel grid supplies instance-level detail (a plain CNN embedding scored
 * 24 points worse on its own), a MobileNetV3 embedding supplies tolerance to corner
 * error, and a whitened PCA projection makes the index small and, as it turns out,
 * more robust still.
 */

export interface IndexCard {
  file: string
  name: string
  set: string
  type: string
  face: string
  orientation: 'portrait' | 'landscape'
  baseId: number
  /** Every printing that uses this exact image, e.g. a card and its foil. */
  variantIds: number[]
  url: string
  urlSegment: string
}

export interface IndexConfig {
  dim: number
  count: number
  unwarpSize: number
  pixelGrid: number
  pixelWeight: number
  /** Binomial-blur passes subtracted from the pixel grid to cancel glare gradients. */
  highpass?: number
  embedSize: number
  embedDim: number
  pcaDim: number
  embedder: string
  storage?: string
}

/** Why a candidate was not asserted. Null when it was accepted. */
export type RejectReason = 'blurry' | 'low-score' | 'low-margin' | 'no-index' | null

export interface Candidate {
  card: IndexCard
  score: number
}

export interface MatchResult {
  /** Best candidate, present even when rejected, so the UI can explain itself. */
  card: IndexCard | null
  /** Cosine similarity to the best reference, in [-1, 1]. */
  score: number
  /** Gap to the second-best card. Small gaps mean "two cards look alike", not "no idea". */
  margin: number
  /** Laplacian variance of the crop; low means too blurry to trust. */
  sharpness: number
  /** Whether this passed every gate and should be shown as an identification. */
  accepted: boolean
  reason: RejectReason
  /** Runners-up, for diagnosing a near-miss. */
  candidates: Candidate[]
  /** The un-warped crop the matcher actually saw. Only populated in debug mode. */
  crop?: Uint8ClampedArray
  cropSize?: number
}

const NO_MATCH: MatchResult = {
  card: null, score: 0, margin: 0, sharpness: 0,
  accepted: false, reason: 'no-index', candidates: [],
}

export interface MatchOptions {
  minScore: number
  minMargin: number
  minSharpness: number
}

export const DEFAULT_MATCH_OPTIONS: MatchOptions = {
  // Calibrated from eval_matcher.py: correct matches average a margin near 0.30 while
  // wrong ones sit near 0.02, so rejecting on margin costs very few true positives.
  minScore: 0.25,
  minMargin: 0.06,
  minSharpness: 8,
}

/** Separable [1,2,1]/4 blur with edge clamping. Mirrors binomial_blur in Python. */
function binomialBlur(src: Float32Array, grid: number): Float32Array {
  const tmp = new Float32Array(src.length)
  const out = new Float32Array(src.length)
  const clamp = (i: number) => Math.max(0, Math.min(grid - 1, i))

  for (let y = 0; y < grid; y++) {
    for (let x = 0; x < grid; x++) {
      for (let c = 0; c < 3; c++) {
        const lo = src[(clamp(y - 1) * grid + x) * 3 + c]
        const mid = src[(y * grid + x) * 3 + c]
        const hi = src[(clamp(y + 1) * grid + x) * 3 + c]
        tmp[(y * grid + x) * 3 + c] = (lo + 2 * mid + hi) / 4
      }
    }
  }
  for (let y = 0; y < grid; y++) {
    for (let x = 0; x < grid; x++) {
      for (let c = 0; c < 3; c++) {
        const lo = tmp[(y * grid + clamp(x - 1)) * 3 + c]
        const mid = tmp[(y * grid + x) * 3 + c]
        const hi = tmp[(y * grid + clamp(x + 1)) * 3 + c]
        out[(y * grid + x) * 3 + c] = (lo + 2 * mid + hi) / 4
      }
    }
  }
  return out
}

/** Block-average an RGB image down to grid x grid. Mirrors box_downscale in Python. */
function boxDownscale(rgb: Uint8ClampedArray, size: number, grid: number): Float32Array {
  const block = size / grid
  if (!Number.isInteger(block)) {
    throw new Error(`unwarp size ${size} does not divide into a ${grid} grid`)
  }
  const out = new Float32Array(grid * grid * 3)
  const perBlock = block * block

  for (let gy = 0; gy < grid; gy++) {
    for (let gx = 0; gx < grid; gx++) {
      let r = 0, g = 0, b = 0
      for (let y = gy * block; y < (gy + 1) * block; y++) {
        for (let x = gx * block; x < (gx + 1) * block; x++) {
          const i = (y * size + x) * 3
          r += rgb[i]; g += rgb[i + 1]; b += rgb[i + 2]
        }
      }
      const o = (gy * grid + gx) * 3
      out[o] = r / perBlock
      out[o + 1] = g / perBlock
      out[o + 2] = b / perBlock
    }
  }
  return out
}

function l2Normalise(v: Float32Array): Float32Array {
  let sum = 0
  for (let i = 0; i < v.length; i++) sum += v[i] * v[i]
  const inv = 1 / Math.max(Math.sqrt(sum), 1e-9)
  for (let i = 0; i < v.length; i++) v[i] *= inv
  return v
}

export class CardMatcher {
  private session: ort.InferenceSession | null = null
  private inputName = 'images'

  /** Reference vectors, PCA space, row-major [count x pcaDim], each unit length once scaled. */
  private index: Int8Array | null = null
  private indexScale: Float32Array | null = null
  private pcaMean: Float32Array | null = null
  private pcaComponents: Int8Array | null = null
  private pcaScale: Float32Array | null = null

  cards: IndexCard[] = []
  config: IndexConfig | null = null
  lastMatchMs = 0

  get ready(): boolean {
    return this.session !== null && this.index !== null
  }

  async load(base: string, fetchFile: (url: string) => Promise<Response> = fetch): Promise<void> {
    const meta = await fetchFile(`${base}/card-index.json`).then((r) => {
      if (!r.ok) throw new Error(`No card index at ${base} (run build_index.py)`)
      return r.json()
    })

    this.config = meta.config as IndexConfig
    if (this.config.storage !== 'int8') throw new Error(`Index at ${base} is not int8 (rebuild with build_index.py)`)
    this.cards = meta.cards as IndexCard[]

    const [indexBuf, pcaBuf, embedderBuf] = await Promise.all([
      fetchFile(`${base}/card-index.bin`).then((r) => r.arrayBuffer()),
      fetchFile(`${base}/card-pca.bin`).then((r) => r.arrayBuffer()),
      fetchFile(`${base}/${this.config.embedder}`).then((r) => r.arrayBuffer()),
    ])

    const { pcaDim, count, embedDim, pixelGrid } = this.config
    const rawDim = pixelGrid * pixelGrid * 3 + embedDim

    // Layouts are documented in model/index_store.py.
    if (indexBuf.byteLength !== count * 4 + count * pcaDim) {
      throw new Error(`Index is ${indexBuf.byteLength} bytes, expected ${count * 4 + count * pcaDim}`)
    }
    this.indexScale = new Float32Array(indexBuf, 0, count)
    this.index = new Int8Array(indexBuf, count * 4)

    const pcaFloats = new Float32Array(pcaBuf, 0, rawDim + pcaDim)
    this.pcaMean = pcaFloats.subarray(0, rawDim)
    this.pcaScale = pcaFloats.subarray(rawDim)
    this.pcaComponents = new Int8Array(pcaBuf, (rawDim + pcaDim) * 4)

    this.session = await ort.InferenceSession.create(new Uint8Array(embedderBuf), {
      executionProviders: ['webgpu', 'wasm'],
      graphOptimizationLevel: 'all',
    })
    this.inputName = this.session.inputNames[0] ?? 'images'

    // WebGPU compiles a shader per operator on first use - about 2.8 seconds here.
    // Paying it now rather than on the first identified card keeps the scanner from
    // appearing to freeze the moment it sees something.
    const { embedSize } = this.config
    try {
      await this.session.run({
        [this.inputName]: new ort.Tensor(
          'float32',
          new Float32Array(3 * embedSize * embedSize).fill(0.5),
          [1, 3, embedSize, embedSize],
        ),
      })
    } catch {
      // Not fatal; the first real crop will surface any genuine problem.
    }
  }

  async dispose(): Promise<void> {
    await this.session?.release()
    this.session = null
    this.index = null
    this.indexScale = null
  }

  /** Build the raw (pre-PCA) descriptor for one un-warped crop. */
  private async describe(rgb: Uint8ClampedArray): Promise<Float32Array> {
    const { unwarpSize, pixelGrid, pixelWeight, embedSize, embedDim } = this.config!

    let grid = binomialBlur(boxDownscale(rgb, unwarpSize, pixelGrid), pixelGrid)

    // Subtracting a heavily smoothed copy removes any gradient across the card. The
    // per-channel normalisation below only cancels a *uniform* colour cast, whereas
    // light reflecting off a glossy card is a ramp - and because glare is additive it
    // ruins dark cards especially: the same +40 is a 20% shift on a bright card and a
    // 4x shift on a near-black one.
    const passes = this.config!.highpass ?? 0
    if (passes > 0) {
      let smooth = grid
      for (let i = 0; i < passes; i++) smooth = binomialBlur(smooth, pixelGrid)
      const highpassed = new Float32Array(grid.length)
      for (let i = 0; i < grid.length; i++) highpassed[i] = grid[i] - smooth[i]
      grid = highpassed
    }

    // Per-channel contrast normalisation, so a warm or cold light cancels out.
    const cells = pixelGrid * pixelGrid
    for (let c = 0; c < 3; c++) {
      let mean = 0
      for (let i = 0; i < cells; i++) mean += grid[i * 3 + c]
      mean /= cells
      let variance = 0
      for (let i = 0; i < cells; i++) {
        const d = grid[i * 3 + c] - mean
        variance += d * d
      }
      const inv = 1 / (Math.sqrt(variance / cells) + 1e-6)
      for (let i = 0; i < cells; i++) grid[i * 3 + c] = (grid[i * 3 + c] - mean) * inv
    }
    l2Normalise(grid)

    // The embedder input is the un-warped crop itself, so there is no second resample.
    if (embedSize !== unwarpSize) {
      throw new Error(`embedder wants ${embedSize}px but crops are ${unwarpSize}px`)
    }
    const planar = new Float32Array(3 * embedSize * embedSize)
    const pixels = embedSize * embedSize
    for (let i = 0; i < pixels; i++) {
      planar[i] = rgb[i * 3] / 255
      planar[pixels + i] = rgb[i * 3 + 1] / 255
      planar[pixels * 2 + i] = rgb[i * 3 + 2] / 255
    }
    const result = await this.session!.run({
      [this.inputName]: new ort.Tensor('float32', planar, [1, 3, embedSize, embedSize]),
    })
    const deep = result[this.session!.outputNames[0]].data as Float32Array

    const raw = new Float32Array(grid.length + embedDim)
    for (let i = 0; i < grid.length; i++) raw[i] = grid[i] * pixelWeight
    for (let i = 0; i < embedDim; i++) raw[grid.length + i] = deep[i] * (1 - pixelWeight)
    return l2Normalise(raw)
  }

  private project(raw: Float32Array): Float32Array {
    const { pcaDim } = this.config!
    const mean = this.pcaMean!
    const comps = this.pcaComponents!
    const scale = this.pcaScale!
    const rawDim = raw.length

    const out = new Float32Array(pcaDim)
    for (let k = 0; k < pcaDim; k++) {
      let acc = 0
      const row = k * rawDim
      for (let i = 0; i < rawDim; i++) acc += (raw[i] - mean[i]) * comps[row + i]
      out[k] = acc * scale[k]
    }
    return l2Normalise(out)
  }

  /**
   * Identify one detected card.
   *
   * Always returns a result rather than null, carrying the best candidate and the reason
   * it was rejected. "No match" has several quite different causes - too blurry, too
   * weak, or torn between two look-alike cards - and they need different fixes, so
   * collapsing them all into null makes the thing undiagnosable from the outside.
   *
   * In `debug` mode the blur gate does not short-circuit and the crop is returned, so a
   * caller can show what the matcher actually saw and what it would have picked.
   */
  async identify(
    frame: ImageData,
    corners: [number, number][],
    options: Partial<MatchOptions> = {},
    debug = false,
  ): Promise<MatchResult> {
    if (!this.ready) return NO_MATCH
    const opts = { ...DEFAULT_MATCH_OPTIONS, ...options }
    const started = performance.now()

    const { unwarpSize, pcaDim, count } = this.config!
    const crop = unwarpCard(frame, corners, unwarpSize)
    const focus = sharpness(crop, unwarpSize)
    const extras = debug ? { crop, cropSize: unwarpSize } : {}

    const tooBlurry = focus < opts.minSharpness
    if (tooBlurry && !debug) {
      this.lastMatchMs = performance.now() - started
      return { ...NO_MATCH, sharpness: focus, reason: 'blurry', ...extras }
    }

    const query = this.project(await this.describe(crop))
    const want = quadOrientation(corners)

    // Keep the best few rows. TOP_K must exceed the largest number of index rows any
    // single card owns (currently 7), so that the list is guaranteed to contain at
    // least one genuinely different card to measure the margin against.
    const TOP_K = 8
    const topScores = new Array<number>(TOP_K).fill(-Infinity)
    const topRows = new Array<number>(TOP_K).fill(-1)

    const index = this.index!
    const indexScale = this.indexScale!
    for (let row = 0; row < count; row++) {
      // A landscape card can never be a portrait one; skipping the impossible half of
      // the index removes a whole category of confusion for free.
      if (this.cards[row].orientation !== want) continue

      let dot = 0
      const at = row * pcaDim
      for (let k = 0; k < pcaDim; k++) dot += query[k] * index[at + k]
      dot *= indexScale[row]

      if (dot <= topScores[TOP_K - 1]) continue
      let slot = TOP_K - 1
      while (slot > 0 && topScores[slot - 1] < dot) {
        topScores[slot] = topScores[slot - 1]
        topRows[slot] = topRows[slot - 1]
        slot--
      }
      topScores[slot] = dot
      topRows[slot] = row
    }

    this.lastMatchMs = performance.now() - started

    if (topRows[0] < 0) {
      return { ...NO_MATCH, sharpness: focus, reason: 'no-index', ...extras }
    }

    const score = topScores[0]
    const winner = this.cards[topRows[0]]

    // Margin is measured against the best *different card*, not the next row. Nearly
    // half the index shares a baseId with another row - alternate printings and promos
    // reuse artwork - so a card that appears twice would otherwise be rejected for
    // being too similar to itself, which is the one case we should be most sure about.
    let rivalScore = 0
    for (let i = 1; i < TOP_K; i++) {
      const row = topRows[i]
      if (row < 0) continue
      if (this.cards[row].baseId === winner.baseId) continue
      rivalScore = topScores[i]
      break
    }
    const margin = score - rivalScore

    // Collapse the candidate list by card too, so the runner-up shown is a real rival.
    const candidates: Candidate[] = []
    const seenBase = new Set<number>()
    for (let i = 0; i < TOP_K && candidates.length < 3; i++) {
      const row = topRows[i]
      if (row < 0 || seenBase.has(this.cards[row].baseId)) continue
      seenBase.add(this.cards[row].baseId)
      candidates.push({ card: this.cards[row], score: topScores[i] })
    }

    const reason: RejectReason = tooBlurry
      ? 'blurry'
      : score < opts.minScore
        ? 'low-score'
        : margin < opts.minMargin
          ? 'low-margin'
          : null

    return {
      card: this.cards[topRows[0]],
      score,
      margin,
      sharpness: focus,
      accepted: reason === null,
      reason,
      candidates,
      ...extras,
    }
  }
}
