<script setup lang="ts">
import { PhCardsThree, PhCrown, PhImage, PhPiggyBank } from "@phosphor-icons/vue";
import type { DeckActionApiModel, DeckApiModel } from "~/api/default";

defineProps<{
  deck: DeckApiModel;
  action: DeckActionApiModel;
}>();

const icons: {[key: string]: Component} = {
  'crown': PhCrown,
  'cards-three': PhCardsThree,
  'image': PhImage,
  'piggy-bank': PhPiggyBank
};
</script>

<template>
  <div v-if="action.type === 'Forcetable'">
    <a
      class="flex align-center gap-1 no-underline"
      :href="'/umbraco/api/export/ExportForceTable?deckId=' + deck.id"
    >
      <component :is="icons['crown']"></component>
      <p>{{ action.displayName }}</p>
    </a>
  </div>
  <div v-else>
    <a
      class="flex align-center gap-1 no-underline"
      href="/umbraco/api/export/export?deckId=@DeckId&exportId=@exportType.Key"
      target="_blank"
    >
      <component :is="icons[action.icon]"></component>
      <p>{{ action.displayName }}</p>
    </a>
  </div>
</template>
