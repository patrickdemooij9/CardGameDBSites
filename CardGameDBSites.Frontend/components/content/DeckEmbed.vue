<script setup lang="ts">
import type {
  CardDetailApiModel,
  DeckApiModel,
  DeckCardGroupApiModel,
  DeckTypeSettingsApiModel,
} from "~/api/default";
import { GetCardValue } from "~/helpers/CardHelper";
import { GetCrop } from "~/helpers/CropUrlHelper";
import { ParseToHumanReadableText } from "~/helpers/DateHelper";
import DeckService from "~/services/DeckService";
import { GetValidCards } from "~/services/requirements/RequirementService";
import DeckAction from "../decks/DeckAction.vue";

type DeckEmbedContent = {
  deckId?: number | null;
  id?: number | null;
};

const props = defineProps<{
  content: DeckEmbedContent;
}>();

const resolvedDeckId = Number(props.content.deckId ?? props.content.id);
const hasDeckId = Number.isInteger(resolvedDeckId) && resolvedDeckId > 0;

const deck = ref<DeckApiModel | null>(null);
const deckSettings = ref<DeckTypeSettingsApiModel | null>(null);
const cards = ref<CardDetailApiModel[]>([]);
const createdBy = ref("Anonymous");

if (hasDeckId) {
  deck.value = await new DeckService().get(resolvedDeckId);

  if (deck.value?.typeId) {
    deckSettings.value = await useSite().getDeckTypeSettings(deck.value.typeId);
  }

  cards.value = await useCards().loadCardsByIds(
    deck.value?.cards
      ?.sort((a, b) => (a.slotId ?? 0) - (b.slotId ?? 0))
      .map((card) => card.cardId!) ?? [],
  );

  if (deck.value?.createdBy) {
    const creator = (await useMembers().loadMembersByIds([deck.value.createdBy]))[0];
    if (creator?.displayName) {
      createdBy.value = creator.displayName;
    }
  }
}

const mainCards = computed(() =>
  GetValidCards(cards.value, deckSettings.value?.mainCardRequirements ?? []),
);

const missingCardsString = computed(() => {
  const parts: string[] = [];
  deck.value?.cards?.forEach((deckCard) => {
    const card = cards.value.find((c) => c.baseId === deckCard.cardId);
    if (!card) return;
    parts.push(`${deckCard.amount ?? 0} ${card.displayName} [${card.setCode ?? ""}]`);
  });
  return parts.join("||");
});

const totalCards = computed(
  () => deck.value?.cards?.reduce((sum, deckCard) => sum + (deckCard.amount ?? 0), 0) ?? 0,
);

function getDeckCard(cardId: number) {
  return deck.value?.cards?.find((card) => card.cardId === cardId);
}

function getCardsInGroup(group: DeckCardGroupApiModel) {
  const groupCards = GetValidCards(cards.value, group.requirements ?? []);
  if (group.sorting && group.sorting.length > 0) {
    return groupCards.sort((a, b) => {
      const aValue = GetCardValue<string>(a, group.sorting![0]);
      const bValue = GetCardValue<string>(b, group.sorting![0]);
      const aNumber = Number(aValue);
      const bNumber = Number(bValue);

      if (!Number.isFinite(aNumber) || !Number.isFinite(bNumber)) {
        return (aValue as string).localeCompare(bValue as string);
      }
      return aNumber - bNumber;
    });
  }
  return groupCards;
}

function getImagesForCard(card: CardDetailApiModel) {
  const images: string[] = [];
  deckSettings.value?.imageRules?.forEach((rule) => {
    if (GetValidCards([card], rule.requirements ?? []).includes(card)) {
      images.push(rule.imageUrl);
    }
  });
  return images;
}
</script>

<template>
  <div
    v-if="deck && deckSettings"
    class="rounded border border-gray-300 bg-white p-4 md:p-6"
  >
    <div class="mb-4 flex flex-col gap-2 border-b border-gray-200 pb-4 md:flex-row md:items-start md:justify-between">
      <div>
        <h2 class="mb-0">{{ deck.name }}</h2>
        <p class="text-sm">by {{ createdBy }}</p>
        <p class="mt-2 text-sm">
          Format: {{ deckSettings.displayName }}
        </p>
        <p v-if="deck.createdDate" class="text-sm">
          Deck Date: {{ ParseToHumanReadableText(deck.createdDate) }}
        </p>
      </div>
      <p
        v-if="deck.price"
        class="w-fit rounded-md bg-green-600 px-2.5 py-1.5 text-white"
      >
        ${{ deck.price.marketPrice.toFixed(2) }}
      </p>
    </div>

    <div class="flex flex-col gap-6 lg:flex-row">
      <aside class="lg:w-52 lg:shrink-0">
        <div class="mb-4 flex gap-2 lg:flex-col">
          <img
            v-for="mainCard in mainCards.slice(0, 3)"
            :key="mainCard.baseId"
            class="w-1/3 rounded border border-gray-200 lg:w-full"
            :src="GetCrop(mainCard.imageUrl, undefined) ?? '#'"
            :alt="mainCard.displayName"
          />
        </div>
        <div class="flex flex-col gap-2 text-sm">
          <DeckAction
            v-for="action in deckSettings.actions"
            :key="action.id"
            :deck="deck"
            :action="action"
            :missing-cards-string="action.type === 'DeckMissingCardsExport' ? missingCardsString : undefined"
          ></DeckAction>
        </div>
      </aside>

      <div class="min-w-0 flex-1">
        <template v-for="group in deckSettings.groupings" :key="group.header">
          <div v-if="getCardsInGroup(group).length > 0" class="mb-4">
            <h3 class="mb-1 text-lg">{{ group.header }} ({{ getCardsInGroup(group).length }})</h3>
            <div
              class="grid grid-cols-1 gap-1 md:grid-cols-2 md:gap-x-4"
              :style="{
                'grid-template-rows': `repeat(${Math.ceil(
                  getCardsInGroup(group).length / 2,
                )}, 1fr)`,
              }"
            >
              <div
                v-for="card in getCardsInGroup(group)"
                :key="card.baseId"
                class="flex items-center justify-between gap-3 rounded border border-gray-200 px-2 py-1 text-sm"
                v-cursor-image="card.imageUrl?.url"
              >
                <div class="flex min-w-0 items-center gap-2">
                  <span class="shrink-0 font-semibold">{{ getDeckCard(card.baseId!)?.amount }}x</span>
                  <div
                    v-if="deckSettings.costImageUrl"
                    class="shrink-0"
                  >
                    <div v-if="deckSettings.renderCostOnImage">
                      <div
                        class="flex h-5 w-[18px] justify-center bg-contain bg-no-repeat font-bold text-white"
                        :style="{
                          'background-image': `url(${deckSettings.costImageUrl})`,
                        }"
                      >
                        <span>{{ GetCardValue(card, "Cost") }}</span>
                      </div>
                    </div>
                    <div
                      class="flex h-5 w-[18px] items-center font-bold text-black"
                      v-else
                    >
                      <span>{{ GetCardValue(card, "Shard Cost") }}</span>
                      <img :src="deckSettings.costImageUrl" />
                    </div>
                  </div>
                  <div class="flex shrink-0 gap-1">
                    <img
                      v-for="image in getImagesForCard(card)"
                      :key="image"
                      :src="image"
                      class="h-5 w-5"
                    />
                  </div>
                  <span class="truncate text-blue-700">{{ card.displayName }}</span>
                </div>
                <span v-if="card.price" class="shrink-0 text-right">${{ card.price.marketPrice.toFixed(2) }}</span>
              </div>
            </div>
          </div>
        </template>
        <p class="mt-2 font-semibold">{{ totalCards }} Cards Total</p>
      </div>
    </div>
    <div
      id="cursor-image"
      class="pointer-events-none absolute h-72 w-48 bg-contain bg-no-repeat"
      style="display: none"
    ></div>
  </div>
</template>
