<script setup lang="ts">
import { useAccountStore } from "~/stores/AccountStore";
import { DoServerFetch } from "~/helpers/RequestsHelper";
import { GetWebpUrl } from "~/helpers/CropUrlHelper";
import SetService from "~/services/SetService";
import type { SetViewModel } from "~/api/default";

const accountStore = useAccountStore();
const setService = new SetService();

interface QueueItem {
  id: number;
  siteId: number;
  status: string;
  sourceType: string;
  sourceUrl: string | null;
  imageUrl: string;
  hasBackImage: boolean;
  backImageUrl: string | null;
  potentialDuplicateId: number | null;
  matchedCardName: string | null;
  createdAt: string;
  setId: number | null;
  extractedData: Record<string, string>;
  templatedFields: string[];
}

interface PresetField {
  alias: string;
  templated: boolean;
}

interface PresetVariant {
  variantTypeId: number;
  name: string;
  fields: PresetField[];
}

interface Preset {
  id: number;
  name: string;
  variants: PresetVariant[];
}

interface RematchResult {
  status: string;
  potentialDuplicateId: number | null;
  matchedCardName: string | null;
}

type ItemMode = "new" | "variant";

const items = ref<QueueItem[]>([]);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);
const editingData = ref<Record<number, Record<string, string>>>({});
const selectedSets = ref<Record<number, number | null>>({});
const sets = ref<SetViewModel[]>([]);
const presets = ref<Preset[]>([]);
const itemMode = ref<Record<number, ItemMode>>({});
const selectedPreset = ref<Record<number, number | null>>({});
// item id -> variant type id -> editable field values
const variantData = ref<Record<number, Record<number, Record<string, string>>>>({});

interface SubmitResult {
  total: number;
  succeeded: number;
  failed: number;
  results: { index: number; success: boolean; error: string | null }[];
}

const submitFiles = ref<File[]>([]);
const submitSourceUrl = ref("");
const isSubmitting = ref(false);
const submitMessage = ref<string | null>(null);
const submitError = ref<string | null>(null);
const fileInputKey = ref(0);

function onFileChange(event: Event) {
  const target = event.target as HTMLInputElement;
  submitFiles.value = target.files ? Array.from(target.files) : [];
}

function readFileAsBase64(
  file: File
): Promise<{ base64: string; mimeType: string }> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      // Strip the "data:<mime>;base64," prefix — the backend expects raw base64.
      const base64 = result.slice(result.indexOf(",") + 1);
      resolve({ base64, mimeType: file.type || "image/png" });
    };
    reader.onerror = () => reject(reader.error);
    reader.readAsDataURL(file);
  });
}

async function submitManual() {
  if (submitFiles.value.length === 0) return;
  isSubmitting.value = true;
  submitMessage.value = null;
  submitError.value = null;
  try {
    const images = await Promise.all(
      submitFiles.value.map(async (file) => {
        const { base64, mimeType } = await readFileAsBase64(file);
        return { imageBase64: base64, mimeType };
      })
    );
    const result = await DoServerFetch<SubmitResult>(
      "/api/cardimportqueue/submitmanual",
      true,
      {
        method: "POST",
        body: {
          sourceUrl: submitSourceUrl.value || null,
          images,
        },
      }
    );
    if (result.failed > 0) {
      submitMessage.value = `${result.succeeded} of ${result.total} image(s) submitted. Processing may take a moment.`;
      submitError.value = `${result.failed} image(s) failed to process.`;
    } else {
      submitMessage.value = `${result.succeeded} image(s) submitted. Processing may take a moment.`;
    }
    submitFiles.value = [];
    submitSourceUrl.value = "";
    fileInputKey.value++;
    await loadPending();
  } catch {
    submitError.value = "Failed to submit the images.";
  } finally {
    isSubmitting.value = false;
  }
}

async function loadPending() {
  isLoading.value = true;
  errorMessage.value = null;
  try {
    const result = await DoServerFetch<QueueItem[]>(
      "/api/cardimportqueue/pending",
      true
    );
    items.value = result ?? [];
    for (const item of items.value) {
      editingData.value[item.id] = { ...item.extractedData };
      selectedSets.value[item.id] = item.setId ?? null;
      itemMode.value[item.id] =
        item.status === "PotentialVariant" ? "variant" : "new";
      selectedPreset.value[item.id] = null;
      variantData.value[item.id] = {};
    }
  } catch {
    errorMessage.value = "Failed to load the import queue.";
  } finally {
    isLoading.value = false;
  }
}

async function loadSets() {
  try {
    sets.value = (await setService.getAllSets()) ?? [];
  } catch {
    // Non-fatal: the set dropdown will simply be empty.
    sets.value = [];
  }
}

async function loadPresets() {
  try {
    presets.value =
      (await DoServerFetch<Preset[]>("/api/cardimportqueue/presets", true)) ?? [];
  } catch {
    // Non-fatal: the variant picker will simply be empty.
    presets.value = [];
  }
}

function selectedPresetFor(item: QueueItem): Preset | undefined {
  const presetId = selectedPreset.value[item.id];
  if (presetId === null || presetId === undefined) return undefined;
  return presets.value.find((p) => p.id === presetId);
}

// When a preset is picked, prepare editable data for every variant it contains, autofilling
// each field from the AI read (case-insensitive) and leaving the rest empty. Templated fields
// are read-only and computed server-side, so they are not collected here.
function onPresetSelected(item: QueueItem) {
  const byVariant: Record<number, Record<string, string>> = {};
  const preset = selectedPresetFor(item);

  if (preset) {
    const aiByLowerKey: Record<string, string> = {};
    for (const [key, value] of Object.entries(item.extractedData)) {
      aiByLowerKey[key.toLowerCase()] = value;
    }

    for (const variant of preset.variants) {
      const data: Record<string, string> = {};
      for (const field of variant.fields) {
        if (field.templated) continue;
        data[field.alias] = aiByLowerKey[field.alias.toLowerCase()] ?? "";
      }
      byVariant[variant.variantTypeId] = data;
    }
  }

  variantData.value[item.id] = byVariant;
}

function modeButtonClass(item: QueueItem, mode: ItemMode) {
  const active = (itemMode.value[item.id] ?? "new") === mode;
  return active
    ? "bg-blue-600 text-white px-3 py-1 rounded text-sm font-medium"
    : "bg-gray-100 border border-gray-300 text-gray-700 px-3 py-1 rounded text-sm hover:bg-gray-200";
}

function canApprove(item: QueueItem) {
  if (!selectedSets.value[item.id]) return false;
  if ((itemMode.value[item.id] ?? "new") === "variant")
    return (
      selectedPreset.value[item.id] !== null &&
      selectedPreset.value[item.id] !== undefined
    );
  return true;
}

async function approve(item: QueueItem) {
  const setId = selectedSets.value[item.id];
  if (!setId) return;

  if ((itemMode.value[item.id] ?? "new") === "variant") {
    const preset = selectedPresetFor(item);
    if (!preset) return;
    const variants = preset.variants.map((v) => ({
      variantTypeId: v.variantTypeId,
      properties: variantData.value[item.id]?.[v.variantTypeId] ?? {},
    }));
    await DoServerFetch<void>(
      `/api/cardimportqueue/approve?id=${item.id}&setId=${setId}`,
      true,
      {
        method: "POST",
        body: { parentId: item.potentialDuplicateId, variants },
      }
    );
  } else {
    await saveEdits(item, editingData.value[item.id]);
    await DoServerFetch<void>(
      `/api/cardimportqueue/approve?id=${item.id}&setId=${setId}`,
      true,
      { method: "POST", body: {} }
    );
  }
  items.value = items.value.filter((i) => i.id !== item.id);
}

async function reject(id: number) {
  await DoServerFetch<void>(`/api/cardimportqueue/reject?id=${id}`, true, {
    method: "POST",
  });
  items.value = items.value.filter((i) => i.id !== id);
}

async function saveEdits(item: QueueItem, data?: Record<string, string>) {
  const payload = data ?? editingData.value[item.id];
  if (!payload) return;
  await DoServerFetch<void>(`/api/cardimportqueue/updatedata?id=${item.id}`, true, {
    method: "POST",
    body: payload,
  });
}

// Re-run the exact-name match after the admin edited the name, in case the system
// missed an existing base card. On a hit the item becomes a potential variant.
async function rematch(item: QueueItem) {
  await saveEdits(item, editingData.value[item.id]);
  const result = await DoServerFetch<RematchResult>(
    `/api/cardimportqueue/rematch?id=${item.id}`,
    true,
    { method: "POST" }
  );
  item.status = result.status;
  item.potentialDuplicateId = result.potentialDuplicateId;
  item.matchedCardName = result.matchedCardName;
  itemMode.value[item.id] =
    result.status === "PotentialVariant" ? "variant" : "new";
  selectedPreset.value[item.id] = null;
  variantData.value[item.id] = {};
}

function proxyImageUrl(url: string) {
  // Same-origin proxy path (must NOT go through the umbraco image provider,
  // which would prepend the API base URL). Append WebP via GetWebpUrl instead.
  return GetWebpUrl(`/api/proxy${url}`);
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString();
}

onMounted(async () => {
  await accountStore.checkLogin();
  if (!accountStore.member?.isAdmin) {
    navigateTo("/");
    return;
  }
  await Promise.all([loadSets(), loadPresets(), loadPending()]);
});
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold">Card Import Queue</h1>
      <button
        class="bg-gray-100 border border-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-200 text-sm"
        :disabled="isLoading"
        @click="loadPending"
      >
        {{ isLoading ? "Loading…" : "Refresh" }}
      </button>
    </div>

    <!-- Manual submit -->
    <div class="bg-white rounded shadow p-4 mb-6">
      <h2 class="text-lg font-semibold mb-3">Submit reveal images manually</h2>
      <div class="flex flex-col gap-3 max-w-xl">
        <div class="flex flex-col">
          <label class="text-xs font-semibold text-gray-500 mb-1">
            Images (you can select multiple)
          </label>
          <input
            :key="fileInputKey"
            type="file"
            accept="image/*"
            multiple
            class="text-sm"
            @change="onFileChange"
          />
          <p class="text-xs text-gray-400 mt-1">
            Images are processed in the order shown below. For double-sided cards (Leaders),
            place the front image immediately before its back image.
          </p>
          <ol
            v-if="submitFiles.length"
            class="text-xs text-gray-500 mt-2 list-decimal list-inside space-y-0.5"
          >
            <li v-for="(file, index) in submitFiles" :key="index">{{ file.name }}</li>
          </ol>
        </div>
        <div class="flex flex-col">
          <label class="text-xs font-semibold text-gray-500 mb-1">
            Source URL (optional)
          </label>
          <input
            v-model="submitSourceUrl"
            type="text"
            placeholder="https://…"
            class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
        </div>
        <div class="flex items-center gap-3">
          <button
            class="bg-blue-600 text-white px-4 py-1.5 rounded hover:bg-blue-700 text-sm font-medium disabled:opacity-50"
            :disabled="submitFiles.length === 0 || isSubmitting"
            @click="submitManual"
          >
            {{ isSubmitting ? "Submitting…" : "Submit" }}
          </button>
          <span v-if="submitMessage" class="text-green-600 text-sm">{{ submitMessage }}</span>
          <span v-if="submitError" class="text-red-600 text-sm">{{ submitError }}</span>
        </div>
      </div>
    </div>

    <p v-if="errorMessage" class="text-red-600 mb-4">{{ errorMessage }}</p>

    <p v-if="!isLoading && items.length === 0" class="text-gray-500">
      No pending items.
    </p>

    <div class="space-y-6">
      <div
        v-for="item in items"
        :key="item.id"
        class="bg-white rounded shadow p-4 flex gap-6"
        :class="{ 'border-l-4 border-yellow-400': item.status === 'PotentialVariant' }"
      >
        <!-- Card image(s) -->
        <div class="flex-shrink-0 flex flex-col gap-2">
          <img
            :src="proxyImageUrl(item.imageUrl)"
            alt="Card front preview"
            class="w-40 rounded shadow-sm object-contain"
          />
          <div v-if="item.hasBackImage && item.backImageUrl" class="flex flex-col">
            <img
              :src="proxyImageUrl(item.backImageUrl)"
              alt="Card back preview"
              class="w-40 rounded shadow-sm object-contain"
            />
            <span class="text-xs text-gray-400 text-center mt-0.5">Back</span>
          </div>
        </div>

        <!-- Details + editable fields -->
        <div class="flex-1 min-w-0">
          <!-- Meta row -->
          <div class="flex items-center gap-3 mb-3 flex-wrap">
            <span
              v-if="item.matchedCardName"
              class="bg-yellow-100 text-yellow-800 text-xs font-semibold px-2 py-0.5 rounded"
            >
              Possible variant of: {{ item.matchedCardName }} (#{{ item.potentialDuplicateId }})
            </span>
            <span class="text-xs text-gray-500">{{ item.sourceType }}</span>
            <a
              v-if="item.sourceUrl"
              :href="item.sourceUrl"
              target="_blank"
              rel="noopener"
              class="text-xs text-blue-500 hover:underline truncate max-w-xs"
            >
              {{ item.sourceUrl }}
            </a>
            <span class="text-xs text-gray-400 ml-auto">{{ formatDate(item.createdAt) }}</span>
          </div>

          <!-- New card vs variant choice (only when a base card was matched) -->
          <div v-if="item.matchedCardName" class="flex flex-wrap gap-2 mb-4">
            <button :class="modeButtonClass(item, 'new')" @click="itemMode[item.id] = 'new'">
              New card (not a variant)
            </button>
            <button :class="modeButtonClass(item, 'variant')" @click="itemMode[item.id] = 'variant'">
              Variant of {{ item.matchedCardName }}
            </button>
          </div>

          <!-- NEW CARD mode: editable AI-read fields -->
          <template v-if="(itemMode[item.id] ?? 'new') === 'new'">
            <div class="grid grid-cols-2 gap-x-4 gap-y-2 mb-4">
              <div
                v-for="(value, key) in editingData[item.id]"
                :key="key"
                class="flex flex-col"
              >
                <label class="text-xs font-semibold text-gray-500 mb-0.5">{{ key }}</label>
                <input
                  v-model="editingData[item.id]![key]"
                  type="text"
                  class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                />
              </div>

              <!-- Read-only templated fields (computed on approve) -->
              <div
                v-for="field in item.templatedFields"
                :key="`templated-${field}`"
                class="flex flex-col"
              >
                <label class="text-xs font-semibold text-gray-500 mb-0.5">{{ field }}</label>
                <input
                  type="text"
                  disabled
                  value="Computed on approve"
                  class="border border-gray-200 bg-gray-100 text-gray-400 italic rounded px-2 py-1 text-sm"
                />
              </div>
            </div>
          </template>

          <!-- VARIANT mode: pick a preset, then edit the fields of each of its variants -->
          <template v-else>
            <div class="flex flex-col mb-4 max-w-xs">
              <label class="text-xs font-semibold text-gray-500 mb-0.5">
                Preset <span class="text-red-500">*</span>
              </label>
              <select
                v-model="selectedPreset[item.id]"
                class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                @change="onPresetSelected(item)"
              >
                <option :value="null" disabled>Select a preset…</option>
                <option v-for="preset in presets" :key="preset.id" :value="preset.id">
                  {{ preset.name }}
                </option>
              </select>
            </div>

            <!-- One field group per variant in the chosen preset; all are created on approval. -->
            <div
              v-for="variant in selectedPresetFor(item)?.variants ?? []"
              :key="variant.variantTypeId"
              class="border border-gray-200 rounded p-3 mb-3"
            >
              <h4 class="text-sm font-semibold text-gray-700 mb-2">{{ variant.name }}</h4>
              <div class="grid grid-cols-2 gap-x-4 gap-y-2">
                <div
                  v-for="field in variant.fields"
                  :key="field.alias"
                  class="flex flex-col"
                >
                  <label class="text-xs font-semibold text-gray-500 mb-0.5">{{ field.alias }}</label>
                  <input
                    v-if="field.templated"
                    type="text"
                    disabled
                    value="Computed on approve"
                    class="border border-gray-200 bg-gray-100 text-gray-400 italic rounded px-2 py-1 text-sm"
                  />
                  <input
                    v-else
                    v-model="variantData[item.id]![variant.variantTypeId]![field.alias]"
                    type="text"
                    class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                  />
                </div>
              </div>
            </div>
          </template>

          <!-- Set selection -->
          <div class="flex flex-col mb-4 max-w-xs">
            <label class="text-xs font-semibold text-gray-500 mb-0.5">
              Set <span class="text-red-500">*</span>
            </label>
            <select
              v-model="selectedSets[item.id]"
              class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            >
              <option :value="null" disabled>Select a set…</option>
              <option v-for="set in sets" :key="set.id" :value="set.id">
                {{ set.displayName }}{{ set.code ? ` (${set.code})` : "" }}
              </option>
            </select>
          </div>

          <!-- Actions -->
          <div class="flex items-center gap-3 flex-wrap">
            <button
              class="bg-green-600 text-white px-4 py-1.5 rounded hover:bg-green-700 text-sm font-medium disabled:opacity-50 disabled:cursor-not-allowed"
              :disabled="!canApprove(item)"
              @click="approve(item)"
            >
              Approve
            </button>
            <button
              class="bg-red-500 text-white px-4 py-1.5 rounded hover:bg-red-600 text-sm font-medium"
              @click="reject(item.id)"
            >
              Reject
            </button>
            <button
              v-if="(itemMode[item.id] ?? 'new') === 'new'"
              class="bg-gray-100 border border-gray-300 text-gray-700 px-4 py-1.5 rounded hover:bg-gray-200 text-sm"
              title="Edit the name above, then rematch to link it to an existing card"
              @click="rematch(item)"
            >
              Rematch
            </button>
            <span v-if="!selectedSets[item.id]" class="text-xs text-gray-400">
              A set is required to approve
            </span>
            <span
              v-else-if="(itemMode[item.id] ?? 'new') === 'variant' && selectedPreset[item.id] === null"
              class="text-xs text-gray-400"
            >
              Select a preset to approve
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
