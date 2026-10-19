<script setup lang="ts">
import type { TextPropertiesModel } from '~/api/umbraco';

const props = defineProps<{
  content: TextPropertiesModel;
}>();

const markup = computed(() =>
  props.content.content?.markup?.replace(/<p>(?:\s|&nbsp;|<br\s*\/?>)*<\/p>/g, '<p class="rte-spacer">&nbsp;</p>')
);
</script>

<template>
    <div v-html="markup"></div>
</template>
