<script setup lang="ts">
import type { SetOverviewContentModel } from "~/api/umbraco";
import { useAllSets } from "~/composables/useAllSets";
import SetOverview from "../overviews/SetOverview.vue";
import SetCollectionSummary from "../sets/SetCollectionSummary.vue";

defineProps<{
  content: SetOverviewContentModel;
}>();

const { data: sets } = await useAllSets();
</script>

<template>
  <div class="container flex flex-col gap-6 px-4 pt-8 md:flex-row md:items-start md:justify-between md:px-8 mb-8">
    <div>
      <h1>{{ content.properties?.title ?? "Sets" }}</h1>
      <span
        v-if="content.properties?.description"
        class="text-gray-600"
        v-html="content.properties?.description.markup"
      ></span>
    </div>
    <SetCollectionSummary :sets="sets" />
  </div>

  <div class="container px-4 md:px-8 pb-12">
    <SetOverview />
  </div>
</template>
