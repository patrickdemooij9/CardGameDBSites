<script setup lang="ts">
import type {
  CardDetailApiModel,
  DeckApiModel,
  DeckTypeSettingsApiModel,
  MemberApiModel,
} from "~/api/default";
import { useMembers } from "~/composables/useMembers";
import { useCards } from "~/composables/useCards";
import { useSite } from "~/composables/useSite";
import DeckCard from "./DeckCard.vue";
import { useAccountStore } from "~/stores/AccountStore";
import {
  PhPencilSimple,
  PhTrash,
  PhFolderSimplePlus,
  PhCheck,
} from "@phosphor-icons/vue";

const props = defineProps<{
  decks: DeckApiModel[];
  decksPerRow?: number;
  showOwnerActions?: boolean;
  selectionMode?: boolean;
  selectedDeckIds?: number[];
}>();

const emit = defineEmits<{
  edit: [deckId: number];
  delete: [deckId: number];
  move: [deckId: number];
  toggleSelect: [deckId: number];
}>();

const selectedSet = computed(() => new Set(props.selectedDeckIds ?? []));

const { loadMembersByIds } = useMembers();
const { loadCardsByIds } = useCards();
const { getDeckTypeSettings } = useSite();
const accountStore = useAccountStore();

const members = ref<Record<number, MemberApiModel>>({});
const cards = ref<Record<number, CardDetailApiModel>>({});
const deckSettings = ref<Record<number, DeckTypeSettingsApiModel>>({});
const deckProgress = ref<Record<number, number>>({});
const isLoggedIn = ref(false);

async function loadDecksData(decks: DeckApiModel[]) {
  const uniqueTypeIds = [
    ...new Set(decks.map((d) => d.typeId).filter(Boolean) as number[]),
  ];

  if (decks.length > 0) {
    const uniqueCreatorIds = [
      ...new Set(
        decks.map((d) => d.createdBy).filter((id): id is number => !!id),
      ),
    ];
    if (uniqueCreatorIds.length > 0) {
      const loadedMembers = await loadMembersByIds(uniqueCreatorIds);
      loadedMembers.forEach((m) => {
        members.value[m.id] = m;
      });
    }

    const allCardIds = decks.flatMap(
      (d) => d.cards?.map((c) => c.cardId!).filter(Boolean) ?? [],
    );
    const uniqueCardIds = [...new Set(allCardIds)];
    if (uniqueCardIds.length > 0) {
      const loadedCards = await loadCardsByIds(uniqueCardIds);
      loadedCards.forEach((c) => {
        if (c.baseId) {
          cards.value[c.baseId] = c;
        }
      });
    }

    const loadedSettings = await Promise.all(
      uniqueTypeIds.map(async (typeId) => ({
        typeId,
        settings: await getDeckTypeSettings(typeId),
      })),
    );
    loadedSettings.forEach(({ typeId, settings }) => {
      if (settings) {
        deckSettings.value[typeId] = settings;
      }
    });

    if (isLoggedIn.value) {
      const collectionService = useCollection();
      const deckIds = decks.map((d) => d.id!);
      collectionService.loadDecksProgress(deckIds).then((progress) => {
        progress.forEach((p) => {
          if (p.deckId) {
            deckProgress.value[p.deckId] = p.progress ?? 0;
          }
        });
      });
    }
  }
}

onMounted(async () => {
  await loadDecksData(props.decks);
  isLoggedIn.value = await accountStore.checkLogin();
});

watch([() => props.decks, isLoggedIn], async ([newDecks]) => {
  await loadDecksData(newDecks);
});

const gridClass = computed(() => `lg:grid-cols-${props.decksPerRow ?? 4}`);
</script>

<template>
  <div
    :class="gridClass"
    class="grid grid-cols-1 auto-rows-fr md:grid-cols-2 gap-4 w-full"
  >
    <div
      v-for="deck in decks"
      :key="deck.id"
      class="relative flex flex-col rounded"
      :class="{
        'ring-2 ring-main-color ring-offset-2': selectedSet.has(deck.id!),
      }"
    >
      <DeckCard
        :deck="deck"
        :member="members[deck.createdBy ?? 0]"
        :cards="cards"
        :settings="deckSettings[deck.typeId ?? 0]"
        :progress="deckProgress[deck.id!]"
      ></DeckCard>

      <button
        v-if="selectionMode"
        type="button"
        class="absolute inset-0 z-20 cursor-pointer"
        :aria-pressed="selectedSet.has(deck.id!)"
        :aria-label="`Select ${deck.name}`"
        @click.prevent="emit('toggleSelect', deck.id!)"
      >
        <span
          class="absolute top-2 left-2 flex h-6 w-6 items-center justify-center rounded border bg-white"
          :class="
            selectedSet.has(deck.id!)
              ? 'bg-main-color border-main-color text-white'
              : 'border-gray-400 text-transparent'
          "
        >
          <PhCheck :size="16" weight="bold" />
        </span>
      </button>

      <div
        v-else-if="showOwnerActions"
        class="mt-2 flex justify-end gap-2"
      >
        <button
          type="button"
          class="flex items-center gap-1 rounded border border-gray-300 bg-white px-2 py-1 text-xs hover:border-main-color"
          @click.stop.prevent="emit('move', deck.id!)"
        >
          <PhFolderSimplePlus :size="16" /> Move
        </button>
        <button
          type="button"
          class="flex items-center gap-1 rounded border border-gray-300 bg-white px-2 py-1 text-xs hover:border-main-color"
          @click.stop.prevent="emit('edit', deck.id!)"
        >
          <PhPencilSimple :size="16" /> Edit
        </button>
        <button
          type="button"
          class="flex items-center gap-1 rounded border border-red-300 bg-white px-2 py-1 text-xs text-red-600 hover:border-red-500"
          @click.stop.prevent="emit('delete', deck.id!)"
        >
          <PhTrash :size="16" /> Delete
        </button>
      </div>
    </div>
  </div>
</template>
