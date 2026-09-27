<script setup lang="ts">
import type { CardDetector, Detection } from "#card-scanner/detector";
import type { CardMatcher, IndexCard, MatchResult, RejectReason } from "#card-scanner/matcher";
import { useScannerModels } from "~/composables/useScannerModels";
import { SightingTracker } from "~/services/scanner/SightingTracker";

const MAX_IDENTIFY_PER_FRAME = 4;
const COMMIT_AFTER_FRAMES = 3;
const FORGET_AFTER_MISSED_FRAMES = 5;

const REASON_TEXT: Record<Exclude<RejectReason, null>, string> = {
  blurry: "too blurry",
  "low-score": "no close match",
  "low-margin": "two cards too alike",
  "no-index": "index not loaded",
};

const props = defineProps<{
  paused: boolean;
}>();

const emit = defineEmits<{
  (e: "scan", card: IndexCard, added: number): void;
}>();

const video = ref<HTMLVideoElement | null>(null);
const overlay = ref<HTMLCanvasElement | null>(null);

const detector = shallowRef<CardDetector>();
const matcher = shallowRef<CardMatcher>();
const loadState = ref<"loading" | "ready" | "error">("loading");
const cameraState = ref<"starting" | "live" | "error">("starting");
const errorMessage = ref("");

const detections = shallowRef<Detection[]>([]);
const matches = shallowRef<MatchResult[]>([]);
const detectMs = ref(0);
const identifyMs = ref(0);

const tracker = new SightingTracker<IndexCard>({
  commitAfter: COMMIT_AFTER_FRAMES,
  forgetAfter: FORGET_AFTER_MISSED_FRAMES,
});

let stream: MediaStream | null = null;
let rafId = 0;
let inFlight = false;
let frameCanvas: HTMLCanvasElement | undefined;
let frameCtx: CanvasRenderingContext2D | null = null;

function describeError(err: unknown) {
  return err instanceof Error ? err.message : String(err);
}

async function loadModels() {
  loadState.value = "loading";
  try {
    const models = await useScannerModels();
    detector.value = models.detector;
    matcher.value = models.matcher;
    loadState.value = "ready";
  } catch (err) {
    loadState.value = "error";
    errorMessage.value = `Could not load the scanner models. ${describeError(err)}`;
  }
}

async function startCamera() {
  cameraState.value = "starting";
  errorMessage.value = "";
  try {
    stream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: { ideal: "environment" }, width: { ideal: 1280 }, height: { ideal: 720 } },
      audio: false,
    });
    const el = video.value!;
    el.srcObject = stream;
    await el.play();
    cameraState.value = "live";
    loop();
  } catch (err) {
    cameraState.value = "error";
    errorMessage.value =
      err instanceof Error && err.name === "NotAllowedError"
        ? "Camera permission denied. Allow it in the app settings and try again."
        : describeError(err);
  }
}

function stopCamera() {
  cancelAnimationFrame(rafId);
  rafId = 0;
  stream?.getTracks().forEach((track) => track.stop());
  stream = null;
}

// Detect and identify on one captured frame: on a slow phone the card moves between the two otherwise.
function captureFrame(el: HTMLVideoElement) {
  if (!frameCanvas) {
    frameCanvas = document.createElement("canvas");
    frameCtx = frameCanvas.getContext("2d", { willReadFrequently: true });
  }
  if (frameCanvas.width !== el.videoWidth || frameCanvas.height !== el.videoHeight) {
    frameCanvas.width = el.videoWidth;
    frameCanvas.height = el.videoHeight;
  }
  frameCtx?.drawImage(el, 0, 0);
  return frameCanvas;
}

async function scan(el: HTMLVideoElement) {
  const cardDetector = detector.value!;
  const frame = captureFrame(el);

  const found = await cardDetector.detect(frame, frame.width, frame.height);
  detectMs.value = cardDetector.lastInferenceMs;

  const results: MatchResult[] = [];
  const cardMatcher = matcher.value;
  if (cardMatcher?.ready && found.length && frameCtx) {
    const pixels = frameCtx.getImageData(0, 0, frame.width, frame.height);
    const started = performance.now();
    for (const detection of found.slice(0, MAX_IDENTIFY_PER_FRAME)) {
      results.push(await cardMatcher.identify(pixels, detection.corners));
    }
    identifyMs.value = performance.now() - started;
  }

  // A result that arrives after pausing belongs to a frame the user has already moved on from.
  if (props.paused) {
    return;
  }

  detections.value = found;
  matches.value = results;

  const accepted = results.flatMap((match) => (match.accepted && match.card ? [match.card] : []));
  for (const event of tracker.observe(accepted)) {
    emit("scan", event.card, event.added);
  }
}

function loop() {
  rafId = requestAnimationFrame(loop);

  const el = video.value;
  if (!el || el.readyState < 2 || !el.videoWidth) {
    return;
  }

  if (!props.paused && detector.value?.ready && !inFlight) {
    inFlight = true;
    scan(el)
      .catch((err) => {
        errorMessage.value = describeError(err);
      })
      .finally(() => {
        inFlight = false;
      });
  }

  draw(el);
}

function draw(el: HTMLVideoElement) {
  const canvas = overlay.value;
  const ctx = canvas?.getContext("2d");
  if (!canvas || !ctx) {
    return;
  }

  const rect = el.getBoundingClientRect();
  const dpr = window.devicePixelRatio || 1;
  const width = Math.round(rect.width * dpr);
  const height = Math.round(rect.height * dpr);
  if (canvas.width !== width || canvas.height !== height) {
    canvas.width = width;
    canvas.height = height;
  }
  ctx.clearRect(0, 0, width, height);

  // Mirror `object-cover` so the overlay lands on the video pixels that are actually on screen.
  const scale = Math.max(width / el.videoWidth, height / el.videoHeight);
  const offsetX = (width - el.videoWidth * scale) / 2;
  const offsetY = (height - el.videoHeight * scale) / 2;
  const toCanvas = ([x, y]: [number, number]): [number, number] => [x * scale + offsetX, y * scale + offsetY];

  const fontSize = 13 * dpr;
  const padding = 4 * dpr;
  ctx.font = `${fontSize}px ui-sans-serif, system-ui, sans-serif`;
  ctx.lineWidth = Math.max(2, 2 * dpr);

  detections.value.forEach((detection, index) => {
    const match = matches.value[index];
    const card = match?.accepted ? match.card : null;
    const confirmed = card ? tracker.isConfirmed(card.baseId) : false;
    const corners = detection.corners.map(toCanvas);
    const topLeft = corners[0];
    if (!topLeft) {
      return;
    }

    ctx.beginPath();
    corners.forEach(([x, y], i) => (i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y)));
    ctx.closePath();
    ctx.fillStyle = confirmed ? "rgba(52, 211, 153, 0.18)" : card ? "rgba(251, 191, 36, 0.16)" : "rgba(56, 189, 248, 0.14)";
    ctx.fill();
    ctx.strokeStyle = confirmed ? "#34d399" : card ? "#fbbf24" : "#38bdf8";
    ctx.stroke();

    const label = card
      ? confirmed
        ? card.name
        : `${card.name}…`
      : match?.reason
        ? REASON_TEXT[match.reason]
        : "";
    if (!label) {
      return;
    }
    const boxHeight = fontSize + padding * 2;
    ctx.fillStyle = "rgba(0, 0, 0, 0.75)";
    ctx.fillRect(topLeft[0], topLeft[1] - boxHeight, ctx.measureText(label).width + padding * 2, boxHeight);
    ctx.fillStyle = confirmed ? "#a7f3d0" : "#fde68a";
    ctx.fillText(label, topLeft[0] + padding, topLeft[1] - padding - fontSize * 0.15);
  });
}

watch(
  () => props.paused,
  (paused) => {
    if (paused) {
      video.value?.pause();
      detections.value = [];
      matches.value = [];
    } else {
      void video.value?.play();
    }
  }
);

onMounted(() => {
  void loadModels();
  void startCamera();
});

onBeforeUnmount(stopCamera);
</script>

<template>
  <div class="fixed inset-0 z-50 bg-black">
    <video ref="video" class="h-full w-full object-cover" playsinline muted autoplay />
    <canvas ref="overlay" class="pointer-events-none absolute inset-0 h-full w-full" />

    <div
      v-if="loadState !== 'ready' || cameraState !== 'live' || errorMessage"
      class="absolute inset-x-6 top-1/3 rounded-lg bg-black/70 p-4 text-center text-sm text-white"
    >
      <p v-if="errorMessage" class="text-red-300">{{ errorMessage }}</p>
      <p v-else-if="cameraState === 'starting'">Starting camera…</p>
      <p v-else>Loading scanner models…</p>
      <button
        v-if="cameraState === 'error'"
        class="mt-3 rounded-md bg-main-color px-4 py-2 font-semibold text-white"
        @click="startCamera"
      >
        Try again
      </button>
    </div>

    <p
      v-if="loadState === 'ready' && cameraState === 'live'"
      class="absolute left-3 top-safe-top mt-3 rounded bg-black/60 px-2 py-1 text-[11px] text-white/80 tabular-nums"
    >
      {{ detector?.backend }} · {{ detectMs.toFixed(0) }} + {{ identifyMs.toFixed(0) }} ms
    </p>

    <slot />
  </div>
</template>
