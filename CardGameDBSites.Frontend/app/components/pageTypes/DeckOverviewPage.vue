<script setup lang="ts">
import type { DeckOverviewContentModel } from "~/api/umbraco";
import DeckOverview from "../overviews/DeckOverview.vue";

const props = defineProps<{
  content: DeckOverviewContentModel;
}>();

const typeId: number | undefined =
  props.content.properties?.squadSettingsId ?? undefined;
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <h1>{{ content.properties?.title }}</h1>
    <span v-html="content.properties?.description?.markup"></span>
    <DeckOverview
    :decks-per-row="content.properties?.decksPerRow ?? 4"
    :type-id="typeId"
  ></DeckOverview>
  </div>
</template>
