<script setup lang="ts">
import type { DeckTypeSettingsApiModel, DeckApiModel } from "~/api/default";
import DeckLike from "~/components/decks/DeckLike.vue";
import { ParseToHumanReadableText } from "~/helpers/DateHelper";
import { GetValidCards } from "~/services/requirements/RequirementService";
import { useCardsStore } from "~/stores/CardStore";
import { useMemberStore } from "~/stores/MemberStore";

const props = defineProps<{
  deck: DeckApiModel;
  settings: DeckTypeSettingsApiModel;
}>();

const cards = await useCardsStore().loadCards(
  props.deck.cards?.map((card) => card.cardId!) ?? []
);
const mainCard = GetValidCards(
  cards,
  props.settings.mainCardRequirements ?? []
)[0];

let createdBy = "Anonymous";
if (props.deck.createdBy){
  createdBy = (await useMemberStore().loadMembers([props.deck.createdBy]))[0].displayName;
}
</script>

<template>
  <div class="overflow-hidden rounded mb-3 shadow-lg">
    <NuxtLink
      class="block h-40 relative text-white rounded no-underline"
      :to="settings.overviewUrl + deck.id"
    >
      <div
        class="overflow-hidden h-full before:content-[''] before:bg-gradient-to-b before:from-black before:to-white before:absolute before:w-full before:h-full"
      >
        <img class="opacity-50 w-full" :src="mainCard.imageUrl + '?width=380&height=210'" />
      </div>
      <div class="absolute p-2 top-0 w-full">
        <div class="flex align-center justify-between gap-4">
          <p class="text-xl font-bold pb-2 break-words">{{ deck.name }}</p>
          <div class="self-start p-1">
            <DeckLike :deck="deck"></DeckLike>
          </div>
        </div>
        <p class="text-sm italic">{{ mainCard.displayName }}</p>
      </div>
    </NuxtLink>
    <!--@if (colors.Count > 0 && squadSettings?.ShowDeckColors is true)
    {
        var totalItems = colors.Sum(it => it.Value);
        <div class="flex h-1">
            @foreach (var color in colors)
            {
                <div style="background-color: #@color.Key.ToHex(); width:@(((double)color.Value / totalItems) * 100)%"></div>
            }
        </div>
    }-->
    <div class="">
      <div class="p-2 flex justify-between">
        <div class="author">
          <p>
            By <a class="no-underline" href="#">{{ createdBy }}</a>
          </p>
        </div>
        <div class="date">Created {{ ParseToHumanReadableText(deck.createdDate!) }}</div>
      </div>
    </div>
  </div>
</template>
