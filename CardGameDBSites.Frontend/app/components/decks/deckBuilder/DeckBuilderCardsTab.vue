<script setup lang="ts">
import BaseCardOverview from "~/components/overviews/BaseCardOverview.vue";
import DeckBuilderTab from "./DeckBuilderTab";
import {
  CardSearchCollectionMode,
  RestrictionType,
  type CardDetailApiModel,
  type RequirementApiModel,
} from "~/api/default";
import type CreateDeckSlot from "./models/CreateDeckSlot";
import { GetFilters } from "~/services/requirements/RequirementService";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import Button from "~/components/shared/Button.vue";
import ButtonType from "~/components/shared/ButtonType";
import { GetMarkdown } from "~/services/MarkdownService";
import type { CreateDeckSelectedArea } from "./models/CreateDeckSelectedArea";
import { GetCrop } from "~/helpers/CropUrlHelper";
import {
  OverviewFilterType,
  type OverviewFilterModel,
} from "~/components/overviews/OverviewFilterModel";
import DeckCostCurve from "./DeckCostCurve.vue";

const emit = defineEmits<{
  (e: "update:currentTab", value: DeckBuilderTab): void;
  (e: "selectCard", value: CardDetailApiModel): void;
}>();

const props = defineProps<{
  currentTab: DeckBuilderTab;
  currentArea?: CreateDeckSelectedArea;
  filters: OverviewFilterModel[];
  deck: CreateDeckModel;
  preselectFirstSlot: boolean;
  ignorePassiveFilters: boolean;
  collectionOnlyMode: boolean;
}>();

const description = ref("");
const markdownPreview = ref(false);
const markdownPreviewText = ref("");

const collectionService = useCollection();

function getUserFilters() {
  if (!props.currentArea) {
    return [];
  }

  const allRequirements = [
    ...props.currentArea.group.requirements,
    ...props.currentArea.slot.requirements,
  ];

  return allRequirements
    .filter((req) => req.restrictionType === RestrictionType.FILTER)
    .map<OverviewFilterModel>((req) => {
      return {
        Alias: req.config?.ability ?? req.alias ?? "Filter",
        DisplayName: req.filterDisplayName ?? req.alias ?? "Filter",
        Type: OverviewFilterType.CHECKBOX,
        Items: [],
        AutoFillValues: false,
        DefaultEnabled: true,
        ToFiltersHandler: (_) => {
          return GetFilters(
            props.currentArea!.group.getCards(),
            [req],
            props.ignorePassiveFilters,
          );
        },
      };
    }).concat(props.filters);
}

function getInternalFilters() {
  if (!props.currentArea) {
    return [];
  }
  const cards = props.currentArea.group.getCards();
  return [
    ...GetFilters(
      cards,
      props.currentArea.group.requirements.filter(
        (req) => req.restrictionType !== RestrictionType.FILTER,
      ),
      props.ignorePassiveFilters,
    ),
    ...GetFilters(
      cards,
      props.currentArea.slot.requirements.filter(
        (req) => req.restrictionType !== RestrictionType.FILTER,
      ),
      props.ignorePassiveFilters,
    ),
  ];
}

function getSlotsForCard(card: CardDetailApiModel) {
  return props.deck
    .getSlotsForCard(card)
    .filter(
      (slot) =>
        props.currentArea === undefined || props.currentArea.slot === slot,
    );
}

function canAddCardToSlot(slot: CreateDeckSlot, card: CardDetailApiModel): boolean {
  const existingInOtherSlots = props.deck.getCardAmountInOtherSlots(slot, card);
  return slot.canAddCard(card, existingInOtherSlots);
}

function addToSquad(slot: CreateDeckSlot, character: CardDetailApiModel) {
  slot.addCard(character);

  if (props.collectionOnlyMode){
    collectionService.loadCards([character.baseId!]);
  }
}

function removeFromSquad(slot: CreateDeckSlot, card: CardDetailApiModel) {
  slot.removeCard(card);
}

async function toggleMarkdownPreview() {
  if (markdownPreview) {
    markdownPreviewText.value = await GetMarkdown(description.value);
  }

  markdownPreview.value = !markdownPreview.value;
}
</script>

<template>
  <div
    class="md:block px-4 md:px-8 py-4 grow bg-gray-100"
    :class="currentTab === DeckBuilderTab.Deck ? 'hidden' : ''"
  >
    <div class="gap-2 hidden md:flex">
      <button
        class="px-4 py-1.5 rounded-full text-sm font-medium transition-colors border"
        type="button"
        :class="
          currentTab === DeckBuilderTab.Cards ||
          currentTab === DeckBuilderTab.Deck
            ? 'bg-main-color text-white border-main-color'
            : 'bg-white text-gray-600 border-gray-300 hover:border-main-color hover:text-gray-900'
        "
        v-on:click="emit('update:currentTab', DeckBuilderTab.Cards)"
      >
        Cards
      </button>
      <button
        class="px-4 py-1.5 rounded-full text-sm font-medium transition-colors border"
        type="button"
        :class="
          currentTab === DeckBuilderTab.Details
            ? 'bg-main-color text-white border-main-color'
            : 'bg-white text-gray-600 border-gray-300 hover:border-main-color hover:text-gray-900'
        "
        v-on:click="emit('update:currentTab', DeckBuilderTab.Details)"
      >
        Details
      </button>
    </div>
    <div
      class="mt-4"
      :class="{
        'md:block':
          currentTab === DeckBuilderTab.Cards ||
          currentTab === DeckBuilderTab.Deck,
        hidden: currentTab !== DeckBuilderTab.Cards,
      }"
    >
      <BaseCardOverview
        :filters="getUserFilters()"
        :internal-filters="getInternalFilters()"
        :white-background="false"
        :enable-query-string-sync="false"
        :collection-mode="collectionOnlyMode ? CardSearchCollectionMode.IN_COLLECTION : CardSearchCollectionMode.IGNORE"
        :hide-reprinted-cards="true"
        :legal-for-deck-type-id="deck.typeId"
        v-slot="{cards}"
      >
        <div class="grid grid-cols-2 gap-4 sm:grid-cols-4 md:grid-cols-5">
          <div v-for="character in cards.items" :key="character.baseId">
            <a
              href="#"
              class="no-underline block relative"
              v-on:click.prevent="emit('selectCard', character)"
            >
              <div class="image wiggle hover:shadow-[0px_0px_8px_3px_#b0b0b0]">
                <img
                  v-if="character.imageUrl"
                  height="300"
                  width="200"
                  :src="GetCrop(character.imageUrl, undefined)"
                  loading="lazy"
                />
                <div v-if="!character.imageUrl" class="missing-card-image">
                  <h2 v-text="character.displayName"></h2>
                  <p>No image yet</p>
                </div>
              </div>
              <div class="overlay">
                <div class="info-bar">
                  <div class="info-icon">
                    <i class="ph ph-info"></i>
                  </div>
                </div>
                <div
                  class="flex flex-col align-middle gap-2"
                  v-if="!preselectFirstSlot"
                >
                  <div
                    v-for="location in getSlotsForCard(character)"
                    class="flex justify-between"
                    :class="{
                      'w-full':
                        (location.getMaxAmount() ?? 0) > 1 ||
                        location.minCards > 1,
                    }"
                  >
                    <div>
                      <div
                        v-if="
                          location.isInsideSlot(character) &&
                          location.isCardAllowedToRemove(character)
                        "
                      >
                        <button
                          v-if="!location.numberMode"
                          v-on:click.prevent.stop="
                            removeFromSquad(location, character)
                          "
                          type="button"
                          class="pointer rounded-lg bg-white text-black border-none w-fit px-4 py-3"
                        >
                          <span>Remove</span>
                        </button>
                        <button
                          v-if="location.numberMode"
                          v-on:click.prevent.stop="
                            removeFromSquad(location, character)
                          "
                          type="button"
                          class="pointer rounded-lg bg-white text-black border-none w-fit px-4 py-3"
                        >
                          <span>-</span>
                        </button>
                      </div>
                    </div>
                    <div v-if="canAddCardToSlot(location, character)">
                      <div>
                        <button
                          v-if="!location.numberMode"
                          v-on:click.prevent.stop="
                            addToSquad(location, character)
                          "
                          type="button"
                          class="pointer rounded-lg bg-white text-black border-none w-fit px-4 py-3"
                        >
                          <span>Add to {{ location.label }}</span>
                        </button>
                        <button
                          v-if="location.numberMode"
                          v-on:click.prevent.stop="
                            addToSquad(location, character)
                          "
                          type="button"
                          class="pointer rounded-lg bg-white text-black border-none w-fit px-4 py-3"
                        >
                          <span>+</span>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </a>
            <div
              class="flex flex-col align-middle gap-2 mt-2"
              v-if="preselectFirstSlot"
            >
              <div
                v-for="location in getSlotsForCard(character)"
                class="flex justify-between w-full"
              >
                <div>
                  <button
                    v-on:click.prevent.stop="
                      removeFromSquad(location, character)
                    "
                    type="button"
                    v-if="!location.disableRemoval"
                    :disabled="!location.isCardAllowedToRemove(character)"
                    class="pointer rounded-lg bg-white text-black border-none w-fit px-4 py-2 disabled:cursor-not-allowed disabled:bg-gray-300 hover:bg-gray-200"
                  >
                    <span v-if="!location.numberMode">Remove</span>
                    <span v-else-if="location.numberMode">-</span>
                  </button>
                </div>
                <div class="flex items-center" v-if="location.numberMode">
                  <span
                    >{{ location.getCardAmount(character) }}/{{
                      location.getCardMaxAmount(character)
                    }}</span
                  >
                </div>
                <div>
                  <Button
                    v-on:click.prevent.stop="addToSquad(location, character)"
                    type="button"
                    :disabled="!canAddCardToSlot(location, character)"
                    class="rounded-lg"
                  >
                    <span v-if="!location.numberMode">Add</span>
                    <span v-else-if="location.numberMode">+</span>
                  </Button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </BaseCardOverview>
    </div>
    <div :class="currentTab === DeckBuilderTab.Details ? '' : 'hidden'">
      <div class="flex flex-col pb-3 mt-4">
        <div class="flex justify-between items-center mb-2">
          <span>Description</span>
          <Button
            v-show="!markdownPreview"
            v-on:click="toggleMarkdownPreview"
            :button-type="ButtonType.Primary"
          >
            Preview
          </Button>
          <Button
            v-show="markdownPreview"
            v-on:click="toggleMarkdownPreview"
            :button-type="ButtonType.Primary"
          >
            Stop preview
          </Button>
        </div>
        <textarea
          id="description"
          class="px-3 py-1 rounded border border-gray-300 h-32"
          v-model="description"
          v-show="!markdownPreview"
        ></textarea>
        <div
          class="markdown-preview px-3 py-1 rounded border border-gray-300 h-32"
          v-show="markdownPreview"
          v-html="markdownPreviewText"
        ></div>
      </div>
      <DeckCostCurve :deck="deck" type-attribute="Card Type" />
    </div>
  </div>
</template>
