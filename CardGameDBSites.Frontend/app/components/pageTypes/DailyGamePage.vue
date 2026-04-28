<script setup lang="ts">
import type { IApiContentModel } from "~/api/umbraco";
import DailyCardGame from "../shared/DailyCardGame.vue";

defineProps<{
  content: IApiContentModel;
}>();
</script>

<template>
  <div class="container px-4 md:px-8 py-8">
    <DailyCardGame />
  </div>
</template>
