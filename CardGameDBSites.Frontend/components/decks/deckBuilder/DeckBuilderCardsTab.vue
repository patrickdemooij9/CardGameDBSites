<script setup lang="ts">
import BaseCardOverview from "~/components/overviews/BaseCardOverview.vue";
import DeckBuilderTab from "./DeckBuilderTab";
import type { CardDetailApiModel } from "~/api/default";
import type CreateDeckSlot from "./models/CreateDeckSlot";
import { GetFilters } from "~/services/requirements/RequirementService";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import Button from "~/components/shared/Button.vue";
import ButtonType from "~/components/shared/ButtonType";
import { GetMarkdown } from "~/services/MarkdownService";
import type { CreateDeckSelectedArea } from "./models/CreateDeckSelectedArea";
import { GetCrop } from "~/helpers/CropUrlHelper";

const emit = defineEmits<{
  (e: "update:currentTab", value: DeckBuilderTab): void;
  (e: "selectCard", value: CardDetailApiModel): void;
}>();

const props = defineProps<{
  currentTab: DeckBuilderTab;
  currentArea?: CreateDeckSelectedArea;
  deck: CreateDeckModel;
  preselectFirstSlot: boolean;
  ignorePassiveFilters: boolean;
}>();

const description = ref("");
const markdownPreview = ref(false);
const markdownPreviewText = ref("");

function getInternalFilters() {
  if (!props.currentArea) {
    return [];
  }
  const cards = props.currentArea.group.getCards();
  return [
    ...GetFilters(cards, props.currentArea.group.requirements, props.ignorePassiveFilters),
    ...GetFilters(cards, props.currentArea.slot.requirements, props.ignorePassiveFilters)
  ];
}

function getSlotsForCard(card: CardDetailApiModel) {
  return props.deck.getSlotsForCard(card).filter((slot) => props.currentArea === undefined || props.currentArea.slot === slot);
}

function addToSquad(slot: CreateDeckSlot, character: CardDetailApiModel) {
  slot.addCard(character);
  // Logic to add the character to the squad
}

function removeFromSquad(slot: CreateDeckSlot, card: CardDetailApiModel) {
  slot.removeCard(card);
}

async function toggleMarkdownPreview(){
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
    <div class="gap-4 text-lg hidden md:flex">
      <button
        class="hover:border-b-2 hover:border-main-color"
        type="button"
        :class="
          currentTab === DeckBuilderTab.Cards ||
          currentTab === DeckBuilderTab.Deck
            ? 'border-b-2 border-main-color'
            : ''
        "
        v-on:click="emit('update:currentTab', DeckBuilderTab.Cards)"
      >
        Cards
      </button>
      <button
        class="hover:border-b-2 hover:border-main-color"
        type="button"
        :class="
          currentTab === DeckBuilderTab.Details
            ? 'border-b-2 border-main-color'
            : ''
        "
        v-on:click="emit('update:currentTab', DeckBuilderTab.Details)"
      >
        Details
      </button>
    </div>
    <div class="mt-4" :class="{'md:block': currentTab === DeckBuilderTab.Cards || currentTab === DeckBuilderTab.Deck, 'hidden': currentTab !== DeckBuilderTab.Cards}">
      <BaseCardOverview
        :filters="[]"
        :internal-filters="getInternalFilters()"
        :white-background="false"
        v-slot="{ cards }"
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
                <div class="flex flex-col align-middle gap-2" v-if="!preselectFirstSlot">
                  <div
                    v-for="location in getSlotsForCard(character)"
                    class="flex justify-between"
                    :class="{
                      'w-full':
                        location.getMaxAmount() > 1 || location.minCards > 1,
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
                    <div v-if="location.canAddCard(character)">
                      <div>
                        <button
                          v-if="!location.numberMode"
                          v-on:click.prevent.stop="addToSquad(location, character)"
                          type="button"
                          class="pointer rounded-lg bg-white text-black border-none w-fit px-4 py-3"
                        >
                          <span>Add to {{ location.label }}</span>
                        </button>
                        <button
                          v-if="location.numberMode"
                          v-on:click.prevent.stop="addToSquad(location, character)"
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
            <div class="flex flex-col align-middle gap-2 mt-2" v-if="preselectFirstSlot">
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
                    v-on:click.prevent.stop="
                      addToSquad(location, character)
                    "
                    type="button"
                    :disabled="
                      !location.canAddCard(character)
                    "
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
      <div class="flex flex-col pb-3">
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
    </div>
  </div>
</template>
