<script setup lang="ts">
import type { HomepageContentModel } from '~/api/umbraco';
import ContentGrid from '../content/ContentGrid.vue';

const props = defineProps<{
    content: HomepageContentModel
}>();
</script>

<template>
    <ContentGrid :content="props.content.properties!.grid!"></ContentGrid>
</template>