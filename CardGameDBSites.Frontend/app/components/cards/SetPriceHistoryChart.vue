<script setup lang="ts">
import { onMounted, ref, computed } from 'vue';
import { Line } from 'vue-chartjs';
import {
    Chart as ChartJS,
    Title,
    Tooltip,
    Legend,
    LineElement,
    PointElement,
    CategoryScale,
    LinearScale,
    Filler,
} from 'chart.js';
import { DoServerFetch } from '~/helpers/RequestsHelper';
import type { SetPriceHistoryItemApiModel } from '~/api/default/models/SetPriceHistoryItemApiModel';

ChartJS.register(Title, Tooltip, Legend, LineElement, PointElement, CategoryScale, LinearScale, Filler);

const props = defineProps<{
    setId: number;
}>();

const history = ref<SetPriceHistoryItemApiModel[]>([]);
const isLoading = ref(true);

onMounted(async () => {
    try {
        history.value = await DoServerFetch<SetPriceHistoryItemApiModel[]>(
            `/api/sets/priceHistory?setId=${props.setId}`,
            true,
        );
    } finally {
        isLoading.value = false;
    }
});

function expandPriceHistory(records: SetPriceHistoryItemApiModel[]) {
    if (!records.length) return { labels: [] as string[], data: [] as number[] };

    const sorted = [...records].sort((a, b) => a.date.localeCompare(b.date));
    const [sy, sm, sd] = sorted[0]!.date.split('-').map(Number);
    const start = new Date(Date.UTC(sy!, sm! - 1, sd));
    const now = new Date();
    const today = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate()));

    const labels: string[] = [];
    const data: number[] = [];

    let currentPrice = sorted[0]!.price;
    let recordIndex = 0;

    const current = new Date(start);
    while (current <= today) {
        const dateStr = current.toISOString().split('T')[0]!;

        while (recordIndex < sorted.length && sorted[recordIndex]!.date <= dateStr) {
            currentPrice = sorted[recordIndex]!.price;
            recordIndex++;
        }

        labels.push(dateStr);
        data.push(currentPrice);

        current.setUTCDate(current.getUTCDate() + 1);
    }

    return { labels, data };
}

const expanded = computed(() => expandPriceHistory(history.value));

const chartData = computed(() => ({
    labels: expanded.value.labels,
    datasets: [
        {
            label: 'Set Value',
            data: expanded.value.data,
            borderColor: '#60a5fa',
            backgroundColor: 'rgba(96, 165, 250, 0.08)',
            borderWidth: 1.5,
            pointRadius: 0,
            pointHoverRadius: 4,
            pointHitRadius: 10,
            fill: true,
            tension: 0,
        },
    ],
}));

const chartOptions = computed(() => ({
    responsive: true,
    maintainAspectRatio: false,
    interaction: {
        intersect: false,
        mode: 'index' as const,
    },
    plugins: {
        legend: { display: false },
        title: { display: false },
        tooltip: {
            callbacks: {
                title: (items: any[]) => {
                    const label = items[0]?.label ?? '';
                    const [year, month, day] = label.split('-').map(Number);
                    const date = new Date(Date.UTC(year, month - 1, day));
                    return date.toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                        timeZone: 'UTC',
                    });
                },
                label: (item: any) => ` $${item.parsed.y.toFixed(2)}`,
            },
        },
    },
    scales: {
        x: {
            grid: { color: 'rgba(0,0,0,0.06)' },
            ticks: {
                maxRotation: 0,
                maxTicksLimit: 13,
                callback: function (this: any, value: any): string {
                    const label: string = this.getLabelForValue(value);
                    if (!label) return '';
                    const [year, month, day] = label.split('-').map(Number);
                    const date = new Date(Date.UTC(year!, month! - 1, day));
                    return date.toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: 'short',
                        timeZone: 'UTC',
                    });
                },
            },
        },
        y: {
            beginAtZero: true,
            grid: { color: 'rgba(0,0,0,0.06)' },
            ticks: {
                color: '#374151',
                callback: (value: number | string) => `$${Number(value).toFixed(2)}`,
            },
        },
    },
}));
</script>

<template>
    <div class="bg-white rounded p-4">
        <h2 class="text-xl font-bold mb-4">Set Value History</h2>
        <ClientOnly>
            <template #fallback>
                <div class="h-64 flex items-center justify-center text-gray-400">Loading...</div>
            </template>
            <div v-if="isLoading" class="h-64 flex items-center justify-center text-gray-400">
                Loading...
            </div>
            <p v-else-if="!history.length" class="text-gray-500">
                No price history available.
            </p>
            <div v-else style="height: 200px;">
                <Line :data="chartData" :options="(chartOptions as any)" />
            </div>
        </ClientOnly>
    </div>
</template>
