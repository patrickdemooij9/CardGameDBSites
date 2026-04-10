<script setup lang="ts">
import type { HomepageContentModel } from '~/api/umbraco';
import ContentGrid from '../content/ContentGrid.vue';
import PriceChangeList from '../cards/PriceChangeList.vue';
import { useSite } from '~/composables/useSite';

const props = defineProps<{
    content: HomepageContentModel
}>();

const { getSettings } = useSite();
const siteSettings = await getSettings();
</script>

<template>
    <PriceChangeList v-if="siteSettings.showPrices" :count="3"></PriceChangeList>
    <ContentGrid :content="props.content.properties!.grid!"></ContentGrid>
</template>