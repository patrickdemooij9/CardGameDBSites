<script setup lang="ts">
import type { SetViewModel } from "~/api/default";
import { useCollection } from "~/composables/useCollection";
import { GetProgressPercent } from "~/helpers/SetHelper";
import ProgressBar from "~/components/shared/ProgressBar.vue";

const props = defineProps<{
  sets: SetViewModel[];
}>();

const { getSetsSummary } = useCollection();
const summary = computed(() => getSetsSummary(props.sets));
</script>

<template>
  <div v-if="summary" class="flex w-full flex-col gap-1 rounded-lg border border-gray-200 bg-surface p-4 shadow-sm md:w-80">
    <span class="text-xs font-medium text-gray-700">Your collection</span>
    <ProgressBar
      :percent-filled="GetProgressPercent(summary.owned, summary.total)"
      :description="`${summary.owned.toLocaleString('en-US')} of ${summary.total.toLocaleString('en-US')} ${summary.unit}`"
    />
  </div>
</template>
