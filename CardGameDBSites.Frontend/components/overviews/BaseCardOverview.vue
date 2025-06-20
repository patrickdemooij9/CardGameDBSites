<script setup lang="ts">
import CardService from "~/services/CardService";
import Overview from "./Overview.vue";
import type { OverviewFilterModel } from "./OverviewFilterModel";
import type {
  CardsQueryFilterApiModel,
  CardsQueryFilterClauseApiModel,
  PagedResultCardDetailApiModel,
} from "~/api/default";
import type OverviewRefreshModel from "./OverviewRefreshModel";

const route = useRoute();
const cardService = new CardService();

const props = defineProps<{
  filters: OverviewFilterModel[];
  internalFilters?: CardsQueryFilterClauseApiModel[];
  whiteBackground: boolean;
}>();

const pageNumber = ref(1);
const pageNumberString = route.query["page"];
if (pageNumberString) {
  pageNumber.value = Number.parseInt(pageNumberString as string);
}

const pagedCards = ref<PagedResultCardDetailApiModel>();

await loadData({
  Query: "",
  SelectedFilters: {},
});

//TODO: this needs to be reworked otherwise we keep on resetting stuff
watch(
  () => props.internalFilters,
  async () => {
    pageNumber.value = 1;
    await loadData({
      Query: "",
      SelectedFilters: {},
    });
  }
);

async function loadData(value: OverviewRefreshModel) {
  const filters: CardsQueryFilterClauseApiModel[] = [];
  const searchFilters: CardsQueryFilterApiModel[] = [];
  Object.entries(value.SelectedFilters).forEach(([key, values]) => {
    searchFilters.push({
      alias: key,
      values,
    });
  });
  props.internalFilters?.forEach((filter) => {
    filters.push(filter);
  });

  pagedCards.value = await cardService.query({
    query: value.Query,
    pageNumber: pageNumber.value,
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
    :page="pageNumber"
    :hide-search="false"
    :hide-filters="false"
    :white-background="whiteBackground"
    :filters="filters"
    @reload="loadData"
    @loadLazyFilter="loadLazyFilter"
  >
    <div v-if="pagedCards">
      <slot :cards="pagedCards"> </slot>
      <div
        class="mt-8 row justify-center"
        v-if="(pagedCards.totalPages ?? 0) > 1"
      >
        <div
          class="flex items-center mt-3 border border-gray-400 rounded bg-white overflow-hidden"
        >
          <a
            v-if="pageNumber > 1"
            :href="'?page=' + (pageNumber - 1)"
            class="pointer px-4 py-2 hover:bg-gray-400 no-underline"
            @click.prevent="pageNumber = pageNumber - 1"
            >Previous</a
          >

          <template v-for="i in pageNumber + 4">
            <a
              v-if="i - 2 <= (pagedCards.totalPages ?? 0) && i - 2 > 0"
              :href="'?page=' + (i - 2)"
              :class="[
                i - 2 === pageNumber
                  ? 'bg-main-color text-white'
                  : 'hover:bg-gray-100',
              ]"
              class="pointer px-4 py-2 border-l border-gray-400 no-underline"
              @click.prevent="pageNumber = i - 2"
              >{{ i - 2 }}</a
            >
          </template>

          <a
            v-if="pageNumber < (pagedCards.totalPages ?? 0)"
            :href="'?page=' + (pageNumber + 1)"
            class="pointer px-4 py-2 border-l border-gray-400 hover:bg-gray-100 no-underline"
            @click.prevent="pageNumber = pageNumber + 1"
            >Next</a
          >
        </div>
      </div>
    </div>
  </Overview>
</template>
