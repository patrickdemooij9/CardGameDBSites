<script setup lang="ts">
import { DeckStatus } from "~/api/default";
import type { CardsPropertiesModel } from "~/api/umbraco";
import DeckService from "~/services/DeckService";
import DeckCardCollection from "../cards/deckCards/DeckCardCollection.vue";

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
  <DeckCardCollection :decks="decks.items ?? []" :decks-per-row="props.content.amountPerRow ?? 4"></DeckCardCollection>
</template>
