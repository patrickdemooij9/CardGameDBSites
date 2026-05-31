<script setup lang="ts">
import type { DeckDetailContentModel } from "~/api/umbraco";
import DeckService from "~/services/DeckService";
import SetService from "~/services/SetService";
import DeckLike from "../decks/DeckLike.vue";
import type {
  CardDetailApiModel,
  CommentViewModel,
  DeckCardGroupApiModel,
  SetViewModel,
} from "~/api/default";
import { GetValidCards } from "~/services/requirements/RequirementService";
import DeckAction from "../decks/DeckAction.vue";
import { GetCrop } from "~/helpers/CropUrlHelper";
import { useCards } from "~/composables/useCards";
import { useCollection } from "~/composables/useCollection";
import { useMembers } from "~/composables/useMembers";
import { useSite } from "~/composables/useSite";
import { useAppToast } from "~/composables/useAppToast";
import { GetCardValue } from "~/helpers/CardHelper";
import CommentSection from "../comments/CommentSection.vue";
import { PhDotsThree } from "@phosphor-icons/vue";
import DeckDetailCardModal from "../decks/DeckDetailCardModal.vue";
import DeckCardGroup from "../decks/DeckCardGroup.vue";
import { useAccountStore } from "~/stores/AccountStore";

console.time("page-render");

defineProps<{
  content: DeckDetailContentModel;
}>();

const route = useRoute();
const router = useRouter();
let slug = route.params.slug as string[];
const deckId = Number.parseInt(slug[slug.length - 1]!);

console.time("fetch-deck");
const deck = await new DeckService().get(deckId);
console.timeEnd("fetch-deck");
if (!deck || deck === null) {
  throw createError({
    statusCode: 404,
    statusMessage: "Resource Not Found",
  });
}

console.time("fetch-related-data");
const accountService = useAccountStore();
const collectionService = useCollection();
const appToast = useAppToast();
const deckSettings = await useSite().getDeckTypeSettings(deck.typeId!);
const comments = ref(await useComments().loadCommentsByDeckId(deckId));
const cards = await useCards().loadCardsByIds(
  deck.cards
    ?.sort((a, b) => (a.slotId ?? 0) - (b.slotId ?? 0))
    .map((card) => card.cardId!) ?? [],
);
const sideboardCards = await useCards().loadCardsByIds(
  deck.sideboard
    ?.sort((a, b) => (a.slotId ?? 0) - (b.slotId ?? 0))
    .map((card) => card.cardId!) ?? [],
);
const childCardIds = [
  ...new Set(deck.cards?.flatMap((deckCard) => deckCard.children ?? []) ?? []),
];
const childCards = await useCards().loadCardsByIds(childCardIds);
const childCardsById = new Map(
  childCards
    .filter((card): card is CardDetailApiModel & { baseId: number } =>
      card.baseId !== undefined,
    )
    .map((card) => [card.baseId, card]),
);
const mainCards = GetValidCards(
  cards,
  deckSettings!.mainCardRequirements ?? [],
);
const mainCardsWithChildren = computed(() =>
  mainCards.map((mainCard) => ({
    mainCard,
    children:
      deck.cards
        ?.find((deckCard) => deckCard.cardId === mainCard.baseId)
        ?.children?.map((childId) => childCardsById.get(childId))
        .filter(
          (child): child is CardDetailApiModel & { baseId: number } =>
            child !== undefined,
        ) ?? [],
  })),
);

const sets = ref<SetViewModel[]>([]);
const setService = new SetService();
const uniqueSetIds = [
  ...new Set(
    cards.map((c) => c.setId).filter((id): id is number => id != null),
  ),
];
await Promise.all(
  uniqueSetIds.map((setId) =>
    setService.getById(setId).then((set) => {
      if (set) sets.value.push(set);
    }),
  ),
);

console.timeEnd("fetch-related-data");

console.time("member");
let createdBy = "Anonymous";
if (deck.createdBy) {
  createdBy = (await useMembers().loadMembersByIds([deck.createdBy]))[0]!
    .displayName;
}
console.timeEnd("member");

const isLoggedIn = ref(false);
const collectionMode = ref(false);
const deckDisplayMode = ref<"text" | "images">("text");
const isCreatingPreset = ref(false);
const showActionsDropdown = ref(false);
const selectedCardIndex = ref<number | undefined>(undefined);
const isAdmin = computed(() => accountService.member?.isAdmin === true);
const isOwner = computed(() => {
  const member = accountService.member;
  return member && deck.createdBy && member.id === deck.createdBy;
});
const groupedDeckCards = computed(() =>
  (deckSettings?.groupings ?? []).flatMap((group) =>
    getCardsInGroup(group, cards),
  ),
);
const hasDecklistCards = computed(() =>
  (deckSettings?.groupings ?? []).some(
    (group) => getCardsInGroup(group, cards).length > 0,
  ),
);
const hasSideboardCards = computed(
  () =>
    deck.sideboard != null &&
    deck.sideboard.length > 0 &&
    (deckSettings?.groupings ?? []).some(
      (group) => getCardsInGroup(group, sideboardCards).length > 0,
    ),
);
const showDecklistSection = computed(
  () => hasDecklistCards.value || hasSideboardCards.value,
);
const selectedCard = computed(() =>
  selectedCardIndex.value === undefined
    ? undefined
    : groupedDeckCards.value[selectedCardIndex.value],
);
function canNavigatePrevious() {
  return selectedCardIndex.value !== undefined && selectedCardIndex.value > 0;
}
function canNavigateNext() {
  return (
    selectedCardIndex.value !== undefined &&
    selectedCardIndex.value < groupedDeckCards.value.length - 1
  );
}
const previousCardName = computed(() => {
  if (!canNavigatePrevious()) {
    return undefined;
  }

  return groupedDeckCards.value[selectedCardIndex.value! - 1]?.displayName;
});
const nextCardName = computed(() => {
  if (!canNavigateNext()) {
    return undefined;
  }

  return groupedDeckCards.value[selectedCardIndex.value! + 1]?.displayName;
});

onMounted(async () => {
  isLoggedIn.value = await accountService.checkLogin();
  if (isLoggedIn.value) {
    collectionService.loadCards(deck.cards?.map((card) => card.cardId!) ?? []);
  }

  new DeckService().viewDeck(deckId);

  document.addEventListener("click", handleOutsideClick);
});

onUnmounted(() => {
  document.removeEventListener("click", handleOutsideClick);
});

function handleOutsideClick(event: MouseEvent) {
  const target = event.target as HTMLElement;
  if (!target.closest(".actions-dropdown")) {
    showActionsDropdown.value = false;
  }
}

function getCardsInGroup(
  group: DeckCardGroupApiModel,
  cardsList: CardDetailApiModel[],
): CardDetailApiModel[] {
  const groupCards = GetValidCards(cardsList, group.requirements ?? []);
  if (group.sorting && group.sorting.length > 0) {
    return groupCards.sort((a, b) => {
      const aValue = GetCardValue<string>(a, group.sorting![0]!);
      const bValue = GetCardValue<string>(b, group.sorting![0]!);

      if (Number.isNaN(aValue) || Number.isNaN(bValue)) {
        return (aValue as string).localeCompare(bValue as string);
      }
      return Number.parseInt(aValue!) - Number.parseInt(bValue!);
    });
  }
  return groupCards;
}

function getCollectionCount(cardId: number) {
  return (
    collectionService
      .getCards(cardId)
      ?.reduce((sum, card) => sum + (card.amount ?? 0), 0) ?? 0
  );
}

const missingCardsString = computed(() => {
  const parts: string[] = [];
  deck.cards?.forEach((deckCard) => {
    const card = cards.find((c) => c.baseId === deckCard.cardId);
    if (!card) return;
    const needed = deckCard.amount ?? 0;
    const owned = isLoggedIn.value ? getCollectionCount(deckCard.cardId!) : 0;
    const missing = needed - owned;
    if (missing > 0) {
      const set = sets.value.find((s) => s.id === card.setId);
      const setCode = set?.code ?? "";
      parts.push(`${missing} ${card.displayName} [${setCode}]`);
    }
  });
  return parts.join("||");
});

function handleCommentAdded(comment: string) {
  useComments()
    .saveCommentByDeckId(deckId, comment)
    .then((loadedComment) => {
      comments.value.push(loadedComment);
    });
}

function handleCommentDeleted(comment: CommentViewModel) {
  comments.value = comments.value.filter((c) => c.id !== comment.id);
  useComments().deleteDeckComment(comment.id);
}

async function handleDeleteDeck() {
  if (!deck.id || !confirm("Are you sure you want to delete this deck?"))
    return;

  await new DeckService().deleteDeck(deck.id);
  router.push("/account/decks");
}

async function handleCreatePresetFromDeck() {
  if (!deck.id || isCreatingPreset.value) return;

  isCreatingPreset.value = true;
  try {
    await collectionService.createPresetFromDeck(deck.id);
    appToast.success("Preset created from decklist.");
  } catch {
    appToast.error("Could not create a preset from this deck.");
  } finally {
    isCreatingPreset.value = false;
  }
}

function openCardPopup(card: CardDetailApiModel) {
  const cardIndex = groupedDeckCards.value.findIndex(
    (item) => item.baseId === card.baseId,
  );
  if (cardIndex < 0) {
    console.warn(
      `Could not find card with base id ${card.baseId} in grouped deck cards.`,
    );
    return;
  }
  selectedCardIndex.value = cardIndex;
}

function closeCardPopup() {
  selectedCardIndex.value = undefined;
}

function goToPreviousCard() {
  if (!canNavigatePrevious()) return;
  selectedCardIndex.value = selectedCardIndex.value! - 1;
}

function goToNextCard() {
  if (!canNavigateNext()) return;
  selectedCardIndex.value = selectedCardIndex.value! + 1;
}

console.timeEnd("page-render");
</script>

<template>
  <div class="container px-4 py-4 md:px-8">
    <div class="p-4 bg-white rounded">
      <div class="flex">
        <div class="grow">
          <h1 class="text-lg pb-0">{{ deck.name }}</h1>
          <p class="text-xs">By {{ createdBy }}</p>
          <div class="flex align-center gap-2 mt-2">
            <p class="border rounded py-1 px-2">
              {{ deckSettings?.displayName }}
            </p>
            <div id="deck-like-@deck.Id">
              <DeckLike :deck="deck"></DeckLike>
            </div>
          </div>
        </div>

        <div class="shrink-0 flex gap-2 items-start">
          <button
            v-if="isAdmin"
            class="border rounded px-2 py-1 hover:bg-gray-100 disabled:opacity-60 disabled:cursor-not-allowed"
            :disabled="isCreatingPreset"
            @click="handleCreatePresetFromDeck"
          >
            {{ isCreatingPreset ? "Creating..." : "Create preset" }}
          </button>
          <div v-if="isOwner" class="relative actions-dropdown">
            <button
              class="border rounded px-2 py-1 hover:bg-gray-100"
              @click="showActionsDropdown = !showActionsDropdown"
            >
              <PhDotsThree :size="20" />
            </button>
            <div
              v-if="showActionsDropdown"
              class="absolute right-0 mt-1 bg-white border rounded shadow-lg z-10 min-w-32"
            >
              <button
                class="block w-full text-left px-4 py-2 hover:bg-gray-100"
                @click="
                  router.push(`/create-deck?id=${deck.id}`);
                  showActionsDropdown = false;
                "
              >
                Edit
              </button>
              <button
                class="block w-full text-left px-4 py-2 text-red-600 hover:bg-gray-100"
                @click="
                  showActionsDropdown = false;
                  handleDeleteDeck();
                "
              >
                Delete
              </button>
            </div>
          </div>
          <p
            v-if="deck.price"
            class="bg-green-600 px-2.5 py-1.5 rounded-md text-white cursor-pointer"
            onclick="document.querySelector('#buyCardsForm').submit()"
          >
            ${{ deck.price?.marketPrice.toFixed(2) }}
          </p>
        </div>
      </div>

      <div class="flex flex-wrap justify-center gap-4 mt-8">
        <div
          v-for="mainCardWithChildren in mainCardsWithChildren"
          :class="{ 'w-2/5': mainCards.length > 1 }"
          :key="mainCardWithChildren.mainCard.baseId"
          class="md:w-max"
        >
          <img
            class="w-48"
            :src="GetCrop(mainCardWithChildren.mainCard.imageUrl, undefined) ?? '#'"
          />
          <p class="text-center">
            <small>{{ mainCardWithChildren.mainCard.displayName }}</small>
          </p>
          <div
            v-if="mainCardWithChildren.children.length > 0"
            class="mt-2 flex flex-wrap justify-center gap-2"
          >
            <div
              v-for="childCard in mainCardWithChildren.children"
              :key="childCard.baseId"
              class="flex items-center gap-1 rounded-full border border-gray-300 bg-gray-50 px-2 py-1 text-xs"
            >
              <img
                class="h-6 w-6 rounded object-cover"
                :src="GetCrop(childCard.imageUrl, undefined) ?? '#'"
                :alt="childCard.displayName ?? ''"
              />
              <span>{{ childCard.displayName }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="flex flex-col md:flex-row gap-8 mt-8">
        <div v-if="showDecklistSection" class="md:w-2/3 shrink-0">
          <div
            class="flex flex-col md:flex-row align-center justify-between gap-4"
          >
            <h2 class="text-lg">Decklist</h2>
            <div class="flex gap-4 flex-col md:flex-row md:items-center">
              <div class="flex gap-2 items-center">
                <label for="deck-display-mode">Display</label>
                <select
                  id="deck-display-mode"
                  v-model="deckDisplayMode"
                  class="border rounded px-2 py-1"
                >
                  <option value="text">Text</option>
                  <option value="images">Images</option>
                </select>
              </div>
              <div v-if="isLoggedIn" class="flex gap-2 items-center">
                <input
                  type="checkbox"
                  id="compare-collection"
                  v-model="collectionMode"
                />
                <label for="compare-collection">Compare with collection</label>
              </div>
            </div>
          </div>
          <hr class="my-2" />
          <div class="flex md:flex-row flex-col gap-4 text-xs">
            <DeckAction
              v-for="action in deckSettings?.actions"
              :deck="deck"
              :action="action"
              :missing-cards-string="
                action.type === 'DeckMissingCardsExport'
                  ? missingCardsString
                  : undefined
              "
            ></DeckAction>
          </div>
          <template
            v-for="group in deckSettings?.groupings"
            :key="group.header"
          >
            <DeckCardGroup
              :group="group"
              :cards="cards"
              :settings="deckSettings"
              :deck-cards="deck.cards"
              :deck-display-mode="deckDisplayMode"
              :collection-mode="collectionMode"
              :cost-image-url="deckSettings?.costImageUrl"
              :render-cost-on-image="deckSettings?.renderCostOnImage"
              @card-click="openCardPopup"
            />
          </template>
          <div v-if="deck.sideboard && deck.sideboard.length > 0" class="mt-4">
            <hr/>
            <h3 class="text-lg mt-2">Sideboard</h3>
            <template
              v-for="group in deckSettings?.groupings"
              :key="group.header"
            >
              <DeckCardGroup
                :group="group"
                :cards="sideboardCards"
                :settings="deckSettings"
                :deck-cards="deck.sideboard"
                :deck-display-mode="deckDisplayMode"
                :collection-mode="collectionMode"
                :cost-image-url="deckSettings?.costImageUrl"
                :render-cost-on-image="deckSettings?.renderCostOnImage"
                @card-click="openCardPopup"
              />
            </template>
          </div>
        </div>
        <div v-if="deck.description">
          <h2 class="text-lg pb-2">Description</h2>
          <div class="content">
            {{ deck.description }}
          </div>
        </div>
      </div>

      <div class="pt-4">
        <CommentSection
          :comments="comments"
          @add-comment="handleCommentAdded"
          @delete-comment="handleCommentDeleted"
        ></CommentSection>
      </div>
    </div>

    <DeckDetailCardModal
      v-if="selectedCard"
      :selected-card="selectedCard"
      :previous-card-name="previousCardName"
      :next-card-name="nextCardName"
      @close="closeCardPopup"
      @previous="goToPreviousCard"
      @next="goToNextCard"
    />
  </div>
</template>
