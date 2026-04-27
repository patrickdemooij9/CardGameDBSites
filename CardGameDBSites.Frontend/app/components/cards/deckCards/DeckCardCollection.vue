<script setup lang="ts">
import type { CardDetailApiModel, DeckApiModel, DeckTypeSettingsApiModel, MemberApiModel } from "~/api/default";
import { useMembers } from "~/composables/useMembers";
import { useCards } from "~/composables/useCards";
import { useSite } from "~/composables/useSite";
import DeckCard from "./DeckCard.vue";
import { useAccountStore } from "~/stores/AccountStore";

const props = defineProps<{
  decks: DeckApiModel[];
  decksPerRow?: number;
}>();

const { loadMembersByIds } = useMembers();
const { loadCardsByIds } = useCards();
const { getDeckTypeSettings } = useSite();

const members = ref<Record<number, MemberApiModel>>({});
const cards = ref<Record<number, CardDetailApiModel>>({});
const deckSettings = ref<Record<number, DeckTypeSettingsApiModel>>({});
const deckProgress = ref<Record<number, number>>({});

const uniqueTypeIds = [...new Set(props.decks.map(d => d.typeId).filter(Boolean) as number[])];

if (props.decks.length > 0) {
  const uniqueCreatorIds = [...new Set(props.decks.map(d => d.createdBy).filter((id): id is number => !!id))];
  if (uniqueCreatorIds.length > 0) {
    const loadedMembers = await loadMembersByIds(uniqueCreatorIds);
    loadedMembers.forEach(m => {
      members.value[m.id] = m;
    });
  }

  const allCardIds = props.decks.flatMap(d => d.cards?.map(c => c.cardId!).filter(Boolean) ?? []);
  const uniqueCardIds = [...new Set(allCardIds)];
  if (uniqueCardIds.length > 0) {
    const loadedCards = await loadCardsByIds(uniqueCardIds);
    loadedCards.forEach(c => {
      if (c.baseId) {
        cards.value[c.baseId] = c;
      }
    });
  }

  for (const typeId of uniqueTypeIds) {
    const settings = await getDeckTypeSettings(typeId);
    if (settings) {
      deckSettings.value[typeId] = settings;
    }
  }
}

onMounted(() => {
  if (useAccountStore().isLoggedIn) {
    const collectionService = useCollection();
    const deckIds = props.decks.map(d => d.id!);
    collectionService.loadDecksProgress(deckIds).then(progress => {
      progress.forEach(p => {
        if (p.deckId) {
          deckProgress.value[p.deckId] = p.progress ?? 0;
        }
      });
    });
  }
});

const gridClass = computed(() => `lg:grid-cols-${props.decksPerRow ?? 4}`);
</script>

<template>
  <div :class="gridClass" class="grid grid-cols-1 auto-rows-fr md:grid-cols-2 gap-4 w-full">
    <DeckCard 
      v-for="deck in decks" 
      :deck="deck" 
      :member="members[deck.createdBy ?? 0]" 
      :cards="cards"
      :settings="deckSettings[deck.typeId ?? 0]"
      :progress="deckProgress[deck.id!]"
    ></DeckCard>
  </div>
</template>
