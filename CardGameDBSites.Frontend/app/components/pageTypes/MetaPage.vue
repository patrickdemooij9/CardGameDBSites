<script setup lang="ts">
import { ref } from 'vue';
import type { TournamentSummaryApiModel, TournamentEntrantApiModel } from '~/api/default';
import TournamentService from '~/services/TournamentService';
import { ParseToHumanReadableText } from '~/helpers/DateHelper';

// ── Real data ──────────────────────────────────────────────────────────────
const service = new TournamentService();
const recentTournaments = ref<TournamentSummaryApiModel[]>([]);
const featuredTop8 = ref<TournamentEntrantApiModel[]>([]);

try {
  recentTournaments.value = await service.getRecent(6);
  const firstId = recentTournaments.value[0]?.id;
  if (recentTournaments.value.length > 0 && firstId != null) {
    featuredTop8.value = await service.getTop8(firstId);
  }
} catch {
  // API not available – fall back to mock data below
}

const hasTournaments = recentTournaments.value.length > 0;
const featuredTournamentData = hasTournaments ? recentTournaments.value[0] : null;

// Top-8 distribution grouped by deck name for the bar chart
const top8Distribution = computed(() => {
  const source = hasTournaments ? featuredTop8.value : mockFeaturedTop8;
  const grouped: Record<string, number> = {};
  for (const e of source) {
    const key = e.deckName ?? 'Unknown';
    grouped[key] = (grouped[key] ?? 0) + 1;
  }
  return Object.entries(grouped)
    .map(([leader, count]) => ({ leader, count }))
    .sort((a, b) => b.count - a.count);
});

// ── Static / mock data (used when no tournament data exists yet) ────────────
const stats = [
  { label: 'Tournaments Tracked', value: hasTournaments ? recentTournaments.value.length.toString() : '143' },
  { label: 'Competitive Decks', value: '4,231' },
  { label: 'Published Decklists', value: '18,431' },
  { label: 'Updated', value: 'Daily' },
];

const mockFeaturedTournament: TournamentSummaryApiModel = {
  name: 'Planetary Qualifier Amsterdam',
  dateUtc: '2026-06-06',
  playerCount: 128,
  externalUrl: undefined,
  winner: { playerName: 'Player1', deckName: 'Sabine Green Aggro' },
};

const mockFeaturedTop8: TournamentEntrantApiModel[] = [
  { deckName: 'Sabine' },
  { deckName: 'Sabine' },
  { deckName: 'Sabine' },
  { deckName: 'Boba' },
  { deckName: 'Boba' },
  { deckName: 'Luke' },
  { deckName: 'Luke' },
  { deckName: 'Han' },
];

const mockRecentTournaments = [
  { id: 0, name: 'Planetary Qualifier Amsterdam', dateUtc: '2026-06-06', playerCount: 128, winner: { deckName: 'Sabine Green Aggro' } },
  { id: 0, name: 'Regional Open Berlin', dateUtc: '2026-05-31', playerCount: 96, winner: { deckName: 'Boba Control' } },
  { id: 0, name: 'Store Championship London', dateUtc: '2026-05-25', playerCount: 64, winner: { deckName: 'Luke Heroic Aggro' } },
  { id: 0, name: 'Galactic Circuit Madrid', dateUtc: '2026-05-18', playerCount: 112, winner: { deckName: 'Han Smuggler Midrange' } },
  { id: 0, name: 'Sector Championship Paris', dateUtc: '2026-05-10', playerCount: 80, winner: { deckName: 'Sabine Hyperspace' } },
  { id: 0, name: 'Regional Open Copenhagen', dateUtc: '2026-05-03', playerCount: 72, winner: { deckName: 'Vader Imperial Control' } },
] as TournamentSummaryApiModel[];

const displayedTournaments = computed(() =>
  hasTournaments ? recentTournaments.value : mockRecentTournaments
);

const displayedFeatured = computed(() =>
  featuredTournamentData ?? mockFeaturedTournament
);

function formatDate(dateUtc: string | undefined) {
  if (!dateUtc) return '';
  try { return ParseToHumanReadableText(dateUtc); } catch { return dateUtc; }
}

// ── Still-static sections (cards/leaders — no backend data yet) ─────────────
const recentWinners = [
  { name: 'Sabine Green Aggro', leader: 'Sabine Wren', tournament: 'Planetary Qualifier Amsterdam', date: 'June 6, 2026', author: 'DeckMaster99' },
  { name: 'Boba Control', leader: 'Boba Fett', tournament: 'Regional Open Berlin', date: 'May 31, 2026', author: 'CardShark42' },
  { name: 'Luke Heroic Aggro', leader: 'Luke Skywalker', tournament: 'Store Championship London', date: 'May 25, 2026', author: 'JediKnight7' },
  { name: 'Han Smuggler Midrange', leader: 'Han Solo', tournament: 'Galactic Circuit Madrid', date: 'May 18, 2026', author: 'SpiceRunner' },
  { name: 'Sabine Hyperspace', leader: 'Sabine Wren', tournament: 'Sector Championship Paris', date: 'May 10, 2026', author: 'MandoWarrior' },
  { name: 'Vader Imperial Control', leader: 'Darth Vader', tournament: 'Regional Open Copenhagen', date: 'May 3, 2026', author: 'DarkSideUser' },
];

const topLeaders = [
  { name: 'Sabine Wren', wins: 12, top8: 34 },
  { name: 'Boba Fett', wins: 9, top8: 28 },
  { name: 'Luke Skywalker', wins: 7, top8: 22 },
  { name: 'Han Solo', wins: 5, top8: 18 },
  { name: 'Darth Vader', wins: 4, top8: 15 },
];

const maxWins = topLeaders[0]?.wins ?? 1;

const popularCards = [
  { name: 'Superlaser Blast', percentage: 73 },
  { name: 'Triple Dark Raid', percentage: 68 },
  { name: "Luke's Lightsaber", percentage: 61 },
  { name: 'Tarkintown Raid', percentage: 58 },
  { name: 'Waylay', percentage: 54 },
  { name: 'Open Fire', percentage: 51 },
  { name: 'Reinforcement Walker', percentage: 47 },
  { name: 'Battle Meditation', percentage: 43 },
];

const trendingCards = [
  { name: 'Superlaser Blast', change: '+18%' },
  { name: 'Reinforcement Walker', change: '+15%' },
  { name: 'Open Fire', change: '+12%' },
  { name: 'Battle Meditation', change: '+9%' },
  { name: 'Waylay', change: '+7%' },
  { name: 'Force Push', change: '+6%' },
];

const deckTabs = ['Recent Winners', 'Most Viewed', 'Most Liked', 'Recently Updated'];
const activeTab = ref('Recent Winners');

const explorerDecks = [
  { name: 'Sabine Green Aggro', leader: 'Sabine Wren', author: 'DeckMaster99', views: 3241, likes: 187, updated: 'June 6, 2026' },
  { name: 'Boba Control', leader: 'Boba Fett', author: 'CardShark42', views: 2891, likes: 154, updated: 'May 31, 2026' },
  { name: 'Luke Heroic Aggro', leader: 'Luke Skywalker', author: 'JediKnight7', views: 2504, likes: 132, updated: 'May 25, 2026' },
  { name: 'Han Smuggler Midrange', leader: 'Han Solo', author: 'SpiceRunner', views: 2188, likes: 121, updated: 'May 18, 2026' },
  { name: 'Sabine Hyperspace', leader: 'Sabine Wren', author: 'MandoWarrior', views: 1974, likes: 109, updated: 'May 10, 2026' },
  { name: 'Vader Imperial Control', leader: 'Darth Vader', author: 'DarkSideUser', views: 1823, likes: 98, updated: 'May 3, 2026' },
  { name: 'Leia Rebel Support', leader: 'Leia Organa', author: 'RebelScum', views: 1654, likes: 87, updated: 'April 28, 2026' },
  { name: 'Maul Aggro Rush', leader: 'Maul', author: 'SithLord', views: 1502, likes: 76, updated: 'April 20, 2026' },
];
</script>

<template>
  <div class="bg-gray-100">

    <!-- Hero Section -->
    <section class="bg-gray-900 text-white py-20 px-4">
      <div class="container md:px-8 text-center">
        <h1 class="text-white text-5xl font-bold mb-4">Star Wars Unlimited Meta</h1>
        <p class="text-gray-300 text-xl mb-8 max-w-2xl mx-auto">
          Track tournament results, winning decks, and the latest competitive trends.
        </p>
        <div class="flex flex-wrap justify-center gap-4 mb-12">
          <a href="#recent-events" class="no-underline bg-yellow-400 hover:bg-yellow-300 text-gray-900 font-semibold px-6 py-3 rounded transition-colors">
            Latest Tournament Results
          </a>
          <a href="#deck-explorer" class="no-underline border border-white text-white hover:bg-white hover:text-gray-900 font-semibold px-6 py-3 rounded transition-colors">
            Browse Competitive Decks
          </a>
        </div>

        <!-- Stat Cards -->
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 max-w-4xl mx-auto">
          <div v-for="stat in stats" :key="stat.label" class="bg-gray-800 rounded-lg p-4">
            <div class="text-2xl font-bold text-yellow-400">{{ stat.value }}</div>
            <div class="text-sm text-gray-400 mt-1">{{ stat.label }}</div>
          </div>
        </div>
      </div>
    </section>

    <!-- Featured Tournament Section -->
    <section class="container px-4 md:px-8 py-12">
      <h2 class="mb-6">Latest Tournament</h2>
      <div class="bg-white rounded-xl shadow-md overflow-hidden">
        <div class="bg-gray-900 text-white px-6 py-4 flex flex-wrap items-center justify-between gap-2">
          <div>
            <h3 class="text-white text-2xl font-bold mb-1">{{ displayedFeatured.name }}</h3>
            <span class="text-gray-400 text-sm">{{ formatDate(displayedFeatured.dateUtc) }} &bull; {{ displayedFeatured.playerCount }} Players</span>
          </div>
          <span class="bg-yellow-400 text-gray-900 text-xs font-bold px-3 py-1 rounded-full uppercase tracking-wider">Latest Event</span>
        </div>
        <div class="p-6 grid md:grid-cols-2 gap-8">
          <!-- Winner -->
          <div>
            <p class="text-sm text-gray-500 uppercase tracking-wide font-semibold mb-3">Winner</p>
            <div class="flex items-center gap-4">
              <div class="w-16 h-16 bg-gray-200 rounded-lg flex items-center justify-center text-gray-400 text-xs font-medium">
                IMG
              </div>
              <div>
                <div class="text-lg font-bold">{{ displayedFeatured.winner?.deckName ?? 'Unknown' }}</div>
                <div class="text-sm text-gray-500">{{ displayedFeatured.winner?.playerName }}</div>
              </div>
            </div>
          </div>
          <!-- Top 8 Distribution -->
          <div>
            <p class="text-sm text-gray-500 uppercase tracking-wide font-semibold mb-3">Top 8 Leaders</p>
            <div class="space-y-2">
              <div v-for="entry in top8Distribution" :key="entry.leader" class="flex items-center gap-3">
                <span class="text-sm font-medium w-32 truncate text-gray-700">{{ entry.leader }}</span>
                <div class="flex-1 bg-gray-100 rounded-full h-4 overflow-hidden">
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
        <div class="px-6 pb-6">
          <a
            :href="displayedFeatured.externalUrl ?? '#'"
            class="no-underline inline-block bg-gray-900 text-white hover:bg-gray-700 font-semibold px-5 py-2 rounded transition-colors"
          >
            View Full Results
          </a>
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
            <h3 class="text-base font-bold leading-tight">{{ tournament.name }}</h3>
          </div>
          <div class="text-sm text-gray-500">{{ formatDate(tournament.dateUtc) }} &bull; {{ tournament.playerCount }} Players</div>
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 bg-gray-100 rounded flex items-center justify-center text-gray-400 text-xs">IMG</div>
            <div>
              <div class="text-sm font-semibold">{{ tournament.winner?.deckName ?? 'Unknown' }}</div>
            </div>
          </div>
          <div class="mt-auto pt-1">
            <a :href="tournament.externalUrl ?? '#'" class="no-underline text-sm bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium px-3 py-1.5 rounded inline-block transition-colors">
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
          <a href="#" class="no-underline text-sm font-medium bg-gray-900 text-white hover:bg-gray-700 px-4 py-2 rounded transition-colors">
            View All Winning Decks
          </a>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            v-for="deck in recentWinners"
            :key="deck.name"
            class="border border-gray-200 rounded-lg p-5 hover:shadow-md transition-shadow cursor-pointer"
          >
            <div class="flex items-start gap-4 mb-3">
              <div class="w-14 h-14 bg-gray-100 rounded-lg flex items-center justify-center text-gray-400 text-xs font-medium flex-shrink-0">IMG</div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap mb-1">
                  <span class="text-xs bg-yellow-100 text-yellow-800 font-semibold px-2 py-0.5 rounded-full">🏆 Winner</span>
                </div>
                <div class="font-bold text-base leading-tight">{{ deck.name }}</div>
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
      <h2 class="mb-6">Most Successful Leaders <span class="text-gray-500 text-lg font-normal">(Last 30 Days)</span></h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
        <div
          v-for="(leader, index) in topLeaders"
          :key="leader.name"
          class="bg-white rounded-lg shadow-sm p-5 flex flex-col items-center text-center hover:shadow-md transition-shadow"
        >
          <div class="text-lg font-bold text-gray-400 mb-2">#{{ index + 1 }}</div>
          <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center text-gray-400 text-xs mb-3">IMG</div>
          <div class="font-bold text-sm mb-3 leading-tight">{{ leader.name }}</div>
          <div class="w-full mb-2">
            <div class="flex justify-between text-xs text-gray-500 mb-1">
              <span>Wins</span><span>{{ leader.wins }}</span>
            </div>
            <div class="bg-gray-100 rounded-full h-2 overflow-hidden">
              <div class="h-2 bg-yellow-400 rounded-full" :style="`width: ${(leader.wins / maxWins) * 100}%`"></div>
            </div>
          </div>
          <div class="text-xs text-gray-500">{{ leader.top8 }} Top 8 Appearances</div>
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
            <div class="bg-gray-100 h-28 flex items-center justify-center text-gray-400 text-xs font-medium">
              Card Image
            </div>
            <div class="p-3">
              <div class="font-semibold text-sm leading-tight mb-1">{{ card.name }}</div>
              <div class="text-xs text-gray-500 mb-2">{{ card.percentage }}% of winning decks</div>
              <div class="bg-gray-100 rounded-full h-1.5 overflow-hidden">
                <div class="h-1.5 bg-green-500 rounded-full" :style="`width: ${card.percentage}%`"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Trending Cards Section -->
    <section class="container px-4 md:px-8 py-12">
      <h2 class="mb-6">Trending Up</h2>
      <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
        <div
          v-for="card in trendingCards"
          :key="card.name"
          class="bg-white rounded-lg shadow-sm overflow-hidden hover:shadow-md transition-shadow"
        >
          <div class="bg-gray-100 h-24 flex items-center justify-center text-gray-400 text-xs font-medium">
            Card Image
          </div>
          <div class="p-3">
            <div class="font-semibold text-sm leading-tight mb-1">{{ card.name }}</div>
            <div class="text-green-600 font-bold text-sm flex items-center gap-1">
              <span>▲</span>{{ card.change }}
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Competitive Deck Explorer Section -->
    <section id="deck-explorer" class="bg-white py-12">
      <div class="container px-4 md:px-8">
        <h2 class="mb-6">Explore Competitive Decks</h2>

        <!-- Tabs -->
        <div class="flex flex-wrap gap-2 mb-6 border-b border-gray-200 pb-0">
          <button
            v-for="tab in deckTabs"
            :key="tab"
            class="px-4 py-2 text-sm font-medium rounded-t transition-colors border-b-2"
            :class="activeTab === tab
              ? 'border-gray-900 text-gray-900 bg-white'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:bg-gray-50'"
            @click="activeTab = tab"
          >
            {{ tab }}
          </button>
        </div>

        <!-- Deck Cards -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div
            v-for="deck in explorerDecks"
            :key="deck.name"
            class="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer"
          >
            <div class="flex items-center gap-3 mb-3">
              <div class="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center text-gray-400 text-xs flex-shrink-0">IMG</div>
              <div class="flex-1 min-w-0">
                <div class="font-semibold text-sm leading-tight truncate">{{ deck.name }}</div>
                <div class="text-xs text-gray-500">{{ deck.leader }}</div>
              </div>
            </div>
            <div class="text-xs text-gray-500 mb-3">by {{ deck.author }}</div>
            <div class="flex items-center justify-between text-xs text-gray-400">
              <span>👁 {{ deck.views.toLocaleString() }}</span>
              <span>♥ {{ deck.likes }}</span>
              <span>{{ deck.updated }}</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Footer CTA Section -->
    <section class="bg-gray-900 text-white py-16 px-4">
      <div class="container md:px-8 text-center">
        <h2 class="text-white text-3xl font-bold mb-4">Explore the Competitive Meta</h2>
        <p class="text-gray-300 text-lg mb-8 max-w-xl mx-auto">
          Browse tournament-winning decks, discover successful strategies, and stay up to date with the latest Star Wars Unlimited results.
        </p>
        <div class="flex flex-wrap justify-center gap-4">
          <a href="#deck-explorer" class="no-underline bg-yellow-400 hover:bg-yellow-300 text-gray-900 font-semibold px-6 py-3 rounded transition-colors">
            Browse Decks
          </a>
          <a href="#recent-events" class="no-underline border border-white text-white hover:bg-white hover:text-gray-900 font-semibold px-6 py-3 rounded transition-colors">
            View Tournament Results
          </a>
        </div>
      </div>
    </section>

  </div>
</template>
