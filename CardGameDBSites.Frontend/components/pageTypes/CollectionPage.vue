<script setup lang="ts">
import {
  type CollectionSummaryApiModel,
  type SetViewModel,
  type CardVariantTypeApiModel,
} from "~/api/default";
import { DoServerFetch } from "~/helpers/RequestsHelper";
import { useAppToast } from "~/composables/useAppToast";
import SetOverview from "../overviews/SetOverview.vue";
import ProgressBar from "../shared/ProgressBar.vue";
import PopupBase from "../popups/PopupBase.vue";
import CardPackModal from "../popups/CardPackModal.vue";
import { PopupSize } from "../popups/PopupTypes";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";

const accountStore = useAccountStore();
const isLoggedIn = await useAccountStore().checkLogin();
const appToast = useAppToast();
const showProgressBar = true;
const progressPercent = 44.5;
const isLoading = ref(true);
const showExportPopup = ref(false);
const showImportPopup = ref(false);
const showPackPopup = ref(false);
const isImporting = ref(false);
const importError = ref("");
const selectedFile = ref<File | null>(null);
const overwriteCollection = ref(false);

const sets = ref<SetViewModel[]>([]);
const variantTypes = ref<CardVariantTypeApiModel[]>([]);

const summaryData = ref<CollectionSummaryApiModel>({
  uniqueCards: 0,
  totalCards: 0,
  packsOpened: 0,
  marketPrice: 0,
});

const exportTypes = [
  {
    value: "Grouped",
    label: "Grouped",
    description: "Export cards grouped by set",
  },
  {
    value: "Detailed",
    label: "Detailed",
    description: "Export with detailed variant info",
  },
];

async function handleExport(exportType: string) {
  window.location.href = `/api/collection/export?exportType=${exportType}`;
  showExportPopup.value = false;
}

function handleFileSelect(event: Event) {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0];
    importError.value = "";
  }
}

async function handleImport() {
  if (!selectedFile.value) {
    importError.value = "Please select a file";
    return;
  }

  isImporting.value = true;
  importError.value = "";

  try {
    const formData = new FormData();
    formData.append("file", selectedFile.value);

    await DoServerFetch(
      `/api/collection/import?overwrite=${overwriteCollection.value}`,
      true,
      {
        method: "POST",
        body: formData,
      },
    );

    appToast.success("Collection imported successfully!");
    showImportPopup.value = false;
    selectedFile.value = null;
    overwriteCollection.value = false;

    summaryData.value = await DoServerFetch<CollectionSummaryApiModel>(
      "/api/collection/summary",
    );
  } catch (error: any) {
    importError.value = error.message || "An error occurred during import";
  } finally {
    isImporting.value = false;
  }
}

onMounted(async () => {
  if (isLoggedIn) {
    summaryData.value = await DoServerFetch<CollectionSummaryApiModel>(
      "/api/collection/summary",
    );
    sets.value = await DoServerFetch<SetViewModel[]>("/api/sets/getAll");
  }
  variantTypes.value = await DoServerFetch<CardVariantTypeApiModel[]>(
    "/api/cards/variantTypes",
  );
  isLoading.value = false;
});

async function handlePackAdded() {
  summaryData.value = await DoServerFetch<CollectionSummaryApiModel>(
    "/api/collection/summary",
  );
}
</script>

<template>
  <div class="container px-4 md:px-8 py-4">
    <div v-if="isLoading" class="flex justify-center items-center py-12">
      <div
        class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"
      ></div>
    </div>
    <template v-else>
      <div class="flex md:flex-row md:items-center flex-col gap-6">
        <div v-if="showProgressBar" class="grow">
          <ProgressBar :percent-filled="progressPercent"></ProgressBar>
          <small>Your progress towards a full collection.</small>
        </div>
        <div class="flex gap-4" v-if="accountStore.isLoggedIn">
          <button class="btn" @click="showPackPopup = true">Add pack</button>
          <button class="btn" @click="showExportPopup = true">Export</button>
          <button class="btn" @click="showImportPopup = true">Import</button>
        </div>
      </div>
      <div
        class="flex md:flex-row flex-col mt-6 gap-4 justify-between text-white"
      >
        <div
          class="flex flex-col items-center px-4 py-2 w-full bg-red-600 rounded-md"
        >
          <p class="font-bold text-lg">{{ summaryData.uniqueCards }}</p>
          <p>Unique cards</p>
        </div>
        <div
          class="flex flex-col items-center px-4 py-2 w-full bg-blue-600 rounded-md"
        >
          <p class="font-bold text-lg">{{ summaryData.totalCards }}</p>
          <p>Total cards</p>
        </div>
        <div
          class="flex flex-col items-center px-4 py-2 w-full bg-green-600 rounded-md"
        >
          <p class="font-bold text-lg">{{ summaryData.packsOpened }}</p>
          <p>Packs opened</p>
        </div>
        <div
          class="flex flex-col items-center px-4 py-2 w-full bg-yellow-600 rounded-md"
        >
          <p class="font-bold text-lg">
            ${{ summaryData.marketPrice?.toFixed(2) ?? 0 }}
          </p>
          <p>Market price</p>
        </div>
      </div>
      <div class="mt-12">
        <div v-if="accountStore.isLoggedIn">
          <h2 class="text-lg font-bold">All sets</h2>
          <SetOverview />
        </div>
        <div v-else>
          <p class="text-lg font-bold text-center">
            <NuxtLink to="/login">Log in</NuxtLink> to keep track of your
            collection.
          </p>
        </div>
      </div>
    </template>
  </div>

  <PopupBase
    v-if="showExportPopup"
    :size="PopupSize.Small"
    @close="showExportPopup = false"
  >
    <h3 class="text-lg font-bold mb-4">Export Collection</h3>
    <div class="space-y-3">
      <p class="text-gray-600 mb-4">Choose an export type:</p>
      <div
        v-for="type in exportTypes"
        :key="type.value"
        class="border rounded-lg p-4 cursor-pointer hover:border-blue-500 hover:bg-blue-50 transition-colors"
        @click="handleExport(type.value)"
      >
        <div class="font-semibold">{{ type.label }}</div>
        <div class="text-sm text-gray-500">{{ type.description }}</div>
      </div>
    </div>
  </PopupBase>

  <PopupBase
    v-if="showImportPopup"
    :size="PopupSize.Small"
    @close="showImportPopup = false"
  >
    <h3 class="text-lg font-bold mb-4">Import Collection</h3>
    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-2"
          >Select file</label
        >
        <input
          type="file"
          accept=".xlsx,.csv"
          @change="handleFileSelect"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
        />
        <p class="text-xs text-gray-500 mt-1">Supported formats: .xlsx, .csv</p>
      </div>

      <div class="flex items-center">
        <input
          id="overwrite"
          type="checkbox"
          v-model="overwriteCollection"
          class="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
        />
        <label for="overwrite" class="ml-2 block text-sm text-gray-900">
          Overwrite existing collection
        </label>
      </div>

      <div v-if="importError" class="text-red-600 text-sm">
        {{ importError }}
      </div>
    </div>

    <template #actions>
      <Button
        @click="handleImport"
        :button-type="ButtonType.Success"
        :disabled="isImporting || !selectedFile"
        class="btn disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {{ isImporting ? "Importing..." : "Import" }}
      </Button>
    </template>
  </PopupBase>

  <CardPackModal
    v-if="showPackPopup"
    :sets="sets"
    :variant-types="variantTypes"
    @close="showPackPopup = false"
    @added="handlePackAdded"
  />
</template>
