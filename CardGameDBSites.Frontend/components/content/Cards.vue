<script setup lang="ts">
import { DeckStatus } from "~/api/default";
import type { CardsElementModel, CardsPropertiesModel } from "~/api/umbraco";
import DeckService from "~/services/DeckService";
import DeckCard from "../cards/deckCards/DeckCard.vue";

const props = defineProps<{
  content: CardsPropertiesModel;
}>();

const decks = await new DeckService().query({
  typeId: props.content.typeId,
  page: 1,
  take: props.content.amount ?? 4,
  status: DeckStatus.PUBLISHED,
});
const gridClass = ref("lg:grid-cols-" + props.content.amountPerRow);
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
  <div
    :class="[gridClass]"
    class="grid grid-cols-1 auto-rows-fr md:grid-cols-2 gap-4 w-full"
  >
    <div v-for="deck in decks.items">
      <DeckCard :deck="deck"></DeckCard>
    </div>
  </div>
</template>
