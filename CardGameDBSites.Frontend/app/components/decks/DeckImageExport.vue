<script setup lang="ts">
import { PhDownloadSimple, PhWarningCircle } from "@phosphor-icons/vue";
import type {
  CardDetailApiModel,
  DeckApiModel,
  DeckTypeSettingsApiModel,
} from "~/api/default";
import PopupBase from "../popups/PopupBase.vue";
import { PopupSize } from "../popups/PopupTypes";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";
import { TIER_LABELS, useDeckImage } from "~/composables/useDeckImage";

const props = defineProps<{
  deck: DeckApiModel;
  cards: CardDetailApiModel[];
  deckTypeSettings: DeckTypeSettingsApiModel;
  heroCardIds: number[];
}>();

const siteSettings = await useSite().getSettings();

const emit = defineEmits<{ close: [] }>();

// This popup only ever mounts on the client, but guard anyway so the component is safe to
// render during SSR if it is ever used somewhere without a v-if.
const deckUrl = import.meta.client
  ? `${window.location.host}${window.location.pathname}`
  : "";

const {
  tiers,
  tier,
  isRendering,
  previewUrl,
  error,
  result,
  render,
  download,
} = useDeckImage({
  deck: props.deck,
  cards: props.cards,
  settings: siteSettings,
  deckTypeSettings: props.deckTypeSettings,
  heroCardIds: props.heroCardIds,
  deckUrl,
});

onMounted(render);
</script>

<template>
  <PopupBase :size="PopupSize.Large" @close="emit('close')">
    <h3 class="text-lg font-bold mb-1">Share as image</h3>
    <p class="text-gray-600 text-sm mb-4">
      Pick how much of the deck to show, then download.
    </p>

    <div v-if="tiers.length > 1" class="flex flex-wrap gap-2 mb-4">
      <button
        v-for="option in tiers"
        :key="option"
        type="button"
        class="px-3 py-1.5 rounded-md text-sm border transition-colors"
        :class="
          tier === option
            ? 'bg-gray-900 text-white border-gray-900'
            : 'bg-white text-gray-700 border-gray-300 hover:border-gray-500'
        "
        :aria-pressed="tier === option"
        :disabled="isRendering"
        @click="tier = option"
      >
        {{ TIER_LABELS[option] }}
      </button>
    </div>

    <div
      class="relative bg-gray-100 rounded-lg overflow-auto flex items-start justify-center"
      style="min-height: 320px; max-height: 60vh"
    >
      <div v-if="error" class="p-8 text-center my-auto">
        <PhWarningCircle class="h-8 w-8 mx-auto text-red-500 mb-2" />
        <p class="font-semibold text-gray-900">Could not build the image</p>
        <p class="text-sm text-gray-600 mt-1">{{ error }}</p>
      </div>

      <img
        v-else-if="previewUrl"
        :src="previewUrl"
        alt="Deck image preview"
        class="max-w-full h-auto my-auto"
      />

      <p v-else class="text-gray-500 text-sm p-8 my-auto">
        Building your image…
      </p>

      <div
        v-if="isRendering && previewUrl"
        class="absolute inset-0 bg-white/60 flex items-center justify-center"
      >
        <p class="text-sm font-semibold text-gray-700">Rendering…</p>
      </div>
    </div>

    <p
      v-if="result && result.missingImages > 0"
      class="text-xs text-gray-500 mt-2"
    >
      {{ result.missingImages }} card
      {{ result.missingImages === 1 ? "image" : "images" }} could not be loaded
      and show as placeholders.
    </p>
    <p v-else-if="result" class="text-xs text-gray-500 mt-2">
      {{ result.width }} x {{ result.height }}
    </p>

    <template #actions>
      <Button
        :button-type="ButtonType.Primary"
        :class="{ 'opacity-50 pointer-events-none': !previewUrl || isRendering }"
        @click="download"
      >
        <div class="flex gap-2 items-center">
          <PhDownloadSimple class="h-5 w-5" />
          <p>Download</p>
        </div>
      </Button>
    </template>
  </PopupBase>
</template>
