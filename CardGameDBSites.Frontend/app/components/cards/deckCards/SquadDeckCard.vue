<script setup lang="ts">
import { PhNotepad } from "@phosphor-icons/vue";
import type { DeckTypeSettingsApiModel, DeckApiModel, MemberApiModel, CardDetailApiModel, IDeckDisplayApiModel } from "~/api/default";
import DeckLike from "~/components/decks/DeckLike.vue";
import { useCards } from "~/composables/useCards";
import { GetCrop } from "~/helpers/CropUrlHelper";
import { ParseToHumanReadableText } from "~/helpers/DateHelper";
import { GetValidCards } from "~/services/requirements/RequirementService";

interface SquadDisplay extends IDeckDisplayApiModel {
  type: "squadDeckDisplay";
  amountOfSquadCards: number;
}

const props = defineProps<{
  deck: DeckApiModel;
  settings: DeckTypeSettingsApiModel;
  member?: MemberApiModel;
  cards?: Record<number, CardDetailApiModel>;
}>();

const deckCards = computed(
  () =>
    (props.deck.cards
      ?.map((c) => props.cards?.[c.cardId!])
      .filter(Boolean) as CardDetailApiModel[]) ?? [],
);
const mainCards = computed(() =>
  GetValidCards(deckCards.value, props.settings.mainCardRequirements ?? []),
);
const createdBy = computed(() => props.member?.displayName ?? "Anonymous");
const groupsOf = computed(() => (props.settings.deckDisplay as SquadDisplay)?.amountOfSquadCards ?? 1);
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
            <DeckLike :deck="deck"></DeckLike>
            <p
              v-if="deck.description"
              class="shrink-0 flex align-center"
              title="Has a description"
            >
              <PhNotepad />
            </p>
          </div>
        </div>
        <div class="text-xs">
          <p>Created by {{ createdBy }}</p>
          <p>{{ ParseToHumanReadableText(deck.createdDate!) }}</p>
        </div>
      </div>
      <div class="grid grid-row-2 gap-2 mt-2">
        <div class="grid gap-2" v-for="groupIndex in Math.ceil(mainCards.length / groupsOf)" :style="{ gridTemplateColumns: `repeat(${groupsOf}, 1fr)` }">
          <img
            v-for="card in mainCards.slice((groupIndex - 1) * groupsOf, groupIndex * groupsOf)"
            :src="card.imageUrl?.url!"
          />
        </div>
      </div>
    </div>
  </NuxtLink>
</template>
