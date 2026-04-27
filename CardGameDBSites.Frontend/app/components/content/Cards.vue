<script setup lang="ts">
import { DeckStatus } from "~/api/default";
import type { CardsPropertiesModel } from "~/api/umbraco";
import DeckCardCollection from "../cards/deckCards/DeckCardCollection.vue";

const props = defineProps<{
  content: CardsPropertiesModel;
}>();

const decks = useDecks();
const request = {
  typeId: props.content.typeId,
  page: 1,
  take: props.content.amount ?? 4,
  status: DeckStatus.PUBLISHED,
  orderBy: props.content.type === "MostPopular" ? "popular" : undefined
};

const { data } = await useAsyncData(decks.toUniqueKey(request), () => decks.query(request));
</script>

<template>
  <div
    :class="{ 'text-white': content.hasLightTitle }"
    class="flex mb-4 z-10 w-full"
  >
    <h2>
      {{ content.title }}
    </h2>
  </div>
  <DeckCardCollection :decks="data?.items ?? []" :decks-per-row="props.content.amountPerRow ?? 4"></DeckCardCollection>
</template>
