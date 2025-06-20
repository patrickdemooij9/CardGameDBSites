<script setup lang="ts">
import type { DeckOverviewContentModel } from "~/api/umbraco";
import DeckOverview from "../overviews/DeckOverview.vue";

defineProps<{
  content: DeckOverviewContentModel;
}>();
</script>

<template>
  <div class="container px-4 md:px-8 mb-16">
    <div class="row justify-center">
      <div class="section-title">
        <h1>{{ content.properties?.title }}</h1>
        <p v-html="content.properties?.description"></p>
      </div>
    </div>

    <DeckOverview
      :decks-per-row="content.properties?.decksPerRow ?? 4"
    ></DeckOverview>
  </div>
</template>
