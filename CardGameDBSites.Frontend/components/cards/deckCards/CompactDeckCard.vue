<script setup lang="ts">
import { PhNotepad } from "@phosphor-icons/vue";
import type { DeckApiModel, DeckTypeSettingsApiModel } from "~/api/default";
import { GetCropUrl } from "~/helpers/CropUrlHelper";
import { ParseToHumanReadableText } from "~/helpers/DateHelper";
import { GetValidCards } from "~/services/requirements/RequirementService";

const props = defineProps<{
  deck: DeckApiModel;
  settings: DeckTypeSettingsApiModel;
}>();

const cards = await useCardsStore().loadCards(
  props.deck.cards?.map((card) => card.cardId!) ?? []
);
const mainCards = GetValidCards(
  cards,
  props.settings.mainCardRequirements ?? []
);

let createdBy = "Anonymous";
if (props.deck.createdBy){
  createdBy = (await useMemberStore().loadMembers([props.deck.createdBy]))[0].displayName;
}
</script>

<template>
  <div class="flex flex-col">
    <NuxtLink
      :to="settings.overviewUrl + deck.id"
      class="block px-2 py-2 border rounded bg-white no-underline hover:border-main-color"
    >
      <div class="flex items-center justify-between">
        <p class="font-bold">{{ deck.name }}</p>
        <div class="flex gap-2 shrink-0">
          <div class="self-start p-1">
            <DeckLike :deck="deck"></DeckLike>
          </div>
          <p v-if="deck.description" class="shrink-0 flex align-center" title="Has a description">
            <PhNotepad/>
          </p>
        </div>
      </div>
      <p class="text-xs"><i>{{ settings.displayName }}</i></p>
      <div class="flex items-center justify-between mt-4">
        <div class="flex gap-2">
            <img v-for="card in mainCards" class="w-12 rounded-full"
          :src="card.imageUrl!" />
        </div>
        <div class="text-right">
            <!--
          @if (memberInfo?.IsLoggedIn is true) {
          <div>
            Collection:
            @_collectionService.CalculateDeckCollection(Model).ToString("0.00")%
          </div>
          } @if (_settingsService.GetSiteSettings().AllowPricing) {
          <div class="flex justify-end mt-1">
            <p class="w-fit bg-green-600 px-2.5 py-1.5 rounded-md text-white">
              $@_cardPriceService.GetPriceByDeck(Model).ToString("0.00")
            </p>
          </div>
          }-->
        </div>
      </div>
    </NuxtLink>
    <div class="flex justify-between px-2 text-xs mt-1">
      <p>By <a class="no-underline" href="#"> {{ createdBy }}</a></p>
      <p>Created {{ ParseToHumanReadableText(deck.createdDate!) }}</p>
    </div>
  </div>
</template>
