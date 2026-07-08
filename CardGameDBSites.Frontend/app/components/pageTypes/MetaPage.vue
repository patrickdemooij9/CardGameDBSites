<script setup lang="ts">
import { ref } from "vue";
import type {
  TournamentSummaryApiModel,
  TournamentEntrantApiModel,
} from "~/api/default";
import TournamentService, {
  type MetaWinningDeckApiModel,
  type MetaLeaderApiModel,
  type MetaPopularCardApiModel,
  type PeriodApiModel,
} from "~/services/TournamentService";
import { ParseToHumanReadableText } from "~/helpers/DateHelper";

// ── Leader configuration ─────────────────────────────────────────────────────
// A deck's leader is the card in this group/slot. This differs per game type;
// for now it is configured here and passed to the meta endpoints.
const LEADER_GROUP_ID = 1;
const LEADER_SLOT_ID = 0;
// The meta page currently only supports a single format per site.
const DEFAULT_FORMAT_ID = 1;

const service = new TournamentService();
const periods = ref<PeriodApiModel[]>([]);
const selectedPeriodId = ref<number | undefined>(undefined);
const recentTournaments = ref<TournamentSummaryApiModel[]>([]);
const featuredTop8Leaders = ref<MetaLeaderApiModel[]>([]);
const recentWinnersData = ref<MetaWinningDeckApiModel[]>([]);
const topLeadersData = ref<MetaLeaderApiModel[]>([]);
const popularCardsData = ref<MetaPopularCardApiModel[]>([]);

async function loadMetaData(periodId: number) {
  try {
    recentTournaments.value = await service.getRecent(periodId, 6);
    const firstId = recentTournaments.value[0]?.id;
    featuredTop8Leaders.value =
      recentTournaments.value.length > 0 && firstId != null
        ? await service.getTopLeaders(periodId, 5, LEADER_GROUP_ID, LEADER_SLOT_ID, firstId)
        : [];

    [recentWinnersData.value, topLeadersData.value, popularCardsData.value] =
      await Promise.all([
        service.getRecentWinners(periodId, 6, LEADER_GROUP_ID, LEADER_SLOT_ID),
        service.getTopLeaders(periodId, 5, LEADER_GROUP_ID, LEADER_SLOT_ID),
        service.getPopularCards(periodId, 8, LEADER_GROUP_ID, LEADER_SLOT_ID),
      ]);
  } catch {
    // API not available – fall back to mock data below
  }
}

try {
  periods.value = await service.getPeriods(DEFAULT_FORMAT_ID);
  // No "current" (open-ended) period? Fall back to the most recent one - periods
  // are ordered by StartingDateUtc descending by the API.
  selectedPeriodId.value = periods.value.find((p) => p.isCurrent)?.id ?? periods.value[0]?.id;
} catch {
  // API not available – no periods to select from
}

if (selectedPeriodId.value != null) {
  await loadMetaData(selectedPeriodId.value);
}

async function onPeriodChange() {
  if (selectedPeriodId.value != null) {
    await loadMetaData(selectedPeriodId.value);
  }
}

const selectedPeriodLabel = computed(() => {
  return periods.value.find((p) => p.id === selectedPeriodId.value)?.name ?? "No Period Available";
});

const hasTournaments = recentTournaments.value.length > 0;
const featuredTournamentData = hasTournaments
  ? recentTournaments.value[0]
  : null;

// Top-8 distribution grouped by deck name for the bar chart
const top8Distribution = computed(() => {
  const source = hasTournaments ? featuredTop8Leaders.value : [];
  const grouped: Record<string, number> = {};
  for (const e of source) {
    const key = e.leaderName ?? "Unknown";
    grouped[key] = (grouped[key] ?? 0) + 1;
  }
  return Object.entries(grouped)
    .map(([leader, count]) => ({ leader, count }))
    .sort((a, b) => b.count - a.count);
});

const displayedTournaments = computed(() =>
  hasTournaments ? recentTournaments.value : [],
);

const displayedFeatured = computed(
  () => featuredTournamentData,
);

function formatDate(dateUtc: string | undefined) {
  if (!dateUtc) return "";
  try {
    return ParseToHumanReadableText(dateUtc);
  } catch {
    return dateUtc;
  }
}

const recentWinners = computed(() => {
  return recentWinnersData.value.map((d) => ({
    name: d.deckName ?? "Unknown",
    leader: d.leaderName ?? "Unknown",
    tournament: d.tournamentName,
    date: formatDate(d.tournamentDateUtc),
    author: d.playerName ?? "Unknown",
  }));
});

const topLeaders = computed(() => {
  return topLeadersData.value.map((l) => ({
    name: l.leaderName,
    wins: l.wins,
    top8: l.top8Count,
  }));
});

const maxWins = computed(() => topLeaders.value[0]?.wins ?? 1);

const popularCards = computed(() => {
  return popularCardsData.value.map((c) => ({
    name: c.cardName,
    percentage: c.percentage,
  }));
});
</script>

<template>
  <div class="bg-gray-100">
    <!-- Hero Section -->
    <section class="bg-gray-900 text-white py-20 px-4">
      <div class="container md:px-8 text-center">
        <h1 class="text-white text-5xl font-bold mb-4">
          Star Wars Unlimited Meta
        </h1>
        <p class="text-gray-300 text-xl mb-8 max-w-2xl mx-auto">
          Track tournament results, winning decks, and the latest competitive
          trends.
        </p>
      </div>
    </section>

    <!-- Period Filter -->
    <section class="container px-4 md:px-8 pt-8" v-if="periods.length > 0">
      <div class="flex items-center justify-end gap-3">
        <label for="period-select" class="text-sm text-gray-600 font-medium">Period</label>
        <select
          id="period-select"
          v-model="selectedPeriodId"
          @change="onPeriodChange"
          class="border border-gray-300 rounded px-3 py-2 text-sm bg-white"
        >
          <option v-for="period in periods" :key="period.id" :value="period.id">
            {{ period.name }}
          </option>
        </select>
      </div>
    </section>

    <!-- Featured Tournament Section -->
    <section class="container px-4 md:px-8 py-12" v-if="displayedFeatured">
      <h2 class="mb-6">Latest Tournament</h2>
      <div class="bg-white rounded-xl shadow-md overflow-hidden">
        <div
          class="bg-gray-900 text-white px-6 py-4 flex flex-wrap items-center justify-between gap-2"
        >
          <div>
            <h3 class="text-white text-2xl font-bold mb-1">
              {{ displayedFeatured.name }}
            </h3>
            <span class="text-gray-400 text-sm"
              >{{ formatDate(displayedFeatured.dateUtc) }} &bull;
              {{ displayedFeatured.playerCount }} Players</span
            >
          </div>
          <span
            class="bg-yellow-400 text-gray-900 text-xs font-bold px-3 py-1 rounded-full uppercase tracking-wider"
            >Latest Event</span
          >
        </div>
        <div class="p-6 grid md:grid-cols-2 gap-8">
          <!-- Winner -->
          <div>
            <p
              class="text-sm text-gray-500 uppercase tracking-wide font-semibold mb-3"
            >
              Winner
            </p>
            <div>
              <div class="text-lg font-bold">
                {{ displayedFeatured.winner?.deckName ?? "Unknown" }}
              </div>
              <div class="text-sm text-gray-500">
                {{ displayedFeatured.winner?.playerName }}
              </div>
            </div>
            <div class="pt-6">
              <a
                :href="displayedFeatured.externalUrl ?? '#'"
                class="no-underline inline-block bg-gray-900 text-white hover:bg-gray-700 font-semibold px-5 py-2 rounded transition-colors"
              >
                View Full Results
              </a>
            </div>
          </div>
          <!-- Top 8 Distribution -->
          <div>
            <p
              class="text-sm text-gray-500 uppercase tracking-wide font-semibold mb-3"
            >
              Top 8 Leaders
            </p>
            <div class="space-y-2">
              <div
                v-for="entry in top8Distribution"
                :key="entry.leader"
                class="flex items-center gap-3"
              >
                <span class="text-sm font-medium w-1/2 truncate text-gray-700">{{
                  entry.leader
                }}</span>
                <div
                  class="flex-1 bg-gray-100 rounded-full h-4 overflow-hidden"
                >
                  <div
                    class="h-4 bg-yellow-400 rounded-full"
                    :style="`width: ${(entry.count / 8) * 100}%`"
                  ></div>
                </div>
                <span class="text-sm text-gray-500 w-4">{{ entry.count }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Recent Tournament Results Section -->
    <section id="recent-events" class="container px-4 md:px-8 pb-12">
      <h2 class="mb-6">Recent Events</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="tournament in displayedTournaments"
          :key="tournament.id"
          class="bg-white rounded-lg shadow-sm p-5 flex flex-col gap-3 hover:shadow-md transition-shadow"
        >
          <div class="flex items-start justify-between gap-2">
            <h3 class="text-base font-bold leading-tight">
              {{ tournament.name }}
            </h3>
          </div>
          <div class="text-sm text-gray-500">
            {{ formatDate(tournament.dateUtc) }} &bull;
            {{ tournament.playerCount }} Players
          </div>
          <div class="flex items-center gap-3">
            <div class="text-sm font-semibold">
                {{ tournament.winner?.deckName ?? "Unknown" }}
              </div>
          </div>
          <div class="mt-auto pt-1">
            <a
              :href="tournament.externalUrl ?? '#'"
              class="no-underline text-sm bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium px-3 py-1.5 rounded inline-block transition-colors"
            >
              View Results
            </a>
          </div>
        </div>
      </div>
    </section>

    <!-- Recent Winning Decks Section -->
    <section class="bg-white py-12">
      <div class="container px-4 md:px-8">
        <div class="flex items-center justify-between mb-6">
          <h2 class="mb-0">Recent Winners</h2>
          <!--<a
            href="#"
            class="no-underline text-sm font-medium bg-gray-900 text-white hover:bg-gray-700 px-4 py-2 rounded transition-colors"
          >
            View All Winning Decks
          </a>-->
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="deck in recentWinners"
            :key="deck.name"
            class="border border-gray-200 rounded-lg p-5 hover:shadow-md transition-shadow"
          >
            <div class="flex items-start gap-4 mb-3">
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap mb-1">
                  <span
                    class="text-xs bg-yellow-100 text-yellow-800 font-semibold px-2 py-0.5 rounded-full"
                    >🏆 Winner</span
                  >
                </div>
                <div class="font-bold text-base leading-tight">
                  {{ deck.name }}
                </div>
                <div class="text-sm text-gray-500">{{ deck.leader }}</div>
              </div>
            </div>
            <div class="text-sm text-gray-600">
              <div class="truncate">{{ deck.tournament }}</div>
              <div class="text-gray-400 mt-0.5">{{ deck.date }}</div>
            </div>
            <div class="text-xs text-gray-400 mt-2">by {{ deck.author }}</div>
          </div>
        </div>
      </div>
    </section>

    <!-- Most Successful Leaders Section -->
    <section class="container px-4 md:px-8 py-12">
      <h2 class="mb-6">
        Most Successful Leaders
        <span class="text-gray-500 text-lg font-normal">({{ selectedPeriodLabel }})</span>
      </h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
        <div
          v-for="(leader, index) in topLeaders"
          :key="leader.name"
          class="bg-white rounded-lg shadow-sm p-5 flex flex-col items-center text-center hover:shadow-md transition-shadow"
        >
          <div class="text-lg font-bold text-gray-400 mb-2">
            #{{ index + 1 }}
          </div>
          <div class="font-bold text-sm mb-3 leading-tight">
            {{ leader.name }}
          </div>
          <div class="w-full mb-2">
            <div class="flex justify-between text-xs text-gray-500 mb-1">
              <span>Wins</span><span>{{ leader.wins }}</span>
            </div>
            <div class="bg-gray-100 rounded-full h-2 overflow-hidden">
              <div
                class="h-2 bg-yellow-400 rounded-full"
                :style="`width: ${(leader.wins / maxWins) * 100}%`"
              ></div>
            </div>
          </div>
          <div class="text-xs text-gray-500">
            {{ leader.top8 }} Top 8 Appearances
          </div>
        </div>
      </div>
    </section>

    <!-- Cards in Winning Decks Section -->
    <section class="bg-white py-12">
      <div class="container px-4 md:px-8">
        <h2 class="mb-6">Cards Driving Recent Success</h2>
        <div class="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-4 gap-4">
          <div
            v-for="card in popularCards"
            :key="card.name"
            class="border border-gray-200 rounded-lg overflow-hidden hover:shadow-md transition-shadow"
          >
            <div
              class="bg-gray-100 h-28 flex items-center justify-center text-gray-400 text-xs font-medium"
            >
              Card Image
            </div>
            <div class="p-3">
              <div class="font-semibold text-sm leading-tight mb-1">
                {{ card.name }}
              </div>
              <div class="text-xs text-gray-500 mb-2">
                {{ card.percentage }}% of winning decks
              </div>
              <div class="bg-gray-100 rounded-full h-1.5 overflow-hidden">
                <div
                  class="h-1.5 bg-green-500 rounded-full"
                  :style="`width: ${card.percentage}%`"
                ></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>
