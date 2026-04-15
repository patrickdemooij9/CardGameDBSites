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
import type { CardPriceHistoryItemApiModel } from '~/api/default/models/CardPriceHistoryItemApiModel';

ChartJS.register(Title, Tooltip, Legend, LineElement, PointElement, CategoryScale, LinearScale, Filler);

const props = defineProps<{
    cardId: number;
    variantId?: number | null;
}>();

// Hardcoded waypoints - to be connected to set release dates later
const waypoints: { date: string; label: string }[] = [
    // Example: { date: '2025-06-01', label: 'Set X' }
];

const history = ref<CardPriceHistoryItemApiModel[]>([]);
const isLoading = ref(true);

onMounted(async () => {
    try {
        const query = props.variantId != null
            ? `/api/cards/priceHistory?cardId=${props.cardId}&variantId=${props.variantId}`
            : `/api/cards/priceHistory?cardId=${props.cardId}`;
        history.value = await DoServerFetch<CardPriceHistoryItemApiModel[]>(query, true);
    } finally {
        isLoading.value = false;
    }
});

function expandPriceHistory(records: CardPriceHistoryItemApiModel[]) {
    if (!records.length) return { labels: [] as string[], data: [] as number[] };

    const sorted = [...records].sort((a, b) => a.date.localeCompare(b.date));
    const start = new Date(sorted[0].date + 'T00:00:00Z');
    const now = new Date();
    const today = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate()));

    const labels: string[] = [];
    const data: number[] = [];

    let currentPrice = sorted[0].price;
    let recordIndex = 0;

    const current = new Date(start);
    while (current <= today) {
        const dateStr = current.toISOString().split('T')[0];

        // Advance through all records up to and including this date
        while (recordIndex < sorted.length && sorted[recordIndex].date <= dateStr) {
            currentPrice = sorted[recordIndex].price;
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
            label: 'Price',
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

const waypointPlugin = {
    id: 'priceHistoryWaypoints',
    afterDraw(chart: any) {
        if (!waypoints.length) return;
        const { ctx, chartArea, scales } = chart;
        const labels = chart.data.labels as string[];

        for (const wp of waypoints) {
            const index = labels.indexOf(wp.date);
            if (index === -1) continue;

            const x = scales.x.getPixelForValue(index);

            ctx.save();
            ctx.setLineDash([4, 4]);
            ctx.strokeStyle = '#94a3b8';
            ctx.lineWidth = 1;
            ctx.beginPath();
            ctx.moveTo(x, chartArea.top);
            ctx.lineTo(x, chartArea.bottom);
            ctx.stroke();

            ctx.setLineDash([]);
            ctx.fillStyle = '#64748b';
            ctx.font = 'bold 11px sans-serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'bottom';
            ctx.fillText(wp.label, x, chartArea.top - 2);
            ctx.restore();
        }
    },
};

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
                    const date = new Date(Date.UTC(year, month - 1, day));
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
    <div class="bg-white rounded p-4 mt-8">
        <h2 class="text-xl font-bold mb-4">Price History</h2>
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
            <div v-else style="height: 300px;">
                <Line :data="chartData" :options="(chartOptions as any)" :plugins="[waypointPlugin]" />
            </div>
        </ClientOnly>
    </div>
</template>
