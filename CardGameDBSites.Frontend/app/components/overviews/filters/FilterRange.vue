<script setup lang="ts">
import type { OverviewFilterModel } from "../OverviewFilterModel";
import {
  GetRangeBounds,
  ParseRangeValue,
  ToRangeValue,
} from "~/helpers/RangeFilterHelper";

const props = defineProps<{
  overviewState: ReturnType<typeof useOverviewState>;
  filter: OverviewFilterModel;
}>();

const bounds = computed(() =>
  GetRangeBounds(props.filter.Items.map((item) => item.Value)),
);

const selected = computed(() => {
  const value = props.overviewState.getFilterValue(props.filter);
  return value ? ParseRangeValue(value) : undefined;
});

const low = ref(0);
const high = ref(0);

watch(
  [bounds, selected],
  () => {
    if (!bounds.value) return;
    low.value = selected.value?.min ?? bounds.value.min;
    high.value = selected.value?.max ?? bounds.value.max;
  },
  { immediate: true },
);

const fillStyle = computed(() => {
  if (!bounds.value) return {};
  const span = Math.max(bounds.value.max - bounds.value.min, 1);
  const left = ((low.value - bounds.value.min) / span) * 100;
  const right = ((high.value - bounds.value.min) / span) * 100;
  return { left: `${left}%`, width: `${right - left}%` };
});

// Overlapping thumbs at the max would otherwise trap the min thumb under the max one.
const lowOnTop = computed(
  () => !!bounds.value && low.value > (bounds.value.min + bounds.value.max) / 2,
);

function onLowInput(event: Event) {
  low.value = Math.min(Number((event.target as HTMLInputElement).value), high.value);
  (event.target as HTMLInputElement).value = low.value.toString();
}

function onHighInput(event: Event) {
  high.value = Math.max(Number((event.target as HTMLInputElement).value), low.value);
  (event.target as HTMLInputElement).value = high.value.toString();
}

function commit() {
  if (!bounds.value) return;
  if (low.value === bounds.value.min && high.value === bounds.value.max) {
    props.overviewState.clearFilter(props.filter);
    return;
  }
  props.overviewState.setFilter(
    props.filter,
    ToRangeValue({ min: low.value, max: high.value }),
  );
}
</script>

<template>
  <p v-if="filter.Loading" class="text-sm text-gray-500 px-1 py-2">Loading…</p>
  <p v-else-if="!bounds" class="text-sm text-gray-500 px-1 py-2">
    No numeric values available.
  </p>
  <div v-else class="flex flex-col gap-2 px-1">
    <div class="flex justify-between text-sm font-semibold">
      <span>{{ low === high ? low : `${low} – ${high}` }}</span>
      <span class="text-gray-500 font-normal">
        {{ bounds.min }} – {{ bounds.max }}
      </span>
    </div>
    <div class="relative h-6">
      <div class="absolute top-1/2 -translate-y-1/2 h-1.5 w-full rounded bg-gray-200" />
      <div
        class="absolute top-1/2 -translate-y-1/2 h-1.5 rounded bg-main-color"
        :style="fillStyle"
      />
      <input
        type="range"
        class="range-thumb"
        :min="bounds.min"
        :max="bounds.max"
        step="1"
        :value="low"
        :class="{ 'z-10': lowOnTop }"
        :aria-label="`Minimum ${filter.DisplayName}`"
        @input="onLowInput"
        @change="commit"
      />
      <input
        type="range"
        class="range-thumb"
        :min="bounds.min"
        :max="bounds.max"
        step="1"
        :value="high"
        :aria-label="`Maximum ${filter.DisplayName}`"
        @input="onHighInput"
        @change="commit"
      />
    </div>
  </div>
</template>

<style scoped>
.range-thumb {
  position: absolute;
  inset: 0;
  width: 100%;
  appearance: none;
  background: transparent;
  pointer-events: none;
}

.range-thumb::-webkit-slider-thumb {
  appearance: none;
  pointer-events: auto;
  width: 1.25rem;
  height: 1.25rem;
  border-radius: 9999px;
  background: white;
  border: 2px solid var(--main-color);
  cursor: pointer;
}

.range-thumb::-moz-range-thumb {
  pointer-events: auto;
  width: 1.25rem;
  height: 1.25rem;
  border-radius: 9999px;
  background: white;
  border: 2px solid var(--main-color);
  cursor: pointer;
}
</style>
