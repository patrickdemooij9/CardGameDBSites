<script setup lang="ts">
import { GetValidCards } from "~/services/requirements/RequirementService";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import type CreateDeckSlot from "./models/CreateDeckSlot";
import type {
  CardDetailApiModel,
  DeckTypeSettingsApiModel,
} from "~/api/default";
import type CreateDeckGroup from "./models/CreateDeckGroup";
import CmsImage from "~/components/shared/CmsImage.vue";
import type { CreateDeckSelectedArea } from "./models/CreateDeckSelectedArea";
import { GetCardValue } from "~/helpers/CardHelper";
import { PhArrowDown, PhArrowUp, PhMinus, PhPlus, PhTrash } from "@phosphor-icons/vue";

const props = defineProps<{
  deck: CreateDeckModel;
  group: CreateDeckGroup;
  slot: CreateDeckSlot;
  selectedArea?: CreateDeckSelectedArea;
  deckTypeSettings: DeckTypeSettingsApiModel;
  collectionOnlyMode: boolean;
  allowMoveFromSideboard?: boolean;
}>();

const emit = defineEmits<{
  (e: "selectCard", card: CardDetailApiModel): void;
  (
    e: "clickSlot",
    group: CreateDeckGroup,
    slot: CreateDeckSlot,
    isMainSlot: boolean,
  ): void;
}>();

const collectionService = useCollection();

function getDisplayClassesForItem(slot: CreateDeckSlot) {
  const classes = [];
  if (slot.displaySize === "Small") {
    classes.push("py-1");
  }
  return classes;
}

function isCardIllegal(card: CardDetailApiModel): boolean {
  return !props.deck.isLegalCard(card);
}

function getImagesForCard(card: CardDetailApiModel): string[] {
  const images: string[] = [];
  props.deckTypeSettings?.imageRules?.forEach((rule) => {
    if (GetValidCards([card], rule.requirements ?? []).includes(card)) {
      images.push(rule.imageUrl);
    }
  });
  return images;
}

function getPoints(card: CardDetailApiModel): number | null {
  return GetCardValue<number>(card, "Points");
}

function canAddCardToSlot(
  slot: CreateDeckSlot,
  card: CardDetailApiModel,
): boolean {
  const existingInOtherSlots = props.deck.getCardAmountInOtherSlots(slot, card);
  return slot.canAddCard(card, existingInOtherSlots);
}

function getOwnedAmount(cardBaseId: number | undefined): number {
  if (cardBaseId === undefined) return 0;
  return collectionService.getAmount(cardBaseId);
}

function hasEnoughInCollection(
  card: CardDetailApiModel,
  amount: number,
): boolean {
  if (!card.baseId) return false;
  const owned = getOwnedAmount(card.baseId);
  return owned >= amount;
}

function getFirstSlotForCard(card: CardDetailApiModel): CreateDeckSlot | undefined {
  const slots = props.deck.getSlotsForCard(card);
  return slots.length > 0 ? slots[0] : undefined;
}
</script>

<template>
  <div v-for="cardGroup in slot.cardGroups">
    <div v-if="cardGroup.cards.length > 0">
      <h2 class="text-sm mb-1" v-if="cardGroup.displayName">
        {{ cardGroup.displayName }}
      </h2>
      <div v-for="item in cardGroup.getOrderedCards()" :key="item.card.baseId">
        <div
          class="flex items-center border rounded mb-2 cursor-pointer tooltip-starter"
          :class="[
            ...getDisplayClassesForItem(slot),
            isCardIllegal(item.card) ? 'border-red-500' : 'border-gray-300',
          ]"
          v-on:click.prevent="emit('selectCard', item.card)"
        >
          <template v-for="imageUrl in getImagesForCard(item.card)" :key="imageUrl">
            <CmsImage
              :src="imageUrl"
              class="rounded-md"
              :class="slot.displaySize == 'Medium' ? 'h-12' : 'h-4 pl-1'"
              :alt="item.card.displayName"
            />
          </template>
          <CmsImage
            v-if="getImagesForCard(item.card).length === 0"
            :src="item.card.imageUrl"
            crop="icon"
            class="rounded-md"
            :class="slot.displaySize == 'Medium' ? 'h-12' : 'h-4 pl-1'"
            :alt="item.card.displayName"
          />
          <div class="flex grow justify-between px-4">
            <div
              class="flex gap-4 items-center justify-between grow mr-2 cursor-source"
              v-cursor-image="item.card.imageUrl?.url"
            >
              <span class="name">{{ item.card.displayName }}</span>
              <span v-if="getPoints(item.card) !== null"
                >({{ Math.abs(getPoints(item.card) ?? 0) }})</span
              >
              <span
                class="shrink-0"
                v-if="slot.numberMode && !collectionOnlyMode"
                >{{ item.amount }} x</span
              >
              <span
                class="shrink-0 font-bold"
                :class="
                  hasEnoughInCollection(item.card, item.amount)
                    ? 'text-green-600'
                    : 'text-red-500'
                "
                v-if="collectionOnlyMode && item.card.baseId"
                >{{ getOwnedAmount(item.card.baseId) }}/{{ item.amount }}
              </span>
            </div>
            <a
              v-if="
                !slot.numberMode && !slot.disableRemoval && item.allowRemoval
              "
              href="#"
              class="flex items-center justify-center ml-2 no-underline"
              v-on:click.prevent.stop="slot.removeCard(item.card)"
            >
              <PhTrash />
            </a>
            <div
              v-if="
                slot.numberMode && !slot.disableRemoval && item.allowRemoval
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
                :disabled="!canAddCardToSlot(slot, item.card)"
                v-on:click.prevent.stop="slot.addCard(item.card)"
              >
                <PhPlus />
              </button>
              <button
                v-if="slot.allowMovingToSideboard"
                class="border border-gray-300 rounded-lg p-1 flex h-fit no-underline"
                v-on:click.prevent.stop="
                  deck.moveCard(slot, deck.sideboardSlot!, item.card)
                "
              >
              <PhArrowDown/>
              </button>
              <button
                v-if="allowMoveFromSideboard && getFirstSlotForCard(item.card)"
                class="border border-gray-300 rounded-lg p-1 flex h-fit no-underline"
                v-on:click.prevent.stop="
                  deck.moveCard(deck.sideboardSlot!, getFirstSlotForCard(item.card)!, item.card)
                "
              >
                <PhArrowUp/>
              </button>
            </div>
          </div>
        </div>

        <div v-for="childSlot in item.children">
          <div v-for="childGroup in childSlot.cardGroups">
            <div v-for="child in childGroup.getOrderedCards()">
              <div
                class="flex items-center border rounded ml-4 mb-2 cursor-pointer tooltip-starter"
                :class="getDisplayClassesForItem(childSlot)"
                v-on:click.prevent="emit('selectCard', child.card)"
              >
                <div class="flex grow justify-between px-4">
                  <div
                    class="flex gap-4 items-center justify-between grow mr-2 cursor-source"
                    v-cursor-image="child.card.imageUrl?.url"
                  >
                    <span class="name">{{ child.card.displayName }}</span>
                    <span
                      class="shrink-0 font-bold"
                      :class="
                        hasEnoughInCollection(child.card, child.amount)
                          ? 'text-green-600'
                          : 'text-red-500'
                      "
                      v-if="collectionOnlyMode && child.card.baseId"
                      >{{ getOwnedAmount(child.card.baseId) }}/{{
                        child.amount
                      }}</span
                    >
                  </div>
                  <a
                    v-if="
                      !childSlot.numberMode &&
                      !childSlot.disableRemoval &&
                      child.allowRemoval
                    "
                    href="#"
                    class="flex items-center justify-center ml-2 no-underline"
                    v-on:click.prevent.stop="childSlot.removeCard(child.card)"
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
            v-on:click="emit('clickSlot', props.group, childSlot, true)"
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
    v-on:click="emit('clickSlot', props.group, slot, true)"
  >
    <span>{{ slot.label }}</span>
  </div>
</template>
