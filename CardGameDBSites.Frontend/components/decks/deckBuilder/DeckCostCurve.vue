<script setup lang="ts">
import type { CreateDeckModel } from "./models/CreateDeckModel";
import { computeCostCurve } from "~/helpers/CostCurveHelper";

const props = defineProps<{
  deck: CreateDeckModel;
  costAttribute?: string;
}>();

const costCurve = computed(() =>
  computeCostCurve(props.deck, props.costAttribute ?? "Cost"),
);
</script>

<template>
  <div v-if="costCurve.length > 0" class="mt-4">
    <h3 class="text-sm font-semibold mb-2">Cost Curve</h3>
    <div class="flex items-end gap-1 h-24 bg-white border border-gray-200 rounded px-2 pt-2 pb-6 relative">
      <div
        v-for="entry in costCurve"
        :key="entry.label"
        class="flex flex-col items-center flex-1 min-w-0 h-full justify-end"
      >
        <span class="text-xs text-gray-600 mb-0.5 leading-none">{{ entry.count }}</span>
        <div
          class="w-full bg-main-color rounded-t min-h-[2px]"
          :style="{ height: entry.heightPercent + '%' }"
        ></div>
      </div>
      <!-- X-axis labels positioned absolutely below the bars -->
      <div class="absolute bottom-1 left-2 right-2 flex gap-1">
        <div
          v-for="entry in costCurve"
          :key="'label-' + entry.label"
          class="flex-1 text-center text-xs text-gray-500 truncate"
        >
          {{ entry.label }}
        </div>
      </div>
    </div>
  </div>
</template>
