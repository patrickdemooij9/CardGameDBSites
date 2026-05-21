<script setup lang="ts">
import { ParseToHumanReadableText } from '~/helpers/DateHelper';
import TournamentService from '~/services/TournamentService';
import type { TournamentEvent } from '~/services/TournamentService';

const route = useRoute();
const id = route.params.id as string;

const tournament = ref<TournamentEvent | null>(null);
const error = ref(false);

try {
    tournament.value = await new TournamentService().get(id);
} catch {
    error.value = true;
}

if (!tournament.value) {
    throw createError({ statusCode: 404, statusMessage: 'Tournament not found' });
}

const sortedEntrants = computed(() => {
    if (!tournament.value) return [];
    return [...tournament.value.entrants].sort((a, b) => {
        if (a.placement == null && b.placement == null) return 0;
        if (a.placement == null) return 1;
        if (b.placement == null) return -1;
        return a.placement - b.placement;
    });
});

useHead({ title: tournament.value?.name ?? 'Tournament' });
</script>

<template>
    <div class="container px-4 py-6 md:px-8">
        <div v-if="tournament" class="bg-white rounded p-6">
            <div class="mb-6">
                <h1 class="text-2xl font-bold mb-1">{{ tournament.name }}</h1>
                <div class="flex flex-wrap gap-4 text-sm text-gray-600 mt-2">
                    <span>📅 {{ ParseToHumanReadableText(tournament.date) }}</span>
                    <span v-if="tournament.formatDisplayName">🎮 {{ tournament.formatDisplayName }}</span>
                    <span v-if="tournament.playerCount">👥 {{ tournament.playerCount }} players</span>
                    <span v-if="tournament.sourceType">🗂️ {{ tournament.sourceType }}</span>
                    <a
                        v-if="tournament.sourceUrl"
                        :href="tournament.sourceUrl"
                        target="_blank"
                        rel="noopener noreferrer"
                        class="text-blue-600 hover:underline"
                    >
                        🔗 Source
                    </a>
                </div>
            </div>

            <h2 class="text-lg font-semibold mb-3">Standings</h2>
            <div v-if="sortedEntrants.length === 0" class="text-gray-500">
                No entrants recorded for this tournament.
            </div>
            <div v-else class="overflow-x-auto">
                <table class="w-full text-sm border-collapse">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="px-3 py-2 border border-gray-200">#</th>
                            <th class="px-3 py-2 border border-gray-200">Player</th>
                            <th class="px-3 py-2 border border-gray-200 text-center">W</th>
                            <th class="px-3 py-2 border border-gray-200 text-center">L</th>
                            <th class="px-3 py-2 border border-gray-200 text-center">D</th>
                            <th class="px-3 py-2 border border-gray-200">Deck</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="entrant in sortedEntrants"
                            :key="entrant.id"
                            class="hover:bg-gray-50 border-b border-gray-100"
                        >
                            <td class="px-3 py-2 border border-gray-200 font-semibold">
                                {{ entrant.placement ?? '—' }}
                            </td>
                            <td class="px-3 py-2 border border-gray-200">{{ entrant.playerName }}</td>
                            <td class="px-3 py-2 border border-gray-200 text-center text-green-700">
                                {{ entrant.wins ?? '—' }}
                            </td>
                            <td class="px-3 py-2 border border-gray-200 text-center text-red-700">
                                {{ entrant.losses ?? '—' }}
                            </td>
                            <td class="px-3 py-2 border border-gray-200 text-center text-gray-600">
                                {{ entrant.draws ?? '—' }}
                            </td>
                            <td class="px-3 py-2 border border-gray-200">
                                <a
                                    v-if="entrant.deckId"
                                    :href="`/decks/${entrant.deckId}`"
                                    class="text-blue-600 hover:underline"
                                >
                                    View Deck
                                </a>
                                <span v-else class="text-gray-400">—</span>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
