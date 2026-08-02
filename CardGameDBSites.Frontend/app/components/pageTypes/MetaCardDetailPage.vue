<script setup lang="ts">
import type { DeckApiModel } from "~/api/default";
import type { MetaCardDetailContentModel } from "~/api/umbraco";
import CmsImage from "~/components/shared/CmsImage.vue";
import DeckCardCollection from "~/components/cards/deckCards/DeckCardCollection.vue";
import TournamentService, {
  type PeriodApiModel,
} from "~/services/TournamentService";
import MetaService, { type MetaCardStatApiModel } from "~/services/MetaService";
import DeckService from "~/services/DeckService";

const props = defineProps<{
  content: MetaCardDetailContentModel;
}>();

// The meta feature currently supports a single format per site (see MetaPage.vue).
const DEFAULT_FORMAT_ID = 1;
const DECK_COUNT = 6;

const route = useRoute();
const metaService = new MetaService();
const tournamentService = new TournamentService();
const deckService = new DeckService();

// The card this page is about is derived from the URL (there is no per-card content node).
const slugParam = route.params.slug;
const requestedPath =
  "/" + (Array.isArray(slugParam) ? slugParam.join("/") : (slugParam ?? ""));

const { data: resolvedCard } = await useAsyncData(
  `meta-detail-card:${requestedPath}`,
  () => metaService.resolveCard(requestedPath).catch(() => null)
);
const leader = resolvedCard.value;
if (!leader) {
  throw createError({ statusCode: 404, statusMessage: "Resource Not Found" });
}

// Per-card SEO — this page targets searches like "<leader> swu", so override the shared
// template node's meta with card-specific values.
useHead({
  title: `${leader.displayName} - Star Wars Unlimited Meta & Best Decks`,
  meta: [
    {
      name: "description",
      content: `Usage, winrate and the best tournament and community decks for ${leader.displayName} in Star Wars Unlimited.`,
    },
  ],
});

const { data: periods } = await useAsyncData(
  `meta-detail-periods:${DEFAULT_FORMAT_ID}`,
  () => tournamentService.getPeriods(DEFAULT_FORMAT_ID).catch(() => []),
  { default: () => [] as PeriodApiModel[] }
);
const periodId =
  periods.value.find((p) => p.isCurrent)?.id ?? periods.value[0]?.id ?? null;

// Usage/winrate from the current period's snapshot.
const { data: stat } = await useAsyncData(
  `meta-detail-stat:${leader.baseId}`,
  () =>
    periodId == null
      ? Promise.resolve(null)
      : metaService
          .getCardStats(periodId, [leader.baseId])
          .then((s) => s[0] ?? null)
          .catch(() => null),
  { default: () => null as MetaCardStatApiModel | null }
);

// Top meta decks = tournament decks; Most popular decks = community (non-tournament) decks.
const { data: topMetaDecks } = await useAsyncData(
  `meta-detail-top-decks:${leader.baseId}`,
  () =>
    periodId == null
      ? Promise.resolve([])
      : metaService.getTopDecks(periodId, leader.baseId, DECK_COUNT).catch(() => []),
  { default: () => [] as DeckApiModel[] }
);

const { data: popularDecks } = await useAsyncData(
  `meta-detail-popular-decks:${leader.baseId}`,
  () =>
    deckService
      .query({ page: 1, take: DECK_COUNT, cards: [leader.baseId], orderBy: "popular" })
      .then((r) => r.items ?? [])
      .catch(() => []),
  { default: () => [] as DeckApiModel[] }
);

function formatPercent(value: number | undefined): string {
  if (value == null || value <= 0) return "0%";
  if (value < 1) return "<1%";
  return `${Math.round(value)}%`;
}
</script>

<template>
  <div class="bg-gray-100">
    <!-- Card summary -->
    <section class="container px-4 md:px-8 py-8">
      <div class="bg-white rounded-xl shadow-md p-6 flex flex-col sm:flex-row gap-6">
        <NuxtLink :href="leader.urlSegment" class="no-underline sm:w-48 shrink-0">
          <CmsImage
            :src="leader.imageUrl"
            :alt="leader.displayName"
            loading="eager"
            class="w-full rounded-lg object-cover"
          >
            <template #fallback>
              <div class="missing-card-image aspect-[2/3]">
                <h2>{{ leader.displayName }}</h2>
                <p>No image yet</p>
              </div>
            </template>
          </CmsImage>
        </NuxtLink>
        <div class="flex-1">
          <h1 class="mb-2">{{ leader.displayName }}</h1>
          <div class="flex flex-wrap gap-6 my-4">
            <div>
              <div class="text-sm text-gray-500 uppercase tracking-wide font-semibold">
                Usage
              </div>
              <div class="text-2xl font-bold">
                {{ formatPercent(stat?.usagePercentage) }}
              </div>
            </div>
            <div>
              <div class="text-sm text-gray-500 uppercase tracking-wide font-semibold">
                Winrate
              </div>
              <div class="text-2xl font-bold">
                {{ formatPercent(stat?.winratePercentage) }}
              </div>
            </div>
          </div>
          <NuxtLink
            :href="leader.urlSegment"
            class="no-underline inline-block bg-gray-900 text-white hover:bg-gray-700 font-semibold px-5 py-2 rounded transition-colors"
          >
            View card details
          </NuxtLink>
        </div>
      </div>
    </section>

    <!-- Top meta decks (tournament) -->
    <section class="container px-4 md:px-8 pb-8">
      <h2 class="mb-6">Top meta decks</h2>
      <DeckCardCollection
        v-if="topMetaDecks.length > 0"
        :decks="topMetaDecks"
        :decks-per-row="3"
      />
      <p v-else class="text-gray-500">
        No tournament decks with this leader yet.
      </p>
    </section>

    <!-- Most popular decks (community) -->
    <section class="container px-4 md:px-8 pb-12">
      <h2 class="mb-6">Most popular decks</h2>
      <DeckCardCollection
        v-if="popularDecks.length > 0"
        :decks="popularDecks"
        :decks-per-row="3"
      />
      <p v-else class="text-gray-500">
        No community decks with this leader yet.
      </p>
    </section>
  </div>
</template>
