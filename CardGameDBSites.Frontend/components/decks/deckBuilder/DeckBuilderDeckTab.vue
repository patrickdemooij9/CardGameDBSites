<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import type CreateDeckSlot from "./models/CreateDeckSlot";
import { PhCaretRight, PhGear, PhMinus, PhPlus, PhTrash } from "@phosphor-icons/vue";
import DeckBuilderTab from "./DeckBuilderTab";
import Button from "~/components/shared/Button.vue";
import CreateDeckGroup from "./models/CreateDeckGroup";
import type { CreateDeckSelectedArea } from "./models/CreateDeckSelectedArea";
import ButtonType from "~/components/shared/ButtonType";
import { placements } from "floating-vue";
import { GetCrop } from "~/helpers/CropUrlHelper";

const selectedArea = defineModel<CreateDeckSelectedArea>("selectedArea");
const name = defineModel<string>("name");

defineEmits<{
  (e: "submitForm", publish: boolean): void;
  (e: "ignorePassiveFilters", ignore: boolean): void;
}>();

defineProps<{
  deck: CreateDeckModel;
  currentTab: DeckBuilderTab;
  ignorePassiveFilters: boolean;
}>();

const accountStore = useAccountStore();
const collectionMode = ref<boolean>(false); //TODO: get from deck

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

function selectCharacter(card: any) {}

function clickSlot(
  group: CreateDeckGroup,
  slot: CreateDeckSlot,
  isChild: boolean
) {
  selectedArea.value = {
    slot: slot,
    group: group,
  };
}

function getAbilityValueByType<T>(card: CardDetailApiModel, ability: string) {
  return (
    (Object.entries(card.attributes!).find(
      (entry) => entry[0] === ability
    )?.[1] as T[]) ?? []
  );
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
          <h1 class="text-base mb-2">Create deck</h1>
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
            <p>{{ group.getAmount() }} / {{ group.getMaxAmount() }}</p>
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
                    :class="getDisplayClassesForItem(slot)"
                    v-on:click="selectCharacter(item.card)"
                  >
                    <img
                      :src="GetCrop(item.card.imageUrl, undefined)!"
                      class="rounded-md"
                      :class="
                        slot.displaySize == 'Medium' ? 'h-12' : 'h-4 pl-1'
                      "
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
                        v-cursor-image="item.card.imageUrl"
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
                          v-if="slot.numberMode && !collectionMode"
                          >{{ item.amount }} x</span
                        >
                        <!--<span
                          class="shrink-0"
                          :class="
                            !hasEnoughInCollection(item.card.id, item.amount)
                              ? 'font-bold text-red-500'
                              : 'font-bold text-green-600'
                          "
                          v-if="collectionMode"
                          >{{ item.amount }} /
                          {{ getAllowedSizeForSlot(item.card) }}</span
                        >-->
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
                          v-on:click="selectCharacter(child.card)"
                        >
                          <div class="flex grow justify-between px-4">
                            <div
                              class="flex gap-4 items-center justify-between grow mr-2 cursor-source"
                              :data-cursor-image="child.card.imageUrl"
                            >
                              <span class="name">{{
                                child.card.displayName
                              }}</span>
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
              :placement="'right'"
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
