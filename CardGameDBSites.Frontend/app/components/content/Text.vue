<script setup lang="ts">
import type { TextPropertiesModel } from '~/api/umbraco';

const props = defineProps<{
  content: TextPropertiesModel;
}>();
</script>

<template>
    <div v-html="content.content?.markup"></div>
</template>