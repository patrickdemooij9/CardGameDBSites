<script setup lang="ts">
import BaseDeckOverview from "./BaseDeckOverview.vue";

defineProps<{
  decksPerRow: number;
}>();
</script>

<template>
  <BaseDeckOverview :decks-per-row="decksPerRow"></BaseDeckOverview>
</template>
