<script setup lang="ts">
import CardService from "~/services/CardService";
import Overview from "./Overview.vue";
import type { OverviewFilterModel } from "./OverviewFilterModel";
import {
  CardSearchFilterClauseType,
  type CardsQueryFilterApiModel,
  type CardsQueryFilterClauseApiModel,
  type PagedResultCardDetailApiModel,
} from "~/api/default";
import type OverviewRefreshModel from "./OverviewRefreshModel";

const cardService = new CardService();

const props = defineProps<{
  filters: OverviewFilterModel[];
  internalFilters?: CardsQueryFilterClauseApiModel[];
  whiteBackground: boolean;
}>();

const pagedCards = ref<PagedResultCardDetailApiModel>();
const overview = ref<InstanceType<typeof Overview>>();

//TODO: this needs to be reworked otherwise we keep on resetting stuff
watch(
  () => props.internalFilters,
  (newVal, oldVal) => {
    // Compare JSON stringified values for a shallow equality check
    if (JSON.stringify(newVal) !== JSON.stringify(oldVal)) {
      overview.value?.setPage(1, true);
    }
  },
  { deep: true }
);

async function loadData(value: OverviewRefreshModel) {
  const filters: CardsQueryFilterClauseApiModel[] = [];
  Object.entries(value.SelectedFilters).forEach(([key, values]) => {
    filters.push({
      clauseType: CardSearchFilterClauseType.AND,
      filters: [{
        alias: key,
        values: values,
      }]
    });
  });
  props.internalFilters?.forEach((filter) => {
    filters.push(filter);
  });

  pagedCards.value = await cardService.query({
    query: value.Query,
    pageNumber: value.PageNumber,
    pageSize: 30,
    filterClauses: filters,
    variantTypeId: 0,
  });
  if (value.LoadedCallback) {
    value.LoadedCallback();
  }
}

async function loadLazyFilter(filter: OverviewFilterModel) {
  const values = await cardService.getValues(filter.Alias);
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
    :hide-search="false"
    :hide-filters="false"
    :white-background="whiteBackground"
    :filters="filters"
    @reload="loadData"
    @loadLazyFilter="loadLazyFilter"
    ref="overview"
  >
    <div v-if="pagedCards">
      <slot :cards="pagedCards"> </slot>
      <div
        class="mt-8 row justify-center"
        v-if="(pagedCards.totalPages ?? 0) > 1"
      >
        <div
        v-if="overview"
          class="flex items-center mt-3 border border-gray-400 rounded bg-white overflow-hidden"
        >
          <a
            v-if="overview.getPage() > 1"
            :href="'?page=' + (overview.getPage() - 1)"
            class="pointer px-4 py-2 hover:bg-gray-400 no-underline"
            @click.prevent="overview.setPage(overview.getPage() - 1)"
            >Previous</a
          >

          <template v-for="i in overview.getPage() + 4">
            <a
              v-if="i - 2 <= (pagedCards.totalPages ?? 0) && i - 2 > 0"
              :href="'?page=' + (i - 2)"
              :class="[
                i - 2 === overview.getPage()
                  ? 'bg-main-color text-white'
                  : 'hover:bg-gray-100',
              ]"
              class="pointer px-4 py-2 border-l border-gray-400 no-underline"
              @click.prevent="overview.setPage(i - 2)"
              >{{ i - 2 }}</a
            >
          </template>

          <a
            v-if="overview.getPage() < (pagedCards.totalPages ?? 0)"
            :href="'?page=' + (overview.getPage() + 1)"
            class="pointer px-4 py-2 border-l border-gray-400 hover:bg-gray-100 no-underline"
            @click.prevent="overview.setPage(overview.getPage() + 1)"
            >Next</a
          >
        </div>
      </div>
    </div>
  </Overview>
</template>
