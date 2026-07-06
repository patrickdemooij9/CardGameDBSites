<script setup lang="ts">
import type { CardDetailApiModel, CardVariantTypeApiModel } from "~/api/default";
import { PhBooks } from "@phosphor-icons/vue";
import Button from "~/components/shared/Button.vue";
import ButtonType from "~/components/shared/ButtonType";
import { GetCrop } from "~/helpers/CropUrlHelper";

const props = defineProps<{
  cards: CardDetailApiModel[];
  isLoggedIn: boolean;
  showCardIdentifier: boolean;
  getCardIdentifier: (card: CardDetailApiModel) => string;
  getMainVariants: (card: CardDetailApiModel) => CardVariantTypeApiModel[];
  ownsVariant: (card: CardDetailApiModel, variantTypeId: number | null) => boolean;
  getAmountForSet: (card: CardDetailApiModel) => number;
  collectionVariantBadgeBaseWidth: number;
  collectionVariantBadgeStep: number;
}>();

const emit = defineEmits<{
  (e: "openCollection", card: CardDetailApiModel): void;
}>();
</script>

<template>
  <div class="container px-4 md:px-8 grid grid-cols-2 gap-4 sm:grid-cols-4 md:grid-cols-6">
    <div class="relative" v-for="card in cards" :key="card.baseId">
      <NuxtLink :href="card.urlSegment" class="no-underline">
        <div class="missing-card-image aspect-[2/3]" v-if="!card.imageUrl">
          <h2>{{ card.displayName }}</h2>
          <p>No image yet</p>
        </div>
        <img
          v-else
          :src="GetCrop(card.imageUrl, undefined)"
          loading="lazy"
          class="w-full aspect-[2/3] object-cover"
        />
      </NuxtLink>
      <div class="flex justify-between items-center mt-2">
        <p>
          <span v-if="showCardIdentifier">{{ getCardIdentifier(card) }}</span>
        </p>
        <a
          v-if="card.price"
          :href="card.price.referenceUrl"
          target="_blank"
          class="block bg-green-600 px-2.5 py-1 rounded-md text-white no-underline"
        >
          <p>$ {{ card.price.marketPrice }}</p>
        </a>
      </div>
      <div v-if="isLoggedIn">
        <hr class="mt-2" />
        <div class="flex mt-2 gap-2 items-center justify-between">
          <p
            class="relative w-4 h-6"
            :style="{ width: `${collectionVariantBadgeBaseWidth + getMainVariants(card).length * collectionVariantBadgeStep}px` }"
          >
            <span
              :class="[
                ownsVariant(card, null)
                  ? 'bg-red-600'
                  : 'bg-[#cfcfcf]',
              ]"
              class="absolute top-0 flex justify-center border border-white rounded h-6 w-4 pt-1 z-10"
              title="Base card"
            >
              <span class="bg-white rounded-full w-2 h-2"></span>
            </span>
            <span
              v-for="(variant, index) in getMainVariants(card)"
              :key="variant.id"
              class="absolute top-0 flex justify-center border border-white rounded h-6 w-4 pt-1"
              :title="variant.displayName"
              :style="{
                'background-color': ownsVariant(card, variant.id)
                  ? variant.color!
                  : '#cfcfcf',
                left: `${collectionVariantBadgeStep + index * collectionVariantBadgeStep}px`,
                'z-index': getMainVariants(card).length - index,
              }"
            >
              <span
                v-if="variant.initial"
                class="text-white text-xs font-bold text-center"
              >{{ variant.initial }}</span>
              <span v-else class="bg-white rounded-full w-2 h-2"></span>
            </span>
          </p>
          <span class="ml-2"
            >{{ getAmountForSet(card) }}
            <span class="md:inline hidden">copies</span></span
          >
          <Button
            :button-type="ButtonType.Outline"
            class="flex justify-center"
            @click="emit('openCollection', card)"
          >
            <PhBooks />
          </Button>
        </div>
      </div>
    </div>
  </div>
</template>
