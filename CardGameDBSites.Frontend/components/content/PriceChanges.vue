<script setup lang="ts">
import type { PriceChangesPropertiesModel } from '~/api/umbraco';
import PriceChangeList from '../cards/PriceChangeList.vue';

defineProps<{
    content: PriceChangesPropertiesModel
}>();
</script>

<template>
    <PriceChangeList :count="content.maxItems ?? 3"></PriceChangeList>
</template>