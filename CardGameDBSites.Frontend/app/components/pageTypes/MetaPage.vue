<script setup lang="ts">
import type { TournamentSummaryApiModel } from "~/api/default";
import TournamentService, {
  type MetaWinningDeckApiModel,
  type PeriodApiModel,
} from "~/services/TournamentService";
import MetaService, {
  type MetaTierListApiModel,
  type MetaTierName,
} from "~/services/MetaService";
import { ParseToHumanReadableText } from "~/helpers/DateHelper";
import MetaTierHeader from "~/components/meta/MetaTierHeader.vue";
import MetaTierTable from "~/components/meta/MetaTierTable.vue";
import FaqSection from "~/components/shared/FaqSection.vue";
import { buildFaqSchema, type FaqEntry } from "~/components/shared/faq";
import { formatNumber, formatMonthYear } from "~/components/meta/format";
import type { FrequentlyAskedQuestionPropertiesModel, MetaPageContentModel } from "~/api/umbraco";

const props = defineProps<{
  content: MetaPageContentModel
}>();

const DEFAULT_FORMAT_ID = 1;
const TIER_ORDER: MetaTierName[] = ["S", "A", "B", "C", "D", "Unranked"];

const route = useRoute();
const tournamentService = new TournamentService();
const metaService = new MetaService();

const { data: periods } = await useAsyncData(
  `meta-periods:${DEFAULT_FORMAT_ID}`,
  () => tournamentService.getPeriods(DEFAULT_FORMAT_ID).catch(() => []),
  { default: () => [] as PeriodApiModel[] }
);

// In the query string so the choice is linkable and useAsyncData can watch it.
const selectedPeriodId = computed<number | undefined>(() => {
  const fromQuery = Number(route.query.period);
  if (fromQuery && periods.value.some((p) => p.id === fromQuery)) return fromQuery;
  return periods.value.find((p) => p.isCurrent)?.id ?? periods.value[0]?.id;
});

const defaultPeriodId = computed(
  () => periods.value.find((p) => p.isCurrent)?.id ?? periods.value[0]?.id
);
const isDefaultPeriod = computed(() => selectedPeriodId.value === defaultPeriodId.value);

// Each call catches separately so one bad endpoint can't blank the page.
const { data: tierList, error: tierError } = await useAsyncData<MetaTierListApiModel | null>(
  () => `meta-tier-list:${selectedPeriodId.value ?? "default"}`,
  () => metaService.getTierList(selectedPeriodId.value),
  { watch: [selectedPeriodId], default: () => null }
);

const { data: recentTournaments } = await useAsyncData(
  () => `meta-recent:${selectedPeriodId.value ?? "default"}`,
  () =>
    selectedPeriodId.value == null
      ? Promise.resolve([])
      : tournamentService.getRecent(selectedPeriodId.value, 6).catch(() => []),
  { watch: [selectedPeriodId], default: () => [] as TournamentSummaryApiModel[] }
);

const { data: recentWinners } = await useAsyncData(
  () => `meta-recent-winners:${selectedPeriodId.value ?? "default"}`,
  () =>
    selectedPeriodId.value == null
      ? Promise.resolve([])
      : tournamentService.getRecentWinners(selectedPeriodId.value, 3, 1, 0).catch(() => []),
  { watch: [selectedPeriodId], default: () => [] as MetaWinningDeckApiModel[] }
);

const leadersByTier = computed(() => {
  const grouped = new Map<MetaTierName, MetaTierListApiModel["leaders"]>();
  for (const tier of TIER_ORDER) grouped.set(tier, []);
  for (const leader of tierList.value?.leaders ?? []) {
    grouped.get(leader.tier)?.push(leader);
  }
  return grouped;
});

const hasRankedLeaders = computed(() =>
  (tierList.value?.leaders ?? []).some((l) => l.tier !== "Unranked")
);

const faqEntries = computed(() => props.content.properties?.faqItems?.items?.map<FaqEntry>((item) => {
  const faq = item.content as FrequentlyAskedQuestionPropertiesModel;
  return {
    question: faq.question!,
    answer: faq.answer!
  }
}) ?? []);

// Month comes from the newest event, not the clock, so the title can't overstate freshness.
const updatedLabel = computed(() => formatMonthYear(tierList.value?.lastUpdatedUtc));

const periodName = computed(() => tierList.value?.periodName?.trim() ?? "");

const heading = computed(() => {
  const parts = ["Star Wars Unlimited Meta Tier List"];
  if (periodName.value) parts.push(periodName.value);
  if (updatedLabel.value) parts.push(updatedLabel.value);
  return parts.length > 1 ? `${parts[0]} — ${parts.slice(1).join(", ")}` : parts[0]!;
});

const metaDescription = computed(() => {
  const list = tierList.value;
  if (!list || list.totalDecks === 0) {
    return "The best leaders in Star Wars Unlimited, ranked S through D from tournament results, with win rates, meta share and weekly movement.";
  }
  return (
    `The best Star Wars Unlimited leaders${periodName.value ? ` in ${periodName.value}` : ""}, ranked S through D from ` +
    `${formatNumber(list.totalDecks)} tournament decks across ${list.totalEvents} events. ` +
    `Win rates, meta share and weekly movement.`
  );
});

// Resolved during setup: useRequestURL needs the Nuxt instance, and the computeds below run lazily
// during head resolution when it is gone.
const origin = useRequestURL().origin;
const canonicalUrl = computed(() => `${origin}${route.path}`);

useHead(() => ({
  title: heading.value,
  meta: [
    { name: "description", content: metaDescription.value },
    // A non-default period is a near-duplicate of the canonical page.
    ...(isDefaultPeriod.value ? [] : [{ name: "robots", content: "noindex,follow" }]),
  ],
  link: [{ rel: "canonical", href: canonicalUrl.value }],
}));

const structuredData = computed(() => {
  const list = tierList.value;
  if (!list) return null;

  const ranked = list.leaders.filter((l) => l.tier !== "Unranked");

  const graph: Record<string, unknown>[] = [
    {
      "@type": "WebPage",
      "@id": `${canonicalUrl.value}#webpage`,
      url: canonicalUrl.value,
      name: heading.value,
      description: metaDescription.value,
      ...(list.lastUpdatedUtc ? { dateModified: list.lastUpdatedUtc } : {}),
      mainEntity: { "@id": `${canonicalUrl.value}#tierlist` },
      ...(faqEntries.value.length > 0
        ? { hasPart: { "@id": `${canonicalUrl.value}#faq` } }
        : {}),
    },
    {
      "@type": "ItemList",
      "@id": `${canonicalUrl.value}#tierlist`,
      name: heading.value,
      numberOfItems: ranked.length,
      itemListOrder: "https://schema.org/ItemListOrderDescending",
      itemListElement: ranked.map((leader, index) => ({
        "@type": "ListItem",
        position: index + 1,
        name: leader.name,
        ...(leader.metaUrl ? { url: `${origin}${leader.metaUrl}` } : {}),
      })),
    },
  ];

  const faqNode = buildFaqSchema(faqEntries.value, canonicalUrl.value);
  if (faqNode) graph.push(faqNode);

  return { "@context": "https://schema.org", "@graph": graph };
});

useHead(() => ({
  script: structuredData.value
    ? [{ type: "application/ld+json", innerHTML: JSON.stringify(structuredData.value) }]
    : [],
}));

function formatDate(dateUtc: string | undefined) {
  if (!dateUtc) return "";
  try {
    return ParseToHumanReadableText(dateUtc);
  } catch {
    return dateUtc;
  }
}
</script>

<template>
  <div class="bg-gray-100">
    <MetaTierHeader v-if="tierList" :tier-list="tierList" :heading="heading" />

    <section v-if="tierError" class="container px-4 md:px-8 py-12">
      <div class="bg-red-50 border border-red-200 text-red-800 rounded-lg px-5 py-4">
        <h2 class="text-red-900 text-lg font-bold mb-1">Tier list unavailable</h2>
        <p class="mb-0">
          The rankings could not be loaded just now. Please try again shortly.
        </p>
      </div>
    </section>

    <template v-else-if="tierList">
      <section class="container px-4 md:px-8 pt-8">
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div
            v-if="content.properties?.intro"
            class="prose max-w-2xl text-gray-700"
            v-html="content.properties?.intro.markup"
          />
          <div v-if="periods.length > 1" class="flex items-center gap-3 ml-auto">
            <label for="period-select" class="text-sm text-gray-600 font-medium">Set</label>
            <select
              id="period-select"
              :value="selectedPeriodId"
              class="border border-gray-300 rounded px-3 py-2 text-sm bg-white"
              @change="
                navigateTo({
                  query: {
                    ...route.query,
                    period: ($event.target as HTMLSelectElement).value,
                  },
                })
              "
            >
              <option v-for="period in periods" :key="period.id" :value="period.id">
                {{ period.name }}
              </option>
            </select>
          </div>
        </div>
      </section>

      <section class="container px-4 md:px-8 py-8">
        <template v-if="hasRankedLeaders">
          <MetaTierTable
            v-for="tier in TIER_ORDER"
            :key="tier"
            :tier="tier"
            :leaders="leadersByTier.get(tier) ?? []"
            :deltas-available="tierList.deltasAvailable"
            :deltas-unavailable-reason="tierList.deltasUnavailableReason"
          />
        </template>
        <p v-else class="text-gray-600">
          No leader has enough tournament results to rank yet for this set. Check back once more
          events have been played.
        </p>
      </section>

      <section v-if="content.properties?.analysis" class="bg-white py-10">
        <div class="container px-4 md:px-8">
          <h2 class="mb-6">What changed and why</h2>
          <div class="prose max-w-3xl text-gray-800" v-html="content.properties?.analysis.markup" />
        </div>
      </section>

      <div class="container px-4 md:px-8 py-10">
        <FaqSection :entries="faqEntries" />
      </div>
    </template>

    <section
      v-if="recentTournaments.length > 0 || recentWinners.length > 0"
      class="bg-white border-t border-gray-200 py-10"
    >
      <div class="container px-4 md:px-8 grid gap-10 lg:grid-cols-2">
        <!-- min-w-0: grid items default to min-width:auto, so the nowrap names below overflow. -->
        <div v-if="recentTournaments.length > 0" class="min-w-0">
          <h2 class="mb-4 text-xl">Recent events</h2>
          <ul class="list-none p-0 m-0 divide-y divide-gray-100">
            <li
              v-for="tournament in recentTournaments"
              :key="tournament.id"
              class="py-3 flex items-baseline justify-between gap-4"
            >
              <div class="min-w-0 flex-1">
                <div class="font-semibold text-sm truncate">{{ tournament.name }}</div>
                <div class="text-xs text-gray-500">
                  {{ formatDate(tournament.dateUtc) }} &bull; {{ tournament.playerCount }} players
                </div>
              </div>
              <a
                v-if="tournament.externalUrl"
                :href="tournament.externalUrl"
                rel="noopener"
                class="text-xs text-gray-600 hover:text-gray-900 whitespace-nowrap"
              >
                Standings ↗
              </a>
            </li>
          </ul>
        </div>

        <div v-if="recentWinners.length > 0" class="min-w-0">
          <h2 class="mb-4 text-xl">Recent winners</h2>
          <ul class="list-none p-0 m-0 divide-y divide-gray-100">
            <li v-for="deck in recentWinners" :key="`${deck.tournamentId}-${deck.deckId}`" class="py-3">
              <div class="font-semibold text-sm">{{ deck.deckName ?? deck.leaderName ?? "Unknown" }}</div>
              <div class="text-xs text-gray-500">
                {{ deck.playerName }} &bull; {{ deck.tournamentName }}
              </div>
            </li>
          </ul>
        </div>
      </div>
      <div class="container px-4 md:px-8 pt-6">
        <p class="text-xs text-gray-500 mb-0">
          Tournament data sourced from
          <a href="https://melee.gg" rel="noopener" class="underline">melee.gg</a>.
        </p>
      </div>
    </section>
  </div>
</template>
