<script setup lang="ts">
import {
  DeckStatus,
  type DeckQueryPostModel,
} from "~/api/default";
import DeckService from "~/services/DeckService";
import DeckCardCollection from "~/components/cards/deckCards/DeckCardCollection.vue";
import type { OverviewSortModel } from "./OverviewSortModel";
import Overview from "./Overview.vue";
import {
  OverviewFilterType,
  type OverviewFilterModel,
} from "./OverviewFilterModel";

const props = defineProps<{
  decksPerRow: number;
  typeId?: number;
  userId?: number;
  sortings?: OverviewSortModel[];
}>();

const deckService = new DeckService();

const defaultSortings = ref<OverviewSortModel[]>([
  { Name: "Popular", Value: "popular" },
  { Name: "Newest", Value: "newest" },
]);

const effectiveSortings = computed(() => props.sortings ?? defaultSortings.value);

const filters: OverviewFilterModel[] = [
  {
    Alias: "card",
    DisplayName: "Contains card",
    Type: OverviewFilterType.TEXT_INPUT,
    Items: [],
    AutoFillValues: false,
  },
  {
    Alias: "fromDate",
    DisplayName: "Created start date",
    Type: OverviewFilterType.DATE,
    Items: [],
    AutoFillValues: false,
  },
  {
    Alias: "toDate",
    DisplayName: "Created end date",
    Type: OverviewFilterType.DATE,
    Items: [],
    AutoFillValues: false,
  },
];

const overviewState = useOverviewState(
  toRef(filters),
  toRef(effectiveSortings),
  undefined,
  {
    enableQueryStringSync: true,
  },
);
const queryModel = computed<DeckQueryPostModel>(() => {
  let dateFrom: string | undefined;
  let dateTo: string | undefined;

  overviewState.state.selectedFilters.forEach((values, filter) => {
    if (filter.Alias === "fromDate") {
      if (values[0]) dateFrom = values[0];
    } else if (filter.Alias === "toDate") {
      if (values[0]) dateTo = values[0];
    }
  });

  const cardFilter = overviewState.state.selectedFilters.get(filters[0]!);
  const cardIds =
    cardFilter && cardFilter.length > 0
      ? cardFilter.map((v) => parseInt(v))
      : undefined;

  return {
    page: page.value,
    take: 20,
    userId: props.userId,
    typeId: props.typeId,
    status: props.userId ? DeckStatus.NONE : DeckStatus.PUBLISHED,
    dateFrom: dateFrom || null,
    dateTo: dateTo || null,
    orderBy: overviewState.state.sortBy,
    cards: cardIds,
  };
});

const page = computed(() => overviewState.state.page);

const id = useId();
const { data: pagedDecks, pending } = await useAsyncData(
  `deck-overview-${id}`,
  () => deckService.query(queryModel.value),
  { watch: [queryModel] },
);

onMounted(() => {
  const accountStore = useAccountStore();
  accountStore.checkLogin().then((isLoggedIn) => {
    if (isLoggedIn) {
      defaultSortings.value.push({ Name: "Collection", Value: "collection" });
    }
  });
});
</script>

<template>
  <Overview
    :overview-state="overviewState"
    :hide-search="true"
    :hide-filters="false"
    :hide-inline-filters="false"
    :white-background="false"
    :enable-query-string-sync="true"
    :sortings="effectiveSortings"
    :filters="filters"
    :is-loading="pending"
    entity-name="decks"
    ref="overview"
  >
    <div v-if="pagedDecks">
      <slot :decks="pagedDecks">
        <DeckCardCollection
          :decks="pagedDecks.items ?? []"
          :decks-per-row="props.decksPerRow"
        ></DeckCardCollection>
      </slot>

      <div
        class="mt-8 row justify-center"
        v-if="(pagedDecks.totalPages ?? 0) > 1"
      >
        <div
          class="flex items-center mt-3 border border-gray-400 rounded bg-white overflow-hidden"
        >
          <a
            v-if="page > 1"
            :href="'?page=' + (page - 1)"
            class="pointer px-4 py-2 hover:bg-gray-400 no-underline"
            @click.prevent="overviewState.setPage(page - 1)"
            >Previous</a
          >

          <template v-for="i in page + 4">
            <a
              v-if="i - 2 <= (pagedDecks.totalPages ?? 0) && i - 2 > 0"
              :href="'?page=' + (i - 2)"
              :class="[
                i - 2 === page
                  ? 'bg-main-color text-white'
                  : 'hover:bg-gray-100',
              ]"
              class="pointer px-4 py-2 border-l border-gray-400 no-underline"
              @click.prevent="overviewState.setPage(i - 2)"
              >{{ i - 2 }}</a
            >
          </template>

          <a
            v-if="page < (pagedDecks.totalPages ?? 0)"
            :href="'?page=' + (page + 1)"
            class="pointer px-4 py-2 border-l border-gray-400 hover:bg-gray-100 no-underline"
            @click.prevent="overviewState.setPage(page + 1)"
            >Next</a
          >
        </div>
      </div>
    </div>
  </Overview>
</template>
