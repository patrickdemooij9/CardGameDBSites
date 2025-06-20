<script setup lang="ts">
import type { PagedResultDeckApiModel } from "~/api/default";
import DeckService from "~/services/DeckService";
import type OverviewRefreshModel from "./OverviewRefreshModel";
import Overview from "./Overview.vue";

const props = defineProps<{
  decksPerRow: number;
  userId?: number;
}>();

const route = useRoute();
const deckService = new DeckService();

const pageNumber = ref(1);
const pageNumberString = route.query["page"];
if (pageNumberString) {
  pageNumber.value = Number.parseInt(pageNumberString as string);
}

const pagedDecks = ref<PagedResultDeckApiModel>();
const gridClass = ref("lg:grid-cols-" + props.decksPerRow);

await loadData({
  Query: "",
  SelectedFilters: {},
});

async function loadData(value: OverviewRefreshModel) {
  pagedDecks.value = await deckService.query({
    page: pageNumber.value,
    take: 30,
    userId: props.userId,
  });
  if (value.LoadedCallback) {
    value.LoadedCallback();
  }
}
</script>

<template>
  <Overview
    :page="pageNumber"
    :hide-search="true"
    :hide-filters="true"
    :white-background="false"
    :filters="[]"
    @reload="loadData"
  >
    <div
      v-if="pagedDecks"
      :class="gridClass"
      class="grid grid-cols-1 auto-rows-fr md:grid-cols-2 gap-4 w-full"
    >
      <slot :decks="pagedDecks"></slot>
      <div
        class="mt-8 row justify-center"
        v-if="(pagedDecks.totalPages ?? 0) > 1"
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
              v-if="i - 2 <= (pagedDecks.totalPages ?? 0) && i - 2 > 0"
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
            v-if="pageNumber < (pagedDecks.totalPages ?? 0)"
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
