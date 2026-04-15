<script setup lang="ts">
import BaseDeckOverview from "./BaseDeckOverview.vue";

defineProps<{
  decksPerRow: number;
  typeId?: number;
  leaderCardId?: number;
}>();
</script>

<template>
  <BaseDeckOverview :decks-per-row="decksPerRow" :type-id="typeId" :leader-card-id="leaderCardId"></BaseDeckOverview>
</template>
