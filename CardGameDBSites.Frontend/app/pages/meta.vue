<script setup lang="ts">
import MetaService from '~/services/MetaService';
import type { ArchetypeMetaResult } from '~/services/MetaService';

useHead({ title: 'Meta' });

const periodOptions = [
    { label: 'Last 2 weeks', days: 14 },
    { label: 'Last month', days: 30 },
    { label: 'Last 3 months', days: 90 },
    { label: 'Last 6 months', days: 180 },
];

const selectedPeriodDays = ref(14);
const isLoading = ref(false);

const topDecks = ref<ArchetypeMetaResult[]>([]);
const trendingDecks = ref<ArchetypeMetaResult[]>([]);

const metaService = new MetaService();

async function loadData() {
    isLoading.value = true;
    try {
        const now = new Date();
        const currentStart = new Date(now);
        currentStart.setDate(currentStart.getDate() - selectedPeriodDays.value);
        const previousStart = new Date(currentStart);
        previousStart.setDate(previousStart.getDate() - selectedPeriodDays.value);

        [topDecks.value, trendingDecks.value] = await Promise.all([
            metaService.getTopDecks(),
            metaService.getTrending(undefined, currentStart, now, previousStart, currentStart),
        ]);
    } finally {
        isLoading.value = false;
    }
}

await loadData();

async function onPeriodChange() {
    await loadData();
}

function trendLabel(trend: number): string {
    if (trend > 0) return `▲ ${trend}`;
    if (trend < 0) return `▼ ${Math.abs(trend)}`;
    return '—';
}

function trendClass(trend: number): string {
    if (trend > 0) return 'text-green-600';
    if (trend < 0) return 'text-red-600';
    return 'text-gray-500';
}
</script>

<template>
    <div class="container px-4 py-6 md:px-8">
        <h1 class="text-2xl font-bold mb-4">Meta</h1>

        <div class="mb-6 flex items-center gap-3">
            <label class="text-sm font-medium text-gray-700">Time period:</label>
            <select
                v-model="selectedPeriodDays"
                class="border rounded px-3 py-1.5 text-sm"
                :disabled="isLoading"
                @change="onPeriodChange"
            >
                <option
                    v-for="option in periodOptions"
                    :key="option.days"
                    :value="option.days"
                >
                    {{ option.label }}
                </option>
            </select>
            <span v-if="isLoading" class="text-sm text-gray-500">Loading…</span>
        </div>

        <div class="grid md:grid-cols-2 gap-6">
            <div class="bg-white rounded p-4">
                <h2 class="text-lg font-semibold mb-3">Top Archetypes</h2>
                <div v-if="topDecks.length === 0" class="text-gray-500 text-sm">No data available.</div>
                <table v-else class="w-full text-sm border-collapse">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="px-3 py-2 border border-gray-200">#</th>
                            <th class="px-3 py-2 border border-gray-200">Archetype</th>
                            <th class="px-3 py-2 border border-gray-200 text-center">Entries</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="(result, index) in topDecks"
                            :key="result.archetype.id"
                            class="hover:bg-gray-50 border-b border-gray-100"
                        >
                            <td class="px-3 py-2 border border-gray-200 font-semibold">{{ index + 1 }}</td>
                            <td class="px-3 py-2 border border-gray-200">
                                <span class="font-medium">{{ result.archetype.name }}</span>
                                <p v-if="result.archetype.description" class="text-xs text-gray-500 mt-0.5">
                                    {{ result.archetype.description }}
                                </p>
                            </td>
                            <td class="px-3 py-2 border border-gray-200 text-center">{{ result.deckCount }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="bg-white rounded p-4">
                <h2 class="text-lg font-semibold mb-3">Trending Decks</h2>
                <div v-if="trendingDecks.length === 0" class="text-gray-500 text-sm">No data available.</div>
                <table v-else class="w-full text-sm border-collapse">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="px-3 py-2 border border-gray-200">Archetype</th>
                            <th class="px-3 py-2 border border-gray-200 text-center">Current</th>
                            <th class="px-3 py-2 border border-gray-200 text-center">Previous</th>
                            <th class="px-3 py-2 border border-gray-200 text-center">Trend</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="result in trendingDecks"
                            :key="result.archetype.id"
                            class="hover:bg-gray-50 border-b border-gray-100"
                        >
                            <td class="px-3 py-2 border border-gray-200">
                                <span class="font-medium">{{ result.archetype.name }}</span>
                            </td>
                            <td class="px-3 py-2 border border-gray-200 text-center">{{ result.deckCount }}</td>
                            <td class="px-3 py-2 border border-gray-200 text-center text-gray-500">
                                {{ result.previousDeckCount }}
                            </td>
                            <td class="px-3 py-2 border border-gray-200 text-center font-semibold" :class="trendClass(result.trend)">
                                {{ trendLabel(result.trend) }}
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
