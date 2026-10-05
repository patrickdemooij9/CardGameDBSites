<script setup lang="ts">
import type { IndexCard } from "#card-scanner/matcher";
import { PhListBullets, PhX } from "@phosphor-icons/vue";
import ScannerCamera from "~/components/scanner/ScannerCamera.vue";
import ScanReview from "~/components/scanner/ScanReview.vue";
import ScanVariantSelect from "~/components/scanner/ScanVariantSelect.vue";
import CmsImage from "~/components/shared/CmsImage.vue";
import { useAppToast } from "~/composables/useAppToast";
import { useScanQueue } from "~/composables/useScanQueue";

type Mode = "scanning" | "paused";

const { entries, add, optionsFor, importAll } = useScanQueue();
const toast = useAppToast();

const mode = ref<Mode>("scanning");
const latestId = ref<number>();
const scanCount = ref(0);
const importing = ref(false);

const latest = computed(() => entries.value.find((entry) => entry.id === latestId.value));
const totalCards = computed(() => entries.value.reduce((sum, entry) => sum + entry.amount, 0));

async function onScan(card: IndexCard, added: number) {
  const id = await add(card, added);
  if (id !== undefined) {
    latestId.value = id;
    scanCount.value++;
  }
}

function resume() {
  mode.value = "scanning";
}

function exit() {
  navigateTo("/app/more");
}

async function importCards() {
  importing.value = true;
  try {
    const imported = await importAll();
    toast.success(`Added ${imported} card${imported === 1 ? "" : "s"} to your collection`);
  } catch {
    toast.error("Could not add the cards to your collection. They are still in the list.");
  } finally {
    importing.value = false;
  }
}
</script>

<template>
  <div>
    <ScannerCamera v-show="mode === 'scanning'" :paused="mode !== 'scanning'" @scan="onScan">
      <div class="absolute right-3 top-safe-top mt-3 flex gap-2">
        <button
          class="relative rounded-full bg-black/60 p-3 text-white"
          aria-label="Review scanned cards"
          @click="mode = 'paused'"
        >
          <PhListBullets :size="22" />
          <span
            v-if="totalCards"
            class="absolute -right-1 -top-1 min-w-5 rounded-full bg-main-color px-1.5 text-center text-xs font-semibold leading-5"
          >
            {{ totalCards }}
          </span>
        </button>
        <button class="rounded-full bg-black/60 p-3 text-white" aria-label="Close scanner" @click="exit">
          <PhX :size="22" />
        </button>
      </div>

      <div class="absolute inset-x-0 bottom-0 px-3 pb-safe-bottom">
        <Transition
          mode="out-in"
          enter-active-class="transition duration-300 ease-out"
          enter-from-class="translate-y-full opacity-0"
          leave-active-class="transition duration-150 ease-in"
          leave-to-class="opacity-0"
        >
          <div
            v-if="latest"
            :key="scanCount"
            class="mb-3 flex items-center gap-3 rounded-xl bg-white/95 p-2 pr-3 shadow-lg"
          >
            <CmsImage :src="latest.imageUrl" width="40" class="w-10 shrink-0 rounded" />
            <div class="min-w-0 flex-1">
              <p class="truncate font-medium">{{ latest.name }}</p>
              <p class="truncate text-xs text-gray-500">{{ latest.set }} · ×{{ latest.amount }}</p>
            </div>
            <ScanVariantSelect v-model="latest.variantId" :options="optionsFor(latest)" class="border-gray-300 bg-white" />
          </div>
          <p v-else class="mb-3 rounded-xl bg-black/60 p-3 text-center text-sm text-white">
            Hold a card in front of the camera
          </p>
        </Transition>
      </div>
    </ScannerCamera>

    <ScanReview v-if="mode !== 'scanning'" :importing="importing" @resume="resume" @import="importCards" @exit="exit" />
  </div>
</template>
