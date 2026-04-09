<script setup lang="ts">
import type { DeckOverviewContentModel } from "~/api/umbraco";
import DeckOverview from "../overviews/DeckOverview.vue";

const props = defineProps<{
  content: DeckOverviewContentModel;
}>();

const typeId: number | undefined = props.content.properties?.squadSettingsId ?? undefined;
</script>

<template>
  <div class="container px-4 py-4 md:px-8 mb-16">
    <div class="row justify-center">
      <div class="section-title">
        <h1>{{ content.properties?.title }}</h1>
        <div v-if="content.properties?.description" v-html="content.properties?.description?.markup"></div>
      </div>
    </div>

    <DeckOverview
      :decks-per-row="content.properties?.decksPerRow ?? 4"
      :type-id="typeId"
    ></DeckOverview>
  </div>
</template>
