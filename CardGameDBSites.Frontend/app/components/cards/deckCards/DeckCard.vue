<script setup lang="ts">
import type {
  CardDetailApiModel,
  DeckApiModel,
  DeckTypeSettingsApiModel,
  MemberApiModel,
} from "~/api/default";
import CompactDeckCard from "./IconDeckCard.vue";
import SquadDeckCard from "./SquadDeckCard.vue";

const props = defineProps<{
  deck: DeckApiModel;
  member?: MemberApiModel;
  cards?: Record<number, CardDetailApiModel>;
  settings?: DeckTypeSettingsApiModel;
  progress?: number;
}>();
</script>

<template>
  <CompactDeckCard
    :deck="deck"
    :settings="settings!"
    :member="member"
    :cards="cards"
    :progress="progress"
    v-if="settings?.deckDisplay.type === 'iconDeckDisplay'"
  ></CompactDeckCard>
  <SquadDeckCard
    :deck="deck"
    :settings="settings!"
    :member="member"
    :cards="cards"
    v-else-if="settings?.deckDisplay.type === 'squadDeckDisplay'"
  ></SquadDeckCard>
</template>
