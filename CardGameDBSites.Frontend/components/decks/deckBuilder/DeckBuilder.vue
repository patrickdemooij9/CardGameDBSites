<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import DeckBuilderCardModal from "./DeckBuilderCardModal.vue";
import DeckBuilderCardsTab from "./DeckBuilderCardsTab.vue";
import DeckBuilderTab from "./DeckBuilderTab";
import DeckBuilderDeckTab from "./DeckBuilderDeckTab.vue";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import SiteService from "~/services/SiteService";
import DeckService from "~/services/DeckService";
import type { CreateDeckSelectedArea } from "./models/CreateDeckSelectedArea";

const route = useRoute();
let deckId: number | undefined = undefined;
if (route.query["id"]){
  const query = route.query["id"];
  if (!Array.isArray(query)){
    deckId = parseInt(query);
  }
}

const deckSettings = await new SiteService().getDeckBuilderSettings(1, deckId)!;

const currentTab = ref<DeckBuilderTab>(DeckBuilderTab.Deck);
const selectedCard = ref<CardDetailApiModel>();
const selectedArea = ref<CreateDeckSelectedArea>();
const deck = ref<CreateDeckModel>(deckSettings);

const ignorePassiveFilters = ref(false);

if (true) {
  //TODO: Depending on SelectFirstSlot
  selectedArea.value = {
    group: deckSettings.groups[0],
    slot: deckSettings.groups[0].slots[0],
  };
}

function clickTab(tab: DeckBuilderTab) {
  currentTab.value = tab;
}

function selectCard(card: CardDetailApiModel) {
  selectedCard.value = card;
}

async function submitForm(publish: boolean) {
  const result = await new DeckService().post(deck.value, publish);
  useRouter().push("/decks/" + result);
}

function handleScroll() {
  const panel = document.getElementById("squad-panel");
  if (!panel) return;
  if (document.body.scrollTop > 56 || document.documentElement.scrollTop > 56) {
    panel.classList.add("scrolled");
  } else {
    panel.classList.remove("scrolled");
  }
}

onMounted(() => {
  window.addEventListener("scroll", handleScroll);
});

onUnmounted(() => {
  window.removeEventListener("scroll", handleScroll);
});
</script>

<template>
  <div>
    <div class="container">
      <div class="flex gap-4 sticky top-0 z-10 text-md bg-white p-4 md:hidden">
        <button
          class="border-b-2 px-2 py-1"
          type="button"
          :class="
            currentTab === DeckBuilderTab.Deck
              ? 'border-2 border-main-color rounded-lg'
              : ''
          "
          @click="clickTab(DeckBuilderTab.Deck)"
        >
          <span>
            Deck {{ deck.getDeckAmount() }} / {{ deck.getDeckMaxAmount() }}
          </span>
        </button>
        <button
          class="border-b-2 px-2 py-1"
          type="button"
          :class="
            currentTab === DeckBuilderTab.Cards
              ? 'border-2 border-main-color rounded-lg'
              : ''
          "
          @click="clickTab(DeckBuilderTab.Cards)"
        >
          Cards
        </button>
        <button
          class="border-b-2 px-2 py-1"
          type="button"
          :class="
            currentTab === DeckBuilderTab.Details
              ? 'border-2 border-main-color rounded-lg'
              : ''
          "
          @click="clickTab(DeckBuilderTab.Details)"
        >
          Details
        </button>
      </div>
      <div class="flex">
        <DeckBuilderDeckTab
          :deck="deck"
          v-model:selected-area="selectedArea"
          v-model:name="deck.name"
          :current-tab="currentTab"
          :ignore-passive-filters="ignorePassiveFilters"
          @submit-form="submitForm"
          @ignore-passive-filters="ignorePassiveFilters = $event"
        />
        <DeckBuilderCardsTab
          v-model:current-tab="currentTab"
          :current-area="selectedArea"
          :deck="deck"
          @select-card="selectCard"
          :preselect-first-slot="true"
          :ignore-passive-filters="ignorePassiveFilters"
        />
      </div>
    </div>

    <DeckBuilderCardModal v-if="selectedCard" :selected-card="selectedCard" @close="selectedCard = undefined" />
    <div
      id="cursor-image"
      class="absolute bg-contain bg-no-repeat pointer-events-none w-48 h-72"
      style="display:none;"
    ></div>
  </div>
</template>
