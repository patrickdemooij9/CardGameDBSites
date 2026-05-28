<script setup lang="ts">
import { useCardLists } from "~/composables/useCardLists";
import { useCards } from "~/composables/useCards";
import type { CardListDetailModel } from "~/models/CardListModel";
import type { CardDetailApiModel } from "~/api/default";
import { GetCrop } from "~/helpers/CropUrlHelper";

const route = useRoute();
const cardLists = useCardLists();
const cardsService = useCards();
const isLoading = ref(true);
const listDetail = ref<CardListDetailModel | null>(null);
const cardDetails = ref<Record<number, CardDetailApiModel>>({});
const variantTypes = ref<{ id: number; displayName: string }[]>([]);
const notFound = ref(false);

const listId = computed(() => {
  const id = route.params.slug;
  if (Array.isArray(id)) return parseInt(id[id.length - 2] ?? "0");
  return parseInt(id as string);
});

onMounted(async () => {
  try {
    listDetail.value = await cardLists.getPublicList(listId.value);
    variantTypes.value = await cardsService.loadVariantTypes();
    await loadCardDetails();
  } catch {
    notFound.value = true;
  }
  isLoading.value = false;
});

async function loadCardDetails() {
  if (!listDetail.value) return;
  const cardIds = [...new Set(listDetail.value.items.map((i) => i.cardId))];
  if (cardIds.length === 0) return;

  const cards = await cardsService.loadCardsByIds(cardIds);
  cards.forEach((card: CardDetailApiModel) => {
    cardDetails.value[card.baseId!] = card;
  });
}

function getVariantName(variantId: number | null | undefined): string {
  if (!variantId) return "Normal";
  const vt = variantTypes.value.find((v) => v.id === variantId);
  return vt?.displayName ?? "Unknown";
}

function getCardForItem(cardId: number): CardDetailApiModel | undefined {
  return cardDetails.value[cardId];
}
</script>

<template>
  <div v-if="isLoading" class="flex justify-center items-center py-12">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>
  <div v-else-if="notFound" class="container px-4 pt-8 md:px-8 mb-6 text-center">
    <h1>List Not Found</h1>
    <p class="text-gray-500 mt-2">This list does not exist or is not publicly available.</p>
  </div>
  <div v-else-if="listDetail" class="container px-4 pt-8 md:px-8 mb-6">
    <!-- Header -->
    <div class="mb-6">
      <h1>{{ listDetail.name }}</h1>
      <p v-if="listDetail.description" class="text-gray-600 mt-1">{{ listDetail.description }}</p>
      <p class="text-sm text-gray-500 mt-1">
        By {{ listDetail.ownerName ?? "Unknown" }} · {{ listDetail.items.length }} cards
      </p>
    </div>

    <!-- Cards grid -->
    <div v-if="listDetail.items.length === 0" class="text-center py-8">
      <p class="text-gray-500">This list is empty.</p>
    </div>
    <div v-else class="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6">
      <div
        v-for="item in listDetail.items"
        :key="item.id"
        class="relative border rounded bg-white overflow-hidden"
      >
        <NuxtLink
          v-if="getCardForItem(item.cardId)"
          :href="getCardForItem(item.cardId)?.urlSegment"
          class="no-underline"
        >
          <div class="missing-card-image" v-if="!getCardForItem(item.cardId)?.imageUrl">
            <h2 class="text-sm">{{ getCardForItem(item.cardId)?.displayName }}</h2>
          </div>
          <img
            v-else
            :src="GetCrop(getCardForItem(item.cardId)!.imageUrl!, undefined)"
            loading="lazy"
            class="w-full"
          />
        </NuxtLink>
        <div class="p-2">
          <p class="text-xs font-medium truncate">
            {{ getCardForItem(item.cardId)?.displayName ?? `Card #${item.cardId}` }}
          </p>
          <span class="text-xs text-gray-500">
            {{ getVariantName(item.variantId) }} × {{ item.amount }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>
