<script setup lang="ts">
import type { SetOverviewContentModel } from "~/api/umbraco";
import SetOverview from "../overviews/SetOverview.vue";

defineProps<{
  content: SetOverviewContentModel;
}>();
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <h1>{{ content.properties?.title ?? "Sets" }}</h1>
    <span
      v-if="content.properties?.description"
      v-html="content.properties?.description.markup"
    ></span>
  </div>

  <div class="container px-4 md:px-8">
    <SetOverview/>
  </div>
</template>
