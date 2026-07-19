<script setup lang="ts">
import { PhX } from "@phosphor-icons/vue";
import type { OverviewFilterModel } from "../OverviewFilterModel";

const props = defineProps<{
  overviewState: ReturnType<typeof useOverviewState>;
}>();

function displayValue(filter: OverviewFilterModel, value: string) {
  const item = filter.Items.find((i) => i.Value === value);
  return item?.DisplayName ?? value;
}

function remove(filter: OverviewFilterModel, value: string) {
  props.overviewState.selectFilter(filter, value);
}
</script>

<template>
  <div class="flex flex-wrap items-center gap-2">
    <template
      v-for="[filter, values] in overviewState.state.selectedFilters"
      :key="filter.Alias"
    >
      <div
        v-for="value in values"
        :key="`${filter.Alias}-${value}`"
        class="flex items-center gap-2 rounded-md border-2 border-main-color p-1"
      >
        <p class="text-xs">
          <span>{{ filter.DisplayName }}</span>
          :
          <span>{{ displayValue(filter, value) }}</span>
        </p>
        <button
          type="button"
          :aria-label="`Remove ${filter.DisplayName} filter`"
          @click="remove(filter, value)"
        >
          <PhX class="items-center" />
        </button>
      </div>
    </template>
  </div>
</template>
