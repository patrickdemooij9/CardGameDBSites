<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import type { DeckTypeSettingsApiModel } from "~/api/default";
import { RestrictionType } from "~/api/default";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import type CreateDeckSlot from "./models/CreateDeckSlot";
import {
  PhCaretDown,
  PhCaretRight,
  PhCheckCircle,
  PhGear,
  PhWarning,
} from "@phosphor-icons/vue";
import DeckBuilderTab from "./DeckBuilderTab";
import Button from "~/components/shared/Button.vue";
import CreateDeckGroup from "./models/CreateDeckGroup";
import type { CreateDeckSelectedArea } from "./models/CreateDeckSelectedArea";
import ButtonType from "~/components/shared/ButtonType";
import { GetValidCards } from "~/services/requirements/RequirementService";
import { useCollection } from "~/composables/useCollection";
import { useAccountStore } from "~/stores/AccountStore";
import DeckBuilderSlot from "./DeckBuilderSlot.vue";

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
  selectedArea?: CreateDeckSelectedArea;
  isSubmitting?: boolean;
}>();

const accountStore = useAccountStore();

function clickSlot(
  group: CreateDeckGroup,
  slot: CreateDeckSlot,
  isChild: boolean,
) {
  emit("selectSlot", {
    slot: slot,
    group: group,
  });
}

const collectionService = useCollection();
const isSideboardOpen = ref(false);

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

watch(
  () => props.collectionOnlyMode,
  (newValue) => {
    if (newValue) {
      ensureCollectionLoaded();
    }
  },
);

watch(
  () => props.deck.getSideboardAmount(),
  (amount, previousAmount) => {
    if (
      amount > 0 &&
      (previousAmount === undefined || amount > previousAmount)
    ) {
      isSideboardOpen.value = true;
    }
  },
  { immediate: true },
);

function toggleSideboard() {
  isSideboardOpen.value = !isSideboardOpen.value;
}

const hasPassiveRequirements = computed(() => {
  return props.deck.groups.some(
    (group) =>
      group.requirements.some(
        (req) => req.restrictionType === RestrictionType.PASSIVE,
      ) ||
      group.slots.some((slot) =>
        slot.requirements.some(
          (req) => req.restrictionType === RestrictionType.PASSIVE,
        ),
      ),
  );
});
</script>

<template>
  <div
    class="md:flex md:flex-col md:w-1/3 w-full shrink-0 bg-white"
    :class="currentTab !== DeckBuilderTab.Deck ? 'hidden' : 'block'"
  >
    <div class="md:sticky md:top-0">
      <div
        class="md:overflow-auto px-4 md:px-8 py-4 tooltip-container"
        id="squad-panel"
      >
        <div class="flex items-center justify-between">
          <div class="flex gap-2 items-center mb-2">
            <h1 class="text-base">Create deck</h1>
            <PhCheckCircle
              class="text-green-600"
              title="Deck is legal"
              v-if="deck.isLegalDeck()"
            />
            <PhWarning
              class="text-yellow-600"
              title="Deck is NOT legal"
              v-else
            />
          </div>
          <VDropdown>
            <button class="border px-2 py-1 rounded hover:bg-gray-200">
              <PhGear />
            </button>
            <template #popper>
              <div class="px-4 py-2">
                <h3 class="text-base mb-2">Editor configurations</h3>
                <div
                  class="flex items-center gap-2"
                  v-if="hasPassiveRequirements"
                >
                  <input
                    type="checkbox"
                    id="filterMode"
                    @click="
                      $emit('ignorePassiveFilters', !ignorePassiveFilters)
                    "
                    v-bind:checked="!ignorePassiveFilters"
                  />
                  <label for="filterMode" class="flex items-center gap-2">
                    Only show cards that fit in the deck.
                  </label>
                </div>
                <div class="flex items-center gap-2 mt-2">
                  <input
                    type="checkbox"
                    id="collectionMode"
                    @click="$emit('collectionOnlyMode', !collectionOnlyMode)"
                    v-bind:checked="collectionOnlyMode"
                  />
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
        <div
          class="squad-column mt-4"
          v-for="group in deck.groups.filter(
            (group) => group.getMaxAmount() > 0,
          )"
        >
          <div class="flex items-center justify-between">
            <h3 class="text-base">{{ group.name }}</h3>
            <div class="flex items-center gap-2">
              <span v-if="!deck.isLegalDeck()" class="text-red-600 text-xs"
                >Illegal cards found</span
              >
              <p>{{ group.getAmount() }} / {{ group.getMaxAmount() }}</p>
            </div>
          </div>
          <hr />
          <div class="mt-2" v-for="slot in group.slots" :key="slot.id">
            <DeckBuilderSlot
              :deck="deck"
              :group="group"
              :slot="slot"
              :deck-type-settings="deckTypeSettings!"
              :selected-area="selectedArea"
              :collection-only-mode="collectionOnlyMode"
              :allow-move-to-sideboard="true"
              @select-card="$emit('selectCard', $event)"
              @click-slot="clickSlot"
            />
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

        <!-- Sideboard section -->
        <div class="squad-column mt-4" v-if="deck.hasSideboard">
          <button
            type="button"
            class="w-full flex items-center justify-between text-left"
            @click="toggleSideboard"
          >
            <h3 class="text-base flex items-center gap-1">
              <PhCaretDown
                v-if="isSideboardOpen"
                class="transition-transform duration-150"
              />
              <PhCaretRight v-else class="transition-transform duration-150" />
              Sideboard
            </h3>
            <div class="flex items-center gap-2" v-if="deck.sideboardGroups.length == 1">
              <p>
                {{ deck.getSideboardAmount() }} /
                {{ deck.getSideboardMaxAmount() }}
              </p>
            </div>
          </button>
          <hr />
          <div
            class="squad-column mt-4"
            v-if="isSideboardOpen"
            v-for="group in deck.sideboardGroups.filter(
              (group) => group.getMaxAmount() > 0,
            )"
          >
            <template v-if="deck.sideboardGroups.length > 1">
              <div
                class="flex items-center justify-between"
                v-if="deck.sideboardGroups.length > 1"
              >
                <h3 class="text-base">{{ group.name }}</h3>
                <div class="flex items-center gap-2">
                  <span v-if="!deck.isLegalDeck()" class="text-red-600 text-xs"
                    >Illegal cards found</span
                  >
                  <p>{{ group.getAmount() }} / {{ group.getMaxAmount() }}</p>
                </div>
              </div>
              <hr />
            </template>
            <div class="mt-2" v-for="slot in group.slots" :key="slot.id">
              <DeckBuilderSlot
                :deck="deck"
                :group="group"
                :slot="slot"
                :deck-type-settings="deckTypeSettings!"
                :selected-area="selectedArea"
                :collection-only-mode="collectionOnlyMode"
                :allow-move-to-sideboard="true"
                @select-card="$emit('selectCard', $event)"
                @click-slot="clickSlot"
              />
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
        </div>

        <div class="flex justify-between mt-4">
          <div class="flex gap-2">
            <Button
              v-if="accountStore.isLoggedIn"
              v-on:click="$emit('submitForm', false)"
              :loading="isSubmitting"
              class="border border-black"
            >
              Save
            </Button>
            <Button
              :button-type="ButtonType.Success"
              v-on:click="$emit('submitForm', true)"
              :loading="isSubmitting"
              class="border border-black disabled:border-none"
              v-bind:disabled="!deck.validate().isValid()"
            >
              Publish
            </Button>
          </div>
          <VDropdown
            v-if="
              deck.validate().items.filter((item) => item.showMessage).length >
              0
            "
            :distance="6"
            :placement="'top'"
            :triggers="['hover', 'click']"
          >
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
                    v-for="(item, index) in deck
                      .validate()
                      .items.filter((item) => item.showMessage)"
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
