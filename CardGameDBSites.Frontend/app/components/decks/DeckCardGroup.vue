<script setup lang="ts">
import type { CardDetailApiModel, DeckCardGroupApiModel, DeckTypeSettingsApiModel } from "~/api/default";
import { GetCardValue } from "~/helpers/CardHelper";
import { GetCrop } from "~/helpers/CropUrlHelper";
import { GetValidCards } from "~/services/requirements/RequirementService";

const props = defineProps<{
  group: DeckCardGroupApiModel;
  cards: CardDetailApiModel[];
  settings: DeckTypeSettingsApiModel;
  deckCards?:
    | {
        cardId?: number | null;
        amount?: number | null;
        groupId?: number | null;
      }[]
    | null;
  deckDisplayMode: string;
  collectionMode: boolean;
  costImageUrl?: string | null;
  renderCostOnImage?: boolean;
}>();

const emit = defineEmits<{
  cardClick: [card: CardDetailApiModel];
}>();

const groupCards = computed(() => {
  let baseCards = props.cards;
  // When the group is bound to a specific deck group id, only show the cards
  // that belong to that group; otherwise fall back to requirement filtering.
  const targetGroupId = props.group.groupId ?? 0;
  if (targetGroupId) {
    const cardIdsInGroup = new Set(
      (props.deckCards ?? [])
        .filter((deckCard) => (deckCard.groupId ?? 0) === targetGroupId)
        .map((deckCard) => deckCard.cardId),
    );
    baseCards = baseCards.filter(
      (card) => card.baseId != null && cardIdsInGroup.has(card.baseId),
    );
  }
  const groupCards = GetValidCards(baseCards, props.group.requirements ?? []);
  if (props.group.sorting && props.group.sorting.length > 0) {
    return groupCards.sort((a, b) => {
      const aValue = GetCardValue<string>(a, props.group.sorting![0]!);
      const bValue = GetCardValue<string>(b, props.group.sorting![0]!);

      if (Number.isNaN(aValue) || Number.isNaN(bValue)) {
        return (aValue as string).localeCompare(bValue as string);
      }
      return Number.parseInt(aValue!) - Number.parseInt(bValue!);
    });
  }
  return groupCards;
});

const deckCardAmountsById = computed(() => {
  const amounts = new Map<number, number>();
  (props.deckCards ?? []).forEach((deckCard) => {
    if (deckCard.cardId == null) {
      return;
    }
    amounts.set(deckCard.cardId, deckCard.amount ?? 0);
  });
  return amounts;
});

function getDeckCardAmount(cardId: number) {
  return deckCardAmountsById.value.get(cardId) ?? 0;
}

function getCollectionCount(cardId: number) {
  return (
    useCollection()
      .getCards(cardId)
      ?.reduce((sum, card) => sum + (card.amount ?? 0), 0) ?? 0
  );
}

function getImagesForCard(card: CardDetailApiModel) {
  const images: string[] = [];
  props.settings?.imageRules?.forEach((rule) => {
    if (GetValidCards([card], rule.requirements ?? []).includes(card)) {
      images.push(rule.imageUrl);
    }
  });
  return images;
}

function handleCardClick(card: CardDetailApiModel) {
  emit("cardClick", card);
}
</script>

<template>
  <div v-if="groupCards.length > 0">
      <h3 v-if="props.group.header" class="text-sm mt-4 mb-2">{{ props.group.header }}</h3>
      <div
        v-if="deckDisplayMode === 'Text'"
        class="md:grid grid-flow-col grid-cols-2 gap-2"
        :style="{
          'grid-template-rows': `repeat(${Math.ceil(groupCards.length / 2)}, 1fr)`,
        }"
      >
        <div
          v-for="card in groupCards"
          :key="card.baseId"
          class="flex md:flex-row flex-col gap-2 md:align-center md:rounded-full rounded-md px-2 py-1 border cursor-source cursor-pointer"
          v-cursor-image="card.imageUrl?.url"
          @click="handleCardClick(card)"
        >
          <div class="flex gap-2">
            <span
              v-if="collectionMode"
              class="flex gap-0.5 js-collection-info font-bold"
            >
              <span
                :class="[
                  getCollectionCount(card.baseId!) >= getDeckCardAmount(card.baseId!)
                    ? 'text-green-600'
                    : 'text-red-600',
                ]"
              >{{ getCollectionCount(card.baseId!) }}</span>
              <span>/</span>
              <span>{{ getDeckCardAmount(card.baseId!) }}</span>
            </span>
            <span v-else class="js-collection-info">{{ getDeckCardAmount(card.baseId!) }} x</span>
            <div v-if="costImageUrl">
              <div v-if="renderCostOnImage">
                <div
                  class="flex justify-center bg-contain bg-no-repeat h-5 w-[18px] text-white font-bold"
                  :style="{
                    'background-image': `url(${costImageUrl})`,
                  }"
                >
                  <span>{{ GetCardValue(card, "Cost") }}</span>
                </div>
              </div>
              <div v-else class="flex items-center h-5 w-[18px] text-black font-bold">
                <span>{{ GetCardValue(card, "Shard Cost") }}</span>
                <img :src="costImageUrl" />
              </div>
            </div>
            <div class="flex gap-2">
              <img
                v-for="image in getImagesForCard(card)"
                :key="image"
                :src="image"
                class="w-5 h-5"
              />
            </div>
          </div>
          <span>{{ card.displayName }}</span>
        </div>
      </div>
      <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-2">
        <div
          v-for="card in groupCards"
          :key="card.baseId"
          class="flex flex-col items-center text-center gap-1 cursor-pointer"
          v-cursor-image="card.imageUrl?.url"
          @click="handleCardClick(card)"
        >
          <img
            class="rounded"
            :src="GetCrop(card.imageUrl, undefined) ?? '#'"
            :alt="card.displayName ?? ''"
          />
          <span
            v-if="collectionMode"
            class="flex gap-0.5 text-xs font-semibold js-collection-info"
          >
            <span
              :class="[
                getCollectionCount(card.baseId!) >= getDeckCardAmount(card.baseId!)
                  ? 'text-green-600'
                  : 'text-red-600',
              ]"
            >{{ getCollectionCount(card.baseId!) }}</span>
            <span>/</span>
            <span>{{ getDeckCardAmount(card.baseId!) }}</span>
          </span>
          <span v-else class="text-xs font-semibold js-collection-info">
            {{ getDeckCardAmount(card.baseId!) }} x
          </span>
        </div>
      </div>
    </div>
</template>
