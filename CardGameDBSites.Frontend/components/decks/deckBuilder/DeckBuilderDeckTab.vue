<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import type { DeckTypeSettingsApiModel } from "~/api/default";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import type CreateDeckSlot from "./models/CreateDeckSlot";
import { PhCaretRight, PhCheckCircle, PhGear, PhMinus, PhPlus, PhTrash, PhWarning } from "@phosphor-icons/vue";
import DeckBuilderTab from "./DeckBuilderTab";
import Button from "~/components/shared/Button.vue";
import CreateDeckGroup from "./models/CreateDeckGroup";
import type { CreateDeckSelectedArea } from "./models/CreateDeckSelectedArea";
import ButtonType from "~/components/shared/ButtonType";
import { GetCrop } from "~/helpers/CropUrlHelper";
import { GetValidCards, IsValid } from "~/services/requirements/RequirementService";
import { useCollection } from "~/composables/useCollection";

const name = defineModel<string>("name");

const emit = defineEmits<{
  (e: "submitForm", publish: boolean): void;
  (e: "ignorePassiveFilters", ignore: boolean): void;
  (e: "collectionOnlyMode", enabled: boolean): void;
  (e: "selectCard", value: CardDetailApiModel): void;
  (e: "selectSlot", value: CreateDeckSelectedArea): void;
}>();

const props = defineProps<{
  deck: CreateDeckModel;
  deckTypeSettings?: DeckTypeSettingsApiModel;
  currentTab: DeckBuilderTab;
  ignorePassiveFilters: boolean;
  collectionOnlyMode: boolean;
  selectedArea?: CreateDeckSelectedArea
}>();

const accountStore = useAccountStore();
const collectionMode = ref<boolean>(false); //TODO: get from deck

function getImagesForCard(card: CardDetailApiModel): string[] {
  const images: string[] = [];
  props.deckTypeSettings?.imageRules?.forEach((rule) => {
    if (GetValidCards([card], rule.requirements ?? []).includes(card)) {
      images.push(rule.imageUrl);
    }
  });
  return images;
}

function getDisplayClassesForItem(slot: CreateDeckSlot) {
  const classes = [];
  if (slot.displaySize === "Small") {
    classes.push("py-1");
  }

  /*if (item && !this.hasEnoughInCollection(item.card.id, item.amount)) {
        classes.push('border-red-300')
      } else {
        classes.push('border-gray-300')
      }*/
  return classes;
}

function clickSlot(
  group: CreateDeckGroup,
  slot: CreateDeckSlot,
  isChild: boolean
) {
  emit("selectSlot", {
    slot: slot,
    group: group,
  });
}

function getAbilityValueByType<T>(card: CardDetailApiModel, ability: string) {
  return (
    (Object.entries(card.attributes!).find(
      (entry) => entry[0] === ability
    )?.[1] as T[]) ?? []
  );
}

function isCardIllegal(card: CardDetailApiModel): boolean {
  return !props.deck.isLegalCard(card);
}

const collectionService = useCollection();

function getAllDeckCardIds(): number[] {
  const cardIds = new Set<number>();
  props.deck.getCards().forEach((deckCard) => {
    if (deckCard.card.baseId) {
      cardIds.add(deckCard.card.baseId);
    }
  });
  return Array.from(cardIds);
}

function ensureCollectionLoaded() {
  const cardIds = getAllDeckCardIds();
  if (cardIds.length > 0) {
    collectionService.loadCards(cardIds);
  }
}

onMounted(() => {
  if (props.collectionOnlyMode) {
    ensureCollectionLoaded();
  }
});

watch(() => props.collectionOnlyMode, (newValue) => {
  if (newValue) {
    ensureCollectionLoaded();
  }
});

function getOwnedAmount(cardBaseId: number | undefined): number {
  if (cardBaseId === undefined) return 0;
  return collectionService.getAmount(cardBaseId);
}

function getNeededAmount(card: CardDetailApiModel): number {
  let totalNeeded = 0;
  if (!card.baseId) return 0;

  props.deck.getCards().forEach((deckCard) => {
    if (deckCard.card.baseId === card.baseId) {
      totalNeeded += deckCard.amount;
    }
  });
  return totalNeeded;
}

function hasEnoughInCollection(card: CardDetailApiModel): boolean {
  if (!card.baseId) return false;
  const owned = getOwnedAmount(card.baseId);
  const needed = getNeededAmount(card);
  return owned >= needed;
}
</script>

<template>
  <div
    class="md:flex md:flex-col md:w-1/3 w-full shrink-0 bg-white"
    :class="currentTab !== DeckBuilderTab.Deck ? 'hidden' : 'block'"
  >
    <div class="sticky top-0">
      <div
        class="md:overflow-auto px-4 md:px-8 py-4 tooltip-container"
        id="squad-panel"
      >
        <div class="flex items-center justify-between">
          <div class="flex gap-2 items-center mb-2">
            <h1 class="text-base">Create deck</h1>
            <PhCheckCircle class="text-green-600" title="Deck is legal" v-if="deck.isLegalDeck()"/>
            <PhWarning class="text-yellow-600" title="Deck is NOT legal" v-else/>
          </div>
          <VDropdown>
            <button class="border px-2 py-1 rounded hover:bg-gray-200">
              <PhGear/>
            </Button>
            <template #popper>
              <div class="px-4 py-2">
                <h3 class="text-base mb-2">Editor configurations</h3>
                <div class="flex items-center gap-2">
                  <input type="checkbox" id="filterMode" @click="$emit('ignorePassiveFilters', !ignorePassiveFilters)" v-bind:checked="!ignorePassiveFilters" />
                  <label for="filterMode" class="flex items-center gap-2">
                    Only show cards that fit in the deck.
                  </label>
                </div>
                <div class="flex items-center gap-2 mt-2">
                  <input type="checkbox" id="collectionMode" @click="$emit('collectionOnlyMode', !collectionOnlyMode)" v-bind:checked="collectionOnlyMode" />
                  <label for="collectionMode" class="flex items-center gap-2">
                    Only show cards in my collection.
                  </label>
                </div>
              </div>
            </template>
          </VDropdown>
          
        </div>
        
        <input
          type="text"
          id="name"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          v-model="name"
          maxlength="60"
          placeholder="Name"
        />
        <!--<template v-if="Object.keys(ownedCharacters).length > 0">
                            <div class="flex gap-2 rounded py-2">
                                <input type="checkbox" id="collectionMode" v-model="collectionMode" />
                                <label for="collectionMode">Match with collection</label>
                            </div>
                        </template>-->
        <div class="squad-column mt-4" v-for="group in deck.groups">
          <div class="flex items-center justify-between">
            <h3 class="text-base">{{ group.name }}</h3>
            <div class="flex items-center gap-2">
              <span v-if="!deck.isLegalDeck()" class="text-red-600 text-xs">Illegal cards found</span>
              <p>{{ group.getAmount() }} / {{ group.getMaxAmount() }}</p>
            </div>
          </div>
          <hr />
          <div class="mt-2" v-for="slot in group.slots" :key="slot.id">
            <div v-for="cardGroup in slot.cardGroups">
              <div v-if="cardGroup.cards.length > 0">
                <h2 class="text-sm mb-1" v-if="cardGroup.displayName">
                  {{ cardGroup.displayName }}
                </h2>
                <div
                  v-for="item in cardGroup.getOrderedCards()"
                  :key="item.card.baseId"
                >
                  <div
                    class="flex items-center border rounded mb-2 cursor-pointer tooltip-starter"
                    :class="[...getDisplayClassesForItem(slot), isCardIllegal(item.card) ? 'border-red-500' : 'border-gray-300']"
                    v-on:click.prevent="emit('selectCard', item.card)"
                  >
                    <template v-for="imageUrl in getImagesForCard(item.card)">
                      <img
                        :src="imageUrl"
                        class="rounded-md"
                        :class="slot.displaySize == 'Medium' ? 'h-12' : 'h-4 pl-1'"
                      />
                    </template>
                    <img
                      v-if="getImagesForCard(item.card).length === 0"
                      :src="GetCrop(item.card.imageUrl, 'icon')!"
                      class="rounded-md"
                      :class="slot.displaySize == 'Medium' ? 'h-12' : 'h-4 pl-1'"
                    />
                    <!--<img
                      v-for="iconUrl in item.card.iconUrls"
                      class="rounded-md"
                      :class="
                        slot.displaySize == 'Medium' ? 'h-12' : 'h-4 pl-1'
                      "
                      :src="iconUrl"
                    />-->
                    <div class="flex grow justify-between px-4">
                      <div
                        class="flex gap-4 items-center justify-between grow mr-2 cursor-source"
                        v-cursor-image="item.card.imageUrl?.url"
                      >
                        <span class="name">{{ item.card.displayName }}</span>
                        <span
                          v-if="
                            getAbilityValueByType(item.card, 'Points').length >
                            0
                          "
                          >({{
                            Math.abs(
                              getAbilityValueByType<number>(item.card, "Points")[0]
                            )
                          }})</span>
                        <span
                          class="shrink-0"
                          v-if="slot.numberMode && !collectionOnlyMode"
                          >{{ item.amount }} x</span
                        >
                        <span
                          class="shrink-0 font-bold"
                          :class="
                            hasEnoughInCollection(item.card)
                              ? 'text-green-600'
                              : 'text-red-500'
                          "
                          v-if="collectionOnlyMode && item.card.baseId"
                          >{{ getOwnedAmount(item.card.baseId) }}/{{
                            getNeededAmount(item.card)
                          }}</span
                        >
                      </div>
                      <a
                        v-if="
                          !slot.numberMode &&
                          !slot.disableRemoval &&
                          item.allowRemoval
                        "
                        href="#"
                        class="flex items-center justify-center ml-2 no-underline"
                        v-on:click.prevent.stop="slot.removeCard(item.card)"
                      >
                        <PhTrash />
                      </a>
                      <div
                        v-if="
                          slot.numberMode &&
                          !slot.disableRemoval &&
                          item.allowRemoval
                        "
                        class="flex items-center gap-2"
                      >
                        <button
                          href="#"
                          class="border border-gray-300 rounded-lg p-1 flex h-fit no-underline"
                          v-on:click.prevent.stop="slot.removeCard(item.card)"
                        >
                          <PhMinus />
                        </button>
                        <button
                          class="border border-gray-300 rounded-lg p-1 flex h-fit no-underline disabled:bg-gray-300 disabled:cursor-not-allowed"
                          :disabled="!slot.canAddCard(item.card)"
                          v-on:click.prevent.stop="slot.addCard(item.card)"
                        >
                          <PhPlus />
                        </button>
                      </div>
                    </div>
                  </div>

                  <div v-for="childSlot in item.children">
                    <div v-for="childGroup in childSlot.cardGroups">
                      <div v-for="child in childGroup.getOrderedCards()">
                        <div
                          class="flex items-center border rounded ml-4 mb-2 cursor-pointer tooltip-starter"
                          :class="getDisplayClassesForItem(slot)"
                          v-on:click.prevent="emit('selectCard', child.card)"
                        >
                          <div class="flex grow justify-between px-4">
                            <div
                              class="flex gap-4 items-center justify-between grow mr-2 cursor-source"
                              v-cursor-image="child.card.imageUrl?.url"
                            >
                              <span class="name">{{
                                child.card.displayName
                              }}</span>
                              <span
                                class="shrink-0 font-bold"
                                :class="
                                  hasEnoughInCollection(child.card)
                                    ? 'text-green-600'
                                    : 'text-red-500'
                                "
                                v-if="collectionOnlyMode && child.card.baseId"
                                >{{ getOwnedAmount(child.card.baseId) }}/{{
                                  getNeededAmount(child.card)
                                }}</span
                              >
                            </div>
                            <a
                              v-if="
                                !slot.numberMode &&
                                !slot.disableRemoval &&
                                item.allowRemoval
                              "
                              href="#"
                              class="flex items-center justify-center ml-2 no-underline"
                              v-on:click.prevent.stop="
                                slot.removeCard(item.card)
                              "
                            >
                              <PhTrash />
                            </a>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div
                      v-if="!childSlot.isFull()"
                      class="flex items-center justify-center border border-dashed border-gray-300 bg-gray-100 hover:bg-gray-300 cursor-pointer ml-4 px-4 rounded"
                      v-on:click="clickSlot(group, childSlot, true)"
                    >
                      <span>{{ childSlot.label }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div
              v-if="!slot.isFull()"
              class="flex items-center justify-center w-full h-12 border bg-gray-100 hover:bg-gray-300 cursor-pointer px-4 py-2 rounded"
              :class="[
                selectedArea?.slot === slot
                  ? 'bg-gray-200 border-gray-500'
                  : 'border-dashed border-gray-300',
              ]"
              v-on:click="clickSlot(group, slot, true)"
            >
              <span>{{ slot.label }}</span>
            </div>
          </div>
          <!--<div class="deck-rules">
            <p v-if="!hasEnoughPoints(squad)" class="invalid">
              <i class="ph ph-warning"></i>
              <span
                >Your squad has {{ Math.abs(pointsLeft(squad)) }} points too
                many.</span
              >
            </p>
          </div>-->
        </div>
        <div class="flex justify-between mt-4">
          <div class="flex gap-2">
            <Button
              v-if="accountStore.isLoggedIn"
              v-on:click="$emit('submitForm', false)"
              class="border border-black"
            >
              Save
            </Button>
            <Button
              :button-type="ButtonType.Success"
              v-on:click="$emit('submitForm', true)"
              class="border border-black disabled:border-none"
              v-bind:disabled="!deck.validate().isValid()"
            >
              Publish
            </Button>
          </div>
          <VDropdown v-if="
                deck.validate().items.filter((item) => item.showMessage)
                  .length > 0
              "
              :distance="6"
              :placement="'top'"
              :triggers="['hover','click']">
            <Button
              :button-type="ButtonType.Danger"
              class="border border-red-600"
            >
              <p class="flex items-center gap-2">
                Errors
                <PhCaretRight />
              </p>
            </Button>
            <template #popper>
              <div class="px-8 py-6">
                <ul class="">
                  <li
                    v-for="(item, index) in deck.validate().items.filter((item) => item.showMessage)"
                    :key="index"
                    class="text-red-600"
                  >
                    {{ item.errorMessage }}
                  </li>
                </ul>
              </div>
            </template>
          </VDropdown>
        </div>
      </div>
    </div>
  </div>
</template>
