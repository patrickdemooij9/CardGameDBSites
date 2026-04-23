<script setup lang="ts">
import { DoServerFetch } from '~/helpers/RequestsHelper';

export type CardPriceChangeApiModel = {
    cardId: number;
    variantId: number | null;
    cardName: string;
    urlSegment: string;
    currentPrice: number;
    previousPrice: number;
    priceChange: number;
    priceChangePercent: number;
};

const props = defineProps<{
    count?: number;
}>();

const count = props.count ?? 3;

const { data: increased } = await useAsyncData('price-changes-increased', () =>
    DoServerFetch<CardPriceChangeApiModel[]>(`/api/cards/topPriceChanges?count=${count}&descending=true`, true)
);

const { data: decreased } = await useAsyncData('price-changes-decreased', () =>
    DoServerFetch<CardPriceChangeApiModel[]>(`/api/cards/topPriceChanges?count=${count}&descending=false`, true)
);

const formatPrice = (price: number) => `$${price.toFixed(2)}`;
const formatChange = (change: number) => {
    const sign = change >= 0 ? '+' : '';
    return `${sign}$${change.toFixed(2)}`;
};
const formatPercent = (percent: number) => {
    const sign = percent >= 0 ? '+' : '';
    return `${sign}${percent.toFixed(2)}%`;
};
</script>

<template>
    <div v-if="(increased && increased.length > 0) || (decreased && decreased.length > 0)" class="bg-white rounded p-6">
        <h2 class="text-xl font-bold mb-4">Price Changes (Last 24 Hours)</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div v-if="increased && increased.length > 0">
                <h3 class="text-lg font-semibold mb-2 text-green-600">Top Risers</h3>
                <table class="w-full text-sm border-collapse">
                    <thead>
                        <tr class="border-b">
                            <th class="text-left py-1 pr-4">Card</th>
                            <th class="text-right py-1 pr-4">Price</th>
                            <th class="text-right py-1">Change</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="card in increased" :key="`inc-${card.cardId}-${card.variantId}`"
                            class="border-b last:border-0">
                            <td class="py-2 pr-4">
                                <a :href="`${card.urlSegment}`" class="hover:underline">{{ card.cardName }}</a>
                            </td>
                            <td class="text-right py-2 pr-4">{{ formatPrice(card.currentPrice) }}</td>
                            <td class="text-right py-2 text-green-600">
                                {{ formatChange(card.priceChange) }}
                                ({{ formatPercent(card.priceChangePercent) }})
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div v-if="decreased && decreased.length > 0">
                <h3 class="text-lg font-semibold mb-2 text-red-600">Top Fallers</h3>
                <table class="w-full text-sm border-collapse">
                    <thead>
                        <tr class="border-b">
                            <th class="text-left py-1 pr-4">Card</th>
                            <th class="text-right py-1 pr-4">Price</th>
                            <th class="text-right py-1">Change</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="card in decreased" :key="`dec-${card.cardId}-${card.variantId}`"
                            class="border-b last:border-0">
                            <td class="py-2 pr-4">
                                <a :href="`${card.urlSegment}`" class="hover:underline">{{ card.cardName }}</a>
                            </td>
                            <td class="text-right py-2 pr-4">{{ formatPrice(card.currentPrice) }}</td>
                            <td class="text-right py-2 text-red-600">
                                {{ formatChange(card.priceChange) }}
                                ({{ formatPercent(card.priceChangePercent) }})
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
