<script setup lang="ts">
import { PhCamera, PhMinus, PhPlus, PhTrash } from "@phosphor-icons/vue";
import CmsImage from "~/components/shared/CmsImage.vue";
import ScanVariantSelect from "~/components/scanner/ScanVariantSelect.vue";
import { useScanQueue } from "~/composables/useScanQueue";

defineProps<{
  importing: boolean;
}>();

const emit = defineEmits<{
  (e: "resume"): void;
  (e: "import"): void;
}>();

const { entries, remove, optionsFor } = useScanQueue();
</script>

<template>
  <div class="pb-28">
    <div class="bg-white border-b border-gray-200 px-4 py-4">
      <h1 class="text-lg font-semibold">Scanned cards</h1>
      <p class="text-sm text-gray-500">
        Check the variant and count of each card before adding them to your collection.
      </p>
    </div>

    <p v-if="!entries.length" class="px-4 py-8 text-center text-gray-500">No cards scanned yet.</p>

    <ul v-else class="bg-white border-b border-gray-200 divide-y divide-gray-100">
      <li v-for="entry in entries" :key="entry.id" class="flex items-center gap-3 px-4 py-3">
        <CmsImage :src="entry.imageUrl" width="48" class="w-12 shrink-0 rounded" />
        <div class="min-w-0 flex-1 space-y-2">
          <div>
            <p class="truncate font-medium">{{ entry.name }}</p>
            <p class="truncate text-xs text-gray-500">{{ entry.set }}</p>
          </div>
          <div class="flex items-center gap-2">
            <ScanVariantSelect
              v-model="entry.variantId"
              :options="optionsFor(entry)"
              class="flex-1 max-w-none border-gray-300 bg-white"
            />
            <div class="flex items-center rounded-md border border-gray-300">
              <button
                class="p-2 disabled:opacity-30"
                :disabled="entry.amount <= 1"
                aria-label="Decrease count"
                @click="entry.amount--"
              >
                <PhMinus :size="14" />
              </button>
              <span class="w-6 text-center text-sm tabular-nums">{{ entry.amount }}</span>
              <button class="p-2" aria-label="Increase count" @click="entry.amount++">
                <PhPlus :size="14" />
              </button>
            </div>
            <button class="p-2 text-red-600" aria-label="Remove card" @click="remove(entry)">
              <PhTrash :size="18" />
            </button>
          </div>
        </div>
      </li>
    </ul>

    <div class="fixed inset-x-0 bottom-0 z-40 flex gap-2 border-t border-gray-200 bg-white px-4 pt-3 pb-safe-bottom">
      <button
        class="mb-3 flex flex-1 items-center justify-center gap-2 rounded-md border border-gray-300 px-3 py-2.5 font-semibold"
        @click="emit('resume')"
      >
        <PhCamera :size="18" />
        {{ entries.length ? "Resume scanning" : "Start scanning" }}
      </button>
      <button
        class="mb-3 flex-1 rounded-md bg-main-color px-3 py-2.5 font-semibold text-white disabled:opacity-50"
        :disabled="!entries.length || importing"
        @click="emit('import')"
      >
        {{ importing ? "Adding…" : "Add to collection" }}
      </button>
    </div>
  </div>
</template>
