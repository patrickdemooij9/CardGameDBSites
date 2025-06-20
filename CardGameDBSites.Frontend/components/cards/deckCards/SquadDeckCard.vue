<script setup lang="ts">
import type { DeckTypeSettingsApiModel, DeckApiModel } from "~/api/default";
import { useCardsStore } from "~/stores/CardStore";

const props = defineProps<{
  deck: DeckApiModel;
  settings: DeckTypeSettingsApiModel
}>();

const cards = await useCardsStore().loadCards(props.deck.cards?.map((card) => card.cardId!) ?? []);
</script>

<template>
  <NuxtLink
    :to="settings.overviewUrl + deck.id"
    class="no-underline"
  >
    <div
      class="flex flex-col justify-between border border-main-color rounded-md px-4 py-3 bg-white h-full"
    >
      <div class="border-b border-gray-300">
        <div class="flex justify-between align-center pb-1">
          <h5>{{ deck.name }}</h5>
          <div class="flex gap-2 shrink-0">
            <div id="deck-like-@Model.Id">
              <span>Like here</span>
            </div>
            <p
              v-if="deck.description"
              class="shrink-0 flex align-center"
              title="Has a description"
            >
              <i class="ph ph-notepad"></i>
            </p>
          </div>
        </div>
        <div class="text-xs">
          <p>Created by {{ deck.createdBy }}</p>
          <p>{{ deck.createdDate }}</p>
        </div>
      </div>
      <div class="grid grid-row-2 gap-2 mt-2">
        <p>Main cards here...</p>
        <div class="grid grid-cols-@squadSettings.AmountOfSquadCards gap-2">
        </div>
        <p v-for="card in cards">
            {{ card.displayName }}
        </p>
      </div>
    </div>
  </NuxtLink>
</template>
