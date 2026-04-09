<script setup lang="ts">
import { DeckStatus, type PagedResultDeckApiModel } from "~/api/default";
import DeckService from "~/services/DeckService";
import DeckCardCollection from "~/components/cards/deckCards/DeckCardCollection.vue";
import type OverviewRefreshModel from "./OverviewRefreshModel";
import Overview from "./Overview.vue";

const props = defineProps<{
  decksPerRow: number;
  typeId?: number;
  userId?: number;
}>();

const deckService = new DeckService();
const route = useRoute();

const initialPage = route.query["page"]
  ? Number.parseInt(route.query["page"] as string)
  : 1;

const { data: ssrData } = await useAsyncData(
  `deck-overview-${props.typeId ?? 0}-${props.userId ?? 0}-${initialPage}`,
  () =>
    deckService.query({
      page: initialPage,
      take: 20,
      userId: props.userId,
      typeId: props.typeId,
      status: props.userId ? DeckStatus.NONE : DeckStatus.PUBLISHED,
    })
);

const pagedDecks = ref<PagedResultDeckApiModel | undefined>(
  ssrData.value ?? undefined
);
const overview = ref<InstanceType<typeof Overview>>();

let initialLoadHandled = false;

async function loadData(value: OverviewRefreshModel) {
  // On the first call, if we already have SSR data for the same page, skip the
  // re-fetch. This prevents a hydration mismatch caused by the server having
  // data while the client starts empty.
  if (
    !initialLoadHandled &&
    value.PageNumber === initialPage &&
    pagedDecks.value
  ) {
    initialLoadHandled = true;
    if (value.LoadedCallback) {
      value.LoadedCallback();
    }
    return;
  }

  initialLoadHandled = true;

  pagedDecks.value = await deckService.query({
    page: value.PageNumber,
    take: 20,
    userId: props.userId,
    typeId: props.typeId,
    status: props.userId ? DeckStatus.NONE : DeckStatus.PUBLISHED,
  });

  if (value.LoadedCallback) {
    value.LoadedCallback();
  }
}
</script>

<template>
  <Overview
    :hide-search="true"
    :hide-filters="true"
    :white-background="false"
    :enable-query-string-sync="true"
    :filters="[]"
    @reload="loadData"
    ref="overview"
  >
    <div v-if="pagedDecks">
      <slot :decks="pagedDecks">
        <DeckCardCollection :decks="pagedDecks.items ?? []" :decks-per-row="props.decksPerRow"></DeckCardCollection>
      </slot>

      <div
        class="mt-8 row justify-center"
        v-if="(pagedDecks.totalPages ?? 0) > 1"
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
              v-if="i - 2 <= (pagedDecks.totalPages ?? 0) && i - 2 > 0"
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
            v-if="overview.getPage() < (pagedDecks.totalPages ?? 0)"
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
