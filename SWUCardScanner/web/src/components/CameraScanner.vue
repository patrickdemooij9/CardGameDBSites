<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, shallowRef, watch } from 'vue'
import { CardDetector, type Detection } from '../lib/detector'
import {
  CardMatcher,
  DEFAULT_MATCH_OPTIONS,
  type IndexCard,
  type MatchResult,
} from '../lib/matcher'

const DEFAULT_MODEL_URL = `${import.meta.env.BASE_URL}models/swu-detect.onnx`
const INDEX_BASE = `${import.meta.env.BASE_URL}index`

/** Identification is the expensive half; cap how many cards get it per frame. */
const MAX_IDENTIFY_PER_FRAME = 4

interface Sighting {
  card: IndexCard
  votes: number
  bestScore: number
}

const video = ref<HTMLVideoElement | null>(null)
const overlay = ref<HTMLCanvasElement | null>(null)

const detector = shallowRef<CardDetector | null>(null)
const detections = shallowRef<Detection[]>([])

const matcher = shallowRef<CardMatcher | null>(null)
/** Parallel to `detections`; carries rejected candidates too, with the reason. */
const matches = shallowRef<(MatchResult | null)[]>([])
const indexState = ref<'idle' | 'loading' | 'ready' | 'error'>('idle')
const indexError = ref('')
const identifyEnabled = ref(true)
const identifyMs = ref(0)
/** Cards seen so far this session, accumulated across frames. */
const sightings = ref<Sighting[]>([])

const debugEnabled = ref(false)
const cropCanvas = ref<HTMLCanvasElement | null>(null)
const minScore = ref(DEFAULT_MATCH_OPTIONS.minScore)
const minMargin = ref(DEFAULT_MATCH_OPTIONS.minMargin)
const minSharpness = ref(DEFAULT_MATCH_OPTIONS.minSharpness)

const REASON_TEXT: Record<string, string> = {
  blurry: 'too blurry',
  'low-score': 'no close match',
  'low-margin': 'two cards too alike',
  'no-index': 'index not loaded',
}

/** The detection the diagnostics panel is describing — the highest-confidence one. */
const inspected = computed<MatchResult | null>(() => matches.value[0] ?? null)

const modelState = ref<'idle' | 'loading' | 'ready' | 'error'>('idle')
const modelError = ref('')
const modelLabel = ref('')
const cameraState = ref<'idle' | 'starting' | 'live' | 'error'>('idle')
const cameraError = ref('')

const devices = ref<MediaDeviceInfo[]>([])
const selectedDeviceId = ref('')
const scoreThreshold = ref(0.35)
const iouThreshold = ref(0.45)
const showCorners = ref(true)
const showBoxes = ref(false)
const paused = ref(false)

const fps = ref(0)
const inferenceMs = ref(0)

let stream: MediaStream | null = null
let rafId = 0
let inFlight = false
let frameTimes: number[] = []

const frameCanvas = document.createElement('canvas')
const frameCtx = frameCanvas.getContext('2d', { willReadFrequently: true })!

const cardCount = computed(() => detections.value.length)
const canScan = computed(() => modelState.value === 'ready' && cameraState.value === 'live')

/* ------------------------------------------------------------------ model */

async function loadModel(source: string | ArrayBuffer, label: string) {
  modelState.value = 'loading'
  modelError.value = ''
  try {
    const instance = detector.value ?? new CardDetector(768)
    await instance.load(source)
    detector.value = instance
    modelLabel.value = label
    modelState.value = 'ready'
  } catch (err) {
    modelState.value = 'error'
    modelError.value = err instanceof Error ? err.message : String(err)
  }
}

async function loadDefaultModel() {
  // A HEAD request first, so a missing model reads as "not trained yet" rather than
  // as an opaque onnxruntime parse failure on an HTML 404 body.
  try {
    const head = await fetch(DEFAULT_MODEL_URL, { method: 'HEAD' })
    if (!head.ok) {
      modelState.value = 'error'
      modelError.value =
        'No model at public/models/swu-detect.onnx yet. Train one with model/train_colab.ipynb, or pick a file below.'
      return
    }
  } catch {
    modelState.value = 'error'
    modelError.value = 'Could not reach the model file.'
    return
  }
  await loadModel(DEFAULT_MODEL_URL, 'swu-detect.onnx')
}

async function onModelFile(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file) return
  await loadModel(await file.arrayBuffer(), file.name)
}

/* -------------------------------------------------------------- identification */

async function loadIndex() {
  indexState.value = 'loading'
  indexError.value = ''
  try {
    const instance = new CardMatcher()
    await instance.load(INDEX_BASE)
    matcher.value = instance
    indexState.value = 'ready'
  } catch (err) {
    indexState.value = 'error'
    indexError.value = err instanceof Error ? err.message : String(err)
  }
}

/**
 * Grab the current video frame at native resolution.
 *
 * Detection runs on a 768px letterbox, but the un-warp samples the full-resolution
 * frame - a card occupying a quarter of a 1080p frame has far more detail there than in
 * the downscaled copy the detector saw, and identification needs that detail.
 */
function captureFrame(): ImageData | null {
  const el = video.value
  if (!el || !el.videoWidth) return null
  if (frameCanvas.width !== el.videoWidth || frameCanvas.height !== el.videoHeight) {
    frameCanvas.width = el.videoWidth
    frameCanvas.height = el.videoHeight
  }
  frameCtx.drawImage(el, 0, 0)
  return frameCtx.getImageData(0, 0, frameCanvas.width, frameCanvas.height)
}

async function identifyAll(found: Detection[]) {
  const instance = matcher.value
  if (!instance?.ready || !found.length) {
    matches.value = []
    return
  }
  const frame = captureFrame()
  if (!frame) return

  const options = {
    minScore: minScore.value,
    minMargin: minMargin.value,
    minSharpness: minSharpness.value,
  }

  const started = performance.now()
  const results: MatchResult[] = []
  for (const [i, det] of found.slice(0, MAX_IDENTIFY_PER_FRAME).entries()) {
    // Only the inspected detection carries a crop back, so debug mode costs one extra
    // copy per frame rather than one per card.
    results.push(await instance.identify(frame, det.corners, options, debugEnabled.value && i === 0))
  }
  identifyMs.value = performance.now() - started
  matches.value = results

  if (debugEnabled.value) drawCrop(results[0])

  // A live feed gives many attempts at the same card, so votes accumulate and a single
  // unlucky frame cannot decide the answer.
  for (const match of results) {
    if (!match.accepted || !match.card) continue
    const card = match.card
    const existing = sightings.value.find((s) => s.card.baseId === card.baseId)
    if (existing) {
      existing.votes++
      existing.bestScore = Math.max(existing.bestScore, match.score)
    } else {
      sightings.value.push({ card, votes: 1, bestScore: match.score })
    }
  }
  sightings.value.sort((a, b) => b.votes - a.votes)
}

/** Paint the un-warped crop, so you can see the image the matcher was given. */
function drawCrop(result: MatchResult | undefined) {
  const canvas = cropCanvas.value
  if (!canvas || !result?.crop || !result.cropSize) return
  const size = result.cropSize
  if (canvas.width !== size) {
    canvas.width = size
    canvas.height = size
  }
  const ctx = canvas.getContext('2d')
  if (!ctx) return

  const rgba = new Uint8ClampedArray(size * size * 4)
  for (let i = 0; i < size * size; i++) {
    rgba[i * 4] = result.crop[i * 3]
    rgba[i * 4 + 1] = result.crop[i * 3 + 1]
    rgba[i * 4 + 2] = result.crop[i * 3 + 2]
    rgba[i * 4 + 3] = 255
  }
  ctx.putImageData(new ImageData(rgba, size, size), 0, 0)
}

/* ----------------------------------------------------------------- camera */

async function refreshDevices() {
  try {
    const all = await navigator.mediaDevices.enumerateDevices()
    devices.value = all.filter((d) => d.kind === 'videoinput')
  } catch {
    devices.value = []
  }
}

async function startCamera() {
  cameraState.value = 'starting'
  cameraError.value = ''
  stopCamera()

  try {
    stream = await navigator.mediaDevices.getUserMedia({
      video: selectedDeviceId.value
        ? { deviceId: { exact: selectedDeviceId.value }, width: { ideal: 1280 }, height: { ideal: 720 } }
        : { facingMode: { ideal: 'environment' }, width: { ideal: 1280 }, height: { ideal: 720 } },
      audio: false,
    })

    const el = video.value
    if (!el) throw new Error('Video element missing')
    el.srcObject = stream
    await el.play()

    // Labels are only populated after permission is granted.
    await refreshDevices()
    if (!selectedDeviceId.value) {
      const track = stream.getVideoTracks()[0]
      selectedDeviceId.value = track?.getSettings().deviceId ?? ''
    }

    cameraState.value = 'live'
    paused.value = false
    loop()
  } catch (err) {
    cameraState.value = 'error'
    cameraError.value =
      err instanceof Error
        ? err.name === 'NotAllowedError'
          ? 'Camera permission denied. Allow it in the browser and try again.'
          : err.message
        : String(err)
  }
}

function stopCamera() {
  cancelAnimationFrame(rafId)
  rafId = 0
  stream?.getTracks().forEach((t) => t.stop())
  stream = null
  if (video.value) video.value.srcObject = null
  if (cameraState.value === 'live') cameraState.value = 'idle'
  detections.value = []
  frameTimes = []
  fps.value = 0
}

/* ------------------------------------------------------------- scan loop */

function loop() {
  rafId = requestAnimationFrame(loop)

  const el = video.value
  if (!el || el.readyState < 2) return

  syncOverlaySize()

  if (!paused.value && detector.value?.ready && !inFlight) {
    inFlight = true
    detector.value
      .detect(el, el.videoWidth, el.videoHeight, {
        scoreThreshold: scoreThreshold.value,
        iouThreshold: iouThreshold.value,
      })
      .then(async (found) => {
        detections.value = found
        inferenceMs.value = detector.value?.lastInferenceMs ?? 0
        trackFps()
        if (identifyEnabled.value) await identifyAll(found)
        else matches.value = []
      })
      .catch((err) => {
        modelState.value = 'error'
        modelError.value = err instanceof Error ? err.message : String(err)
      })
      .finally(() => {
        inFlight = false
      })
  }

  draw()
}

function trackFps() {
  const now = performance.now()
  frameTimes.push(now)
  while (frameTimes.length > 1 && now - frameTimes[0] > 2000) frameTimes.shift()
  fps.value =
    frameTimes.length > 1
      ? ((frameTimes.length - 1) * 1000) / (now - frameTimes[0])
      : 0
}

/** Keep the overlay pixel grid matched to the displayed video box. */
function syncOverlaySize() {
  const el = video.value
  const canvas = overlay.value
  if (!el || !canvas) return
  const rect = el.getBoundingClientRect()
  const dpr = window.devicePixelRatio || 1
  const w = Math.round(rect.width * dpr)
  const h = Math.round(rect.height * dpr)
  if (canvas.width !== w || canvas.height !== h) {
    canvas.width = w
    canvas.height = h
  }
}

function draw() {
  const canvas = overlay.value
  const el = video.value
  if (!canvas || !el) return
  const ctx = canvas.getContext('2d')
  if (!ctx) return

  ctx.clearRect(0, 0, canvas.width, canvas.height)
  if (!el.videoWidth) return

  // `object-contain` letterboxes the video inside its box; mirror that mapping so the
  // overlay lands on the actual pixels rather than on the black bars.
  const scale = Math.min(canvas.width / el.videoWidth, canvas.height / el.videoHeight)
  const offsetX = (canvas.width - el.videoWidth * scale) / 2
  const offsetY = (canvas.height - el.videoHeight * scale) / 2
  const tx = (x: number) => x * scale + offsetX
  const ty = (y: number) => y * scale + offsetY

  const lineWidth = Math.max(2, canvas.width / 480)

  for (const [detIndex, det] of detections.value.entries()) {
    const [tl, tr, br, bl] = det.corners
    const match = matches.value[detIndex] ?? null

    ctx.beginPath()
    ctx.moveTo(tx(tl[0]), ty(tl[1]))
    ctx.lineTo(tx(tr[0]), ty(tr[1]))
    ctx.lineTo(tx(br[0]), ty(br[1]))
    ctx.lineTo(tx(bl[0]), ty(bl[1]))
    ctx.closePath()
    // Identified cards get a green outline so a glance tells you whether the scanner
    // merely found a card or actually knows which one it is.
    const identified = match?.accepted === true
    ctx.fillStyle = identified ? 'rgba(52, 211, 153, 0.16)' : 'rgba(56, 189, 248, 0.14)'
    ctx.fill()
    ctx.strokeStyle = identified ? '#34d399' : '#38bdf8'
    ctx.lineWidth = lineWidth
    ctx.stroke()

    if (showBoxes.value) {
      const [x1, y1, x2, y2] = det.box
      ctx.strokeStyle = 'rgba(255,255,255,0.5)'
      ctx.lineWidth = lineWidth / 2
      ctx.strokeRect(tx(x1), ty(y1), (x2 - x1) * scale, (y2 - y1) * scale)
    }

    if (showCorners.value) {
      // Distinct colours per corner make a wrong orientation obvious at a glance.
      const colours = ['#f87171', '#4ade80', '#60a5fa', '#facc15']
      det.corners.forEach((pt, i) => {
        ctx.beginPath()
        ctx.arc(tx(pt[0]), ty(pt[1]), lineWidth * 2.2, 0, Math.PI * 2)
        ctx.fillStyle = colours[i]
        ctx.fill()
      })
    }

    // A rejected candidate still says what it nearly picked and why it declined, so a
    // failure is readable from the viewfinder instead of being a silent blue box.
    let label: string
    if (identified && match?.card) {
      label = `${match.card.name} · ${(match.score * 100).toFixed(0)}% ±${(match.margin * 100).toFixed(0)}`
    } else if (match?.reason && match.reason !== 'no-index') {
      const near = match.card ? ` (${match.card.name} ${(match.score * 100).toFixed(0)}%)` : ''
      label = `${REASON_TEXT[match.reason]}${near}`
    } else {
      label = `card ${(det.score * 100).toFixed(0)}%`
    }
    ctx.font = `${Math.max(12, canvas.width / 45)}px ui-sans-serif, system-ui, sans-serif`
    const metrics = ctx.measureText(label)
    const padding = lineWidth * 2
    const boxH = Math.max(16, canvas.width / 38)
    ctx.fillStyle = 'rgba(2, 6, 23, 0.78)'
    ctx.fillRect(tx(tl[0]), ty(tl[1]) - boxH, metrics.width + padding * 2, boxH)
    ctx.fillStyle = identified ? '#a7f3d0' : match?.reason ? '#fcd34d' : '#e0f2fe'
    ctx.fillText(label, tx(tl[0]) + padding, ty(tl[1]) - boxH * 0.28)
  }
}

/* ------------------------------------------------------------- lifecycle */

watch(selectedDeviceId, (id, previous) => {
  if (id && previous && cameraState.value === 'live') startCamera()
})

// Freezing holds the last frame *and* its detections, so you can actually look at a
// result instead of chasing it around the viewfinder.
watch(paused, (isPaused) => {
  const el = video.value
  if (!el) return
  if (isPaused) el.pause()
  else void el.play()
})

onMounted(() => {
  loadDefaultModel()
  loadIndex()
  refreshDevices()
})

onBeforeUnmount(() => {
  stopCamera()
  detector.value?.dispose()
  matcher.value?.dispose()
})
</script>

<template>
  <div class="flex flex-col gap-4 lg:flex-row">
    <!-- Camera stage -->
    <div class="flex-1">
      <div class="relative overflow-hidden rounded-xl border border-slate-700 bg-slate-950 aspect-video">
        <video
          ref="video"
          class="h-full w-full object-contain"
          playsinline
          muted
          autoplay
        />
        <canvas ref="overlay" class="pointer-events-none absolute inset-0 h-full w-full" />

        <div
          v-if="cameraState !== 'live'"
          class="absolute inset-0 flex flex-col items-center justify-center gap-3 p-6 text-center"
        >
          <p class="text-sm text-slate-400">
            {{ cameraState === 'starting' ? 'Starting camera…' : 'Camera is off' }}
          </p>
          <p v-if="cameraError" class="max-w-sm text-sm text-rose-400">{{ cameraError }}</p>
          <button
            class="rounded-lg bg-sky-500 px-4 py-2 text-sm font-medium text-slate-950 hover:bg-sky-400"
            @click="startCamera"
          >
            Start camera
          </button>
        </div>

        <div
          v-if="canScan"
          class="absolute left-3 top-3 flex items-center gap-2 rounded-lg bg-slate-950/75 px-3 py-1.5 text-xs font-medium text-sky-200"
        >
          <span class="h-2 w-2 rounded-full" :class="paused ? 'bg-amber-400' : 'bg-emerald-400'" />
          {{ cardCount }} card{{ cardCount === 1 ? '' : 's' }}
          <span class="text-slate-500">·</span>
          {{ fps.toFixed(1) }} fps
          <span class="text-slate-500">·</span>
          detect {{ inferenceMs.toFixed(0) }} ms
          <template v-if="identifyEnabled && indexState === 'ready'">
            <span class="text-slate-500">·</span>
            id {{ identifyMs.toFixed(0) }} ms
          </template>
        </div>
      </div>

      <div class="mt-3 flex flex-wrap gap-2">
        <button
          v-if="cameraState === 'live'"
          class="rounded-lg border border-slate-700 px-3 py-1.5 text-sm text-slate-200 hover:bg-slate-800"
          @click="stopCamera"
        >
          Stop camera
        </button>
        <button
          v-if="cameraState === 'live'"
          class="rounded-lg border border-slate-700 px-3 py-1.5 text-sm text-slate-200 hover:bg-slate-800"
          @click="paused = !paused"
        >
          {{ paused ? 'Resume' : 'Freeze' }}
        </button>
        <select
          v-if="devices.length > 1"
          v-model="selectedDeviceId"
          class="rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-sm text-slate-200"
        >
          <option v-for="d in devices" :key="d.deviceId" :value="d.deviceId">
            {{ d.label || 'Camera' }}
          </option>
        </select>
      </div>
    </div>

    <!-- Controls -->
    <aside class="w-full shrink-0 space-y-5 lg:w-72">
      <section class="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 class="text-sm font-semibold text-slate-200">Model</h2>
        <p class="mt-1 text-xs" :class="modelState === 'ready' ? 'text-emerald-400' : 'text-slate-400'">
          <template v-if="modelState === 'ready'">
            {{ modelLabel }} loaded &middot; running on
            <span class="font-medium text-slate-200">{{ detector?.backend }}</span>
          </template>
          <template v-else-if="modelState === 'loading'">Loading…</template>
          <template v-else>Not loaded</template>
        </p>
        <p v-if="modelError" class="mt-2 text-xs leading-relaxed text-amber-400">{{ modelError }}</p>

        <label class="mt-3 block text-xs text-slate-400">
          Load a .onnx file
          <input
            type="file"
            accept=".onnx"
            class="mt-1 block w-full text-xs file:mr-2 file:rounded file:border-0 file:bg-slate-700 file:px-2 file:py-1 file:text-slate-100"
            @change="onModelFile"
          />
        </label>
      </section>

      <section class="rounded-xl border border-slate-700 bg-slate-900/60 p-4 space-y-4">
        <h2 class="text-sm font-semibold text-slate-200">Detection</h2>

        <label class="block text-xs text-slate-400">
          Confidence: <span class="text-slate-200">{{ scoreThreshold.toFixed(2) }}</span>
          <input v-model.number="scoreThreshold" type="range" min="0.05" max="0.95" step="0.01" class="mt-1 w-full" />
        </label>

        <label class="block text-xs text-slate-400">
          Overlap (IoU): <span class="text-slate-200">{{ iouThreshold.toFixed(2) }}</span>
          <input v-model.number="iouThreshold" type="range" min="0.1" max="0.9" step="0.01" class="mt-1 w-full" />
        </label>

        <label class="flex items-center gap-2 text-xs text-slate-300">
          <input v-model="showCorners" type="checkbox" class="accent-sky-500" />
          Show corner keypoints
        </label>
        <label class="flex items-center gap-2 text-xs text-slate-300">
          <input v-model="showBoxes" type="checkbox" class="accent-sky-500" />
          Show bounding boxes
        </label>
      </section>

      <section class="rounded-xl border border-slate-700 bg-slate-900/60 p-4 space-y-3">
        <div class="flex items-baseline justify-between gap-2">
          <h2 class="text-sm font-semibold text-slate-200">Identification</h2>
          <span class="text-xs text-slate-500">
            {{ indexState === 'ready' ? `${matcher?.config?.count ?? 0} cards` : indexState }}
          </span>
        </div>

        <p v-if="indexError" class="text-xs leading-relaxed text-amber-400">{{ indexError }}</p>

        <label class="flex items-center gap-2 text-xs text-slate-300">
          <input
            v-model="identifyEnabled"
            type="checkbox"
            class="accent-emerald-500"
            :disabled="indexState !== 'ready'"
          />
          Identify detected cards
        </label>

        <label class="flex items-center gap-2 text-xs text-slate-300">
          <input
            v-model="debugEnabled"
            type="checkbox"
            class="accent-amber-500"
            :disabled="indexState !== 'ready'"
          />
          Diagnostics
        </label>

        <div v-if="debugEnabled" class="space-y-3 rounded-lg bg-slate-950/60 p-3">
          <p class="text-[0.7rem] leading-relaxed text-slate-500">
            What the matcher sees after un-warping, for the highest-confidence detection.
            Freeze the frame to inspect it.
          </p>

          <canvas
            ref="cropCanvas"
            class="w-full rounded border border-slate-700 bg-slate-900"
            style="image-rendering: pixelated"
          />

          <dl v-if="inspected" class="grid grid-cols-2 gap-x-2 gap-y-1 text-[0.7rem]">
            <dt class="text-slate-500">Score</dt>
            <dd class="text-right tabular-nums text-slate-200">{{ inspected.score.toFixed(3) }}</dd>
            <dt class="text-slate-500">Margin</dt>
            <dd class="text-right tabular-nums" :class="inspected.margin >= minMargin ? 'text-emerald-400' : 'text-amber-400'">
              {{ inspected.margin.toFixed(3) }}
            </dd>
            <dt class="text-slate-500">Sharpness</dt>
            <dd class="text-right tabular-nums" :class="inspected.sharpness >= minSharpness ? 'text-emerald-400' : 'text-amber-400'">
              {{ inspected.sharpness.toFixed(1) }}
            </dd>
            <dt class="text-slate-500">Verdict</dt>
            <dd class="text-right" :class="inspected.accepted ? 'text-emerald-400' : 'text-amber-400'">
              {{ inspected.accepted ? 'accepted' : REASON_TEXT[inspected.reason ?? ''] ?? '—' }}
            </dd>
          </dl>

          <ol v-if="inspected?.candidates.length" class="space-y-1">
            <li
              v-for="(c, i) in inspected.candidates"
              :key="c.card.baseId + '-' + i"
              class="flex items-baseline gap-2 text-[0.7rem]"
              :class="i === 0 ? 'text-slate-200' : 'text-slate-500'"
            >
              <span class="w-3 shrink-0 tabular-nums">{{ i + 1 }}</span>
              <span class="min-w-0 flex-1 truncate">{{ c.card.name }}</span>
              <span class="shrink-0 tabular-nums">{{ c.score.toFixed(3) }}</span>
            </li>
          </ol>

          <div class="space-y-2 border-t border-slate-800 pt-2">
            <label class="block text-[0.7rem] text-slate-400">
              Min score: <span class="tabular-nums text-slate-200">{{ minScore.toFixed(2) }}</span>
              <input v-model.number="minScore" type="range" min="0" max="0.9" step="0.01" class="mt-1 w-full" />
            </label>
            <label class="block text-[0.7rem] text-slate-400">
              Min margin: <span class="tabular-nums text-slate-200">{{ minMargin.toFixed(3) }}</span>
              <input v-model.number="minMargin" type="range" min="0" max="0.3" step="0.005" class="mt-1 w-full" />
            </label>
            <label class="block text-[0.7rem] text-slate-400">
              Min sharpness: <span class="tabular-nums text-slate-200">{{ minSharpness.toFixed(0) }}</span>
              <input v-model.number="minSharpness" type="range" min="0" max="60" step="1" class="mt-1 w-full" />
            </label>
          </div>
        </div>

        <div v-if="sightings.length" class="space-y-2">
          <div class="flex items-center justify-between">
            <span class="text-xs text-slate-400">Seen this session</span>
            <button
              class="text-xs text-slate-400 underline underline-offset-2 hover:text-slate-200"
              @click="sightings = []"
            >
              Clear
            </button>
          </div>
          <ul class="max-h-64 space-y-1 overflow-y-auto pr-1">
            <li
              v-for="s in sightings"
              :key="s.card.baseId"
              class="flex items-baseline justify-between gap-2 rounded bg-slate-800/60 px-2 py-1.5"
            >
              <span class="min-w-0 flex-1 truncate text-xs text-slate-100">{{ s.card.name }}</span>
              <span class="shrink-0 text-[0.65rem] text-slate-400">{{ s.card.set }}</span>
              <span class="shrink-0 text-[0.65rem] tabular-nums text-emerald-400">
                ×{{ s.votes }}
              </span>
            </li>
          </ul>
        </div>
        <p v-else class="text-xs text-slate-500">
          Nothing identified yet. Hold a card steady in frame.
        </p>
      </section>

      <p class="text-xs leading-relaxed text-slate-500">
        Corner colours follow the card's own frame: red = top-left, green = top-right,
        blue = bottom-right, yellow = bottom-left. If they rotate with the card, the
        model has learnt orientation and un-warping for identification will work.
      </p>
    </aside>
  </div>
</template>
