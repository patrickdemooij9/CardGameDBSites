<script setup lang="ts">
import type { DeckDetailContentModel } from "~/api/umbraco";
import DeckService from "~/services/DeckService";
import DeckLike from "../decks/DeckLike.vue";
import SiteService from "~/services/SiteService";
import type { CardDetailApiModel, DeckCardGroupApiModel } from "~/api/default";
import { GetValidCards } from "~/services/requirements/RequirementService";
import DeckAction from "../decks/DeckAction.vue";

defineProps<{
  content: DeckDetailContentModel;
}>();

const route = useRoute();
let slug = route.params.slug as string[];
const deckId = Number.parseInt(slug[slug.length - 1]);

const deck = await new DeckService().get(deckId);
const deckSettings = await new SiteService().getDeckTypeSettings(deck.typeId!);
const cards = await useCardsStore().loadCards(
  deck.cards?.map((card) => card.cardId!) ?? []
);
const mainCards = GetValidCards(
  cards,
  deckSettings!.mainCardRequirements ?? []
);

const showPrices = false;
const isLoggedIn = false;

function getDeckCard(cardId: number) {
  return deck.cards?.find((card) => card.cardId === cardId);
}
function getCardsInGroup(group: DeckCardGroupApiModel) {
  return GetValidCards(cards, group.requirements ?? []);
}
function getImagesForCard(card: CardDetailApiModel) {
  const images: string[] = [];
  deckSettings?.imageRules?.forEach((rule) => {
    if (GetValidCards([card], rule.requirements ?? []).includes(card)) {
      images.push(rule.imageUrl);
    }
  });
  return images;
}
</script>

<template>
  <div class="container px-4 py-4 md:px-8">
    <div class="p-4 bg-white rounded">
      <div class="flex">
        <div class="grow">
          <h1 class="text-lg pb-0">{{ deck.name }}</h1>
          <p class="text-xs">By {{ deck.createdBy ?? "Anonymous" }}</p>
          <div class="flex align-center gap-2 mt-2">
            <p class="border rounded py-1 px-2">
              {{ deckSettings?.displayName }}
            </p>
            <div id="deck-like-@deck.Id">
              <DeckLike :deck="deck"></DeckLike>
            </div>
          </div>
        </div>

        <div class="shrink-0" v-if="showPrices">
          <p
            class="w-fit bg-green-600 px-2.5 py-1.5 rounded-md text-white cursor-pointer"
            onclick="document.querySelector('#buyCardsForm').submit()"
          >
            $@deckCost.ToString("0.00")
          </p>
        </div>
      </div>

      <div class="flex flex-wrap justify-center gap-4 mt-8">
        <div
          v-for="mainCard in mainCards"
          :class="{ 'w-2/5': mainCards.length > 1 }"
          class="md:w-max"
        >
          <img class="w-48" :src="mainCard.imageUrl ?? '#'" />
          <p class="text-center">
            <small>{{ mainCard.displayName }}</small>
          </p>
        </div>
      </div>

      <div class="flex flex-col md:flex-row gap-8 mt-8">
        <div class="md:w-2/3 shrink-0">
          <div class="flex align-center justify-between gap-4">
            <h2 class="text-lg">Decklist</h2>
            <div v-if="isLoggedIn">
              <input
                type="checkbox"
                id="compare-collection"
                data-toggle=".js-collection-info"
              />
              <label for="compare-collection">Compare with collection</label>
            </div>
          </div>
          <hr class="my-2" />
          <div class="flex md:flex-row flex-col gap-4 text-xs">
            <DeckAction v-for="action in deckSettings?.actions" :deck="deck" :action="action"></DeckAction>
          </div>
          <template v-for="group in deckSettings?.groupings">
            <div v-if="getCardsInGroup(group).length > 0">
              <h3 class="text-sm mt-4 mb-2">{{ group.header }}</h3>
              <div
                class="md:grid grid-flow-col grid-cols-2 gap-2"
                :style="{
                  'grid-template-rows': `repeat(${Math.ceil(
                    getCardsInGroup(group).length / 2
                  )}, 1fr)`,
                }"
              >
                <div
                  v-for="card in getCardsInGroup(group)"
                  class="flex md:flex-row flex-col gap-2 md:align-center md:rounded-full rounded-md px-2 py-1 border cursor-source"
                  data-cursor-image="@card.Image?.GetCropUrl(width: 400)"
                >
                  <div class="flex gap-2">
                    <!--@if (_isLoggedIn)
                {
                    var collectionAmount = GetAmountFromCollection(card.BaseId);
                    <span class="flex gap-0.5 js-collection-info font-bold" style="display:none;">
                        <span class="@(collectionAmount >= deckCard.Amount ? "text-green-600" : "text-red-600")">@collectionAmount</span>
                        <span>/</span>
                        <span>@deckCard.Amount</span>
                    </span>
                  }-->
                    <span class="js-collection-info"
                      >{{ getDeckCard(card.baseId!)?.amount }} x</span
                    >
                    <div
                      class="flex justify-center bg-contain bg-no-repeat h-5 w-[18px] text-white font-bold"
                      style="background-image: url('/images/icon-cost.png')"
                    >
                      <span>3</span>
                    </div>
                    <div class="flex gap-2">
                      <img
                        v-for="image in getImagesForCard(card)"
                        :src="image"
                        class="w-5 h-5"
                      />
                    </div>
                  </div>
                  <span>{{ card.displayName }}</span>
                </div>
              </div>
            </div>
          </template>
          <!--TODO: Cards-->
        </div>
        <div v-if="deck.description">
          <h2 class="text-lg pb-2">Description</h2>
          <div class="content">
            {{ deck.description }}
          </div>
        </div>
      </div>

      <div class="pt-4">
        <!--TODO: Comments-->
      </div>
    </div>
  </div>
</template>
