<script setup lang="ts">
import type { HomepageContentModel } from '~/api/umbraco';
import ContentGrid from '../content/ContentGrid.vue';

const props = defineProps<{
    content: HomepageContentModel
}>();
</script>

<template>
    <!-- Own wrapper so the layout's loading indicator doesn't shift the even/odd band colors -->
    <div>
        <ContentGrid :content="props.content.properties!.grid!" banded></ContentGrid>
    </div>
</template>