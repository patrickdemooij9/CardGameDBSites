<script setup lang="ts">
import type { ContentPageContentModel } from '~/api/umbraco';
import ContentGrid from '../content/ContentGrid.vue';

const props = defineProps<{
    content: ContentPageContentModel
}>();
</script>

<template>
    <div class="pt-8">
        <ContentGrid v-if="props.content.properties?.grid" :content="props.content.properties.grid"></ContentGrid>
    </div>
</template>
