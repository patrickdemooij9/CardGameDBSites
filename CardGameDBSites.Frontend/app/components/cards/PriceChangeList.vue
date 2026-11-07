<script setup lang="ts">
import { DoServerFetch } from '~/helpers/RequestsHelper';
import type { CardPriceChangeApiModel } from '~/models/CardPriceChangeApiModel';
import SectionHeader from '../shared/SectionHeader.vue';
import PriceChangeTable from './PriceChangeTable.vue';

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
</script>

<template>
    <div v-if="increased?.length || decreased?.length">
        <SectionHeader title="Price changes" subtitle="last 24 hours" />
        <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
            <PriceChangeTable v-if="increased?.length" title="Top risers" :changes="increased" rising />
            <PriceChangeTable v-if="decreased?.length" title="Top fallers" :changes="decreased" :rising="false" />
        </div>
    </div>
</template>
