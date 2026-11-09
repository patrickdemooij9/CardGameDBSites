<script setup lang="ts">
import { DeckStatus } from "~/api/default";
import type { CardsPropertiesModel } from "~/api/umbraco";
import DeckCardCollection from "../cards/deckCards/DeckCardCollection.vue";
import SectionHeader from "../shared/SectionHeader.vue";
import SegmentedControl from "../shared/SegmentedControl.vue";

type DeckOrder = "recent" | "popular";

const props = defineProps<{
  content: CardsPropertiesModel;
}>();

const orderOptions: { value: DeckOrder; label: string }[] = [
  { value: "recent", label: "Recent" },
  { value: "popular", label: "Most popular" },
];

const order = ref<DeckOrder>(props.content.type === "MostPopular" ? "popular" : "recent");

const decks = useDecks();
const request = computed(() => ({
  typeId: props.content.typeId,
  page: 1,
  take: props.content.amount ?? 4,
  status: DeckStatus.PUBLISHED,
  isLegal: true,
  orderBy: order.value === "popular" ? "popular" : undefined,
}));

const { data } = await useAsyncData(
  () => decks.toUniqueKey(request.value),
  () => decks.query(request.value),
);

const decksUrl = await useSite().getNavigationUrl("/decks");
</script>

<template>
  <SectionHeader :title="content.title ?? 'Decks'">
    <SegmentedControl v-model="order" :options="orderOptions" label="Sort decks" />
  </SectionHeader>
  <DeckCardCollection :decks="data?.items ?? []" :decks-per-row="props.content.amountPerRow ?? 4"></DeckCardCollection>
  <div v-if="decksUrl" class="mt-6 flex justify-center">
    <NuxtLink :to="decksUrl" class="btn-secondary no-underline">All decks</NuxtLink>
  </div>
</template>
