<script setup lang="ts">
import Overview from "./Overview.vue";
import type { OverviewFilterModel } from "./OverviewFilterModel";
import type { OverviewSortModel } from "./OverviewSortModel";
import {
  CardSearchCollectionMode,
  CardSearchFilterClauseType,
  type CardsQueryFilterClauseApiModel,
  type CardsQueryPostApiModel,
  type PagedResultCardDetailApiModel,
} from "~/api/default";

const props = defineProps<{
  filters: OverviewFilterModel[];
  sortings?: OverviewSortModel[];
  internalFilters?: CardsQueryFilterClauseApiModel[];
  whiteBackground: boolean;
  enableQueryStringSync: boolean;
  collectionMode?: CardSearchCollectionMode;
  hideReprintedCards?: boolean;
  legalForDeckTypeId?: number;
  availableViews?: string[];
  pageSize?: number;
  variantTypeIds?: number[];
}>();

const emit = defineEmits<{
  (e: "reloaded", value: PagedResultCardDetailApiModel): void;
}>();

const reactiveFilters = reactive(props.filters);
const filtersRef = computed(() => reactiveFilters);

const overviewState = useOverviewState(
  filtersRef,
  toRef(props, "sortings"),
  toRef(props, "availableViews"),
  {
    enableQueryStringSync: props.enableQueryStringSync,
  },
);
const queryModel = computed<CardsQueryPostApiModel>(() => {
  const filters: CardsQueryFilterClauseApiModel[] = [];
  overviewState.state.selectedFilters.forEach((values, key) => {
    if (values.length === 0) {
      return;
    }
    if (key.ToFiltersHandler) {
      filters.push(...key.ToFiltersHandler(values));
      return;
    }

    // "all" mode: the card must have every selected value. Since values within a
    // single clause are OR'd server-side, emit one AND clause per value so they
    // combine as AND. "any" mode keeps the single multi-value (OR) clause.
    if (overviewState.getMatchMode(key) === "all" && values.length > 1) {
      values.forEach((value) => {
        filters.push({
          clauseType: CardSearchFilterClauseType.AND,
          filters: [{ alias: key.Alias, values: [value] }],
        });
      });
      return;
    }

    filters.push({
      clauseType: CardSearchFilterClauseType.AND,
      filters: [
        {
          alias: key.Alias,
          values: values,
        },
      ],
    });
  });
  props.internalFilters?.forEach((filter) => {
    filters.push(filter);
  });

  return {
    query: overviewState.state.search,
    pageNumber: page.value,
    pageSize: props.pageSize ?? 30,
    filterClauses: filters,
    variantTypeIds: props.variantTypeIds ?? [0],
    collectionMode: props.collectionMode ?? CardSearchCollectionMode.IGNORE,
    sortBy: overviewState.state.sortBy,
    includeReprintedCards: props.hideReprintedCards ? false : undefined,
    legalForDeckTypeId: props.legalForDeckTypeId,
  };
});

const page = computed(() => overviewState.state.page);

const queryKey = computed(() => JSON.stringify(queryModel.value));

//TODO: this needs to be reworked otherwise we keep on resetting stuff
watch(
  () => props.internalFilters,
  (newVal, oldVal) => {
    // Compare JSON stringified values for a shallow equality check
    if (JSON.stringify(newVal) !== JSON.stringify(oldVal)) {
      overviewState.setPage(1);
    }
  },
  { deep: true },
);

watch(
  () => props.collectionMode,
  () => {
    overviewState.setPage(1);
  },
);

const id = useId();
const { data: pagedCards, pending } = await useAsyncData(
  `card-overview-${id}`,
  () => useCards().queryCards(queryModel.value),
  { watch: [queryKey] },
);

watch(
  pagedCards,
  (newValue) => {
    if (newValue) {
      emit("reloaded", newValue);
    }
  },
  { immediate: true },
);

async function loadLazyFilter(filter: OverviewFilterModel) {
  const values = await useCards().getAbilityValues(filter.Alias);
  filter.Items = values.map((item) => {
    return {
      DisplayName: item,
      Value: item,
    };
  });
}
</script>

<template>
  <Overview
    :overview-state="overviewState"
    :hide-search="false"
    :hide-filters="false"
    :white-background="whiteBackground"
    :filters="reactiveFilters"
    :sortings="sortings"
    :available-views="availableViews"
    :is-loading="pending"
    @loadLazyFilter="loadLazyFilter"
    ref="overview"
    v-slot="{ viewMode }"
  >
    <div v-if="pagedCards">
      <slot :cards="pagedCards" :viewMode="viewMode"> </slot>
      <div
        class="mt-8 row justify-center"
        v-if="(pagedCards.totalPages ?? 0) > 1"
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
              v-if="i - 2 <= (pagedCards.totalPages ?? 0) && i - 2 > 0"
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
            v-if="page < (pagedCards.totalPages ?? 0)"
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
