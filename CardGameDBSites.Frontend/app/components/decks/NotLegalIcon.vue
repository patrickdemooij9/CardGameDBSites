<script setup lang="ts">
import { PhProhibit } from "@phosphor-icons/vue";

withDefaults(
  defineProps<{
    title?: string;
  }>(),
  {
    title: "Not legal: contains cards from sets that rotated out of this format",
  },
);
</script>

<template>
  <span
    class="inline-flex shrink-0 text-red-600 dark:text-red-400"
    role="img"
    :title="title"
    :aria-label="title"
  >
    <PhProhibit :size="16" weight="bold" />
  </span>
</template>
