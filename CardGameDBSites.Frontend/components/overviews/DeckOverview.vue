<script setup lang="ts">
import DeckService from "~/services/DeckService";
import type OverviewRefreshModel from "./OverviewRefreshModel";
import type { PagedResultDeckApiModel } from "~/api/default";
import Overview from "./Overview.vue";
import DeckCard from "../cards/deckCards/DeckCard.vue";
import BaseDeckOverview from "./BaseDeckOverview.vue";

defineProps<{
  decksPerRow: number;
}>();
</script>

<template>
  <BaseDeckOverview
    :decks-per-row="decksPerRow">
    <template #default="{ decks }">
      <DeckCard v-for="deck in decks?.items" :deck="deck"></DeckCard>
    </template>
  </BaseDeckOverview>
</template>
