<script setup lang="ts">
import { PhX } from "@phosphor-icons/vue";
import type { CardDetailApiModel } from "~/api/default";
import CardDetailAbility from "~/components/cards/CardDetailAbility.vue";
import CmsImage from "~/components/shared/CmsImage.vue";
import { useSite } from "~/composables/useSite";

const props = defineProps<{
  selectedCard: CardDetailApiModel;
  previousCardName?: string;
  nextCardName?: string;
}>();

const emit = defineEmits<{
  (e: "close"): void;
  (e: "previous"): void;
  (e: "next"): void;
}>();

const siteSettings = await useSite().getSettings();
</script>

<template>
  <Teleport to="#root">
    <div
      class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60"
      role="dialog"
      aria-modal="true"
    >
      <div
        class="relative bg-white rounded-lg shadow-lg md:w-[70vw] w-screen mx-4 max-h-screen overflow-auto"
      >
        <button
          class="absolute top-3 right-3 text-gray-400 hover:text-gray-700 transition-colors"
          aria-label="Close modal"
          @click.prevent="emit('close')"
        >
          <PhX class="h-6 w-6" />
        </button>

        <div class="flex md:flex-row flex-col gap-4 p-6 md:mt-0 mt-4">
          <div
            class="md:w-72 mb-4 flex items-center justify-center bg-gray-100 rounded shadow-inner"
          >
            <CmsImage
              :src="selectedCard.imageUrl"
              :alt="selectedCard.displayName ?? 'Card image'"
            >
              <template #fallback>
                <div class="text-sm text-gray-600">No image available</div>
              </template>
            </CmsImage>
          </div>

          <div>
            <h2 class="text-xl font-semibold mb-2">
              {{ selectedCard.displayName }}
            </h2>
            <div
              v-for="(element, index) in siteSettings.cardSections"
              :key="index"
              class="mb-2"
            >
              <div v-if="!element.isDivider">
                <CardDetailAbility :section="element" :card="selectedCard" />
              </div>
              <hr v-else class="mb-2" />
            </div>
          </div>
        </div>

        <div
          class="bg-gray-50 px-4 py-3 gap-2 sm:flex sm:flex-row-reverse sm:px-6"
        >
          <button
            type="button"
            class="inline-flex w-full justify-center rounded-md bg-main-color px-3 py-2 text-sm font-semibold text-white shadow-sm hover:opacity-90 disabled:opacity-50 sm:w-auto"
            :disabled="!nextCardName"
            @click="emit('next')"
          >
            {{ nextCardName ? `Next: ${nextCardName}` : "Next" }}
          </button>
          <button
            type="button"
            class="mt-3 sm:mt-0 inline-flex w-full justify-center rounded-md bg-main-color px-3 py-2 text-sm font-semibold text-white shadow-sm hover:opacity-90 disabled:opacity-50 sm:w-auto"
            :disabled="!previousCardName"
            @click="emit('previous')"
          >
            {{ previousCardName ? `Previous: ${previousCardName}` : "Previous" }}
          </button>
          <button
            type="button"
            @click="emit('close')"
            class="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:mt-0 sm:w-auto"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
