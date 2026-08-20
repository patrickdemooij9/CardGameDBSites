<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    value?: number | null;
    isNew?: boolean;
    unavailableReason?: string | null;
    threshold?: number;
  }>(),
  { value: null, isNew: false, unavailableReason: null, threshold: 0.5 }
);

const direction = computed(() => {
  if (props.value == null) return "none";
  if (props.value > props.threshold) return "up";
  if (props.value < -props.threshold) return "down";
  return "flat";
});

const formatted = computed(() => {
  if (props.value == null) return "—";
  const rounded = Math.abs(props.value).toFixed(1);
  if (direction.value === "flat") return "±0";
  return `${props.value > 0 ? "+" : "-"}${rounded}`;
});

const label = computed(() => {
  if (props.isNew) return "New this week";
  if (props.value == null) {
    return props.unavailableReason ?? "Not enough data in one of the two weeks to show movement";
  }
  const pts = `${Math.abs(props.value).toFixed(1)} percentage points`;
  if (direction.value === "up") return `Meta share up ${pts}`;
  if (direction.value === "down") return `Meta share down ${pts}`;
  return "Meta share broadly unchanged";
});
</script>

<template>
  <span
    v-if="isNew"
    class="inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs font-semibold text-blue-800"
    :title="label"
  >
    New
    <span class="sr-only">{{ label }}</span>
  </span>
  <span
    v-else
    class="inline-flex items-center gap-1 text-sm font-semibold tabular-nums"
    :class="{
      'text-green-700': direction === 'up',
      'text-red-700': direction === 'down',
      'text-gray-500': direction === 'flat',
      'text-gray-400': direction === 'none',
    }"
    :title="label"
  >
    <span aria-hidden="true">
      <template v-if="direction === 'up'">▲</template>
      <template v-else-if="direction === 'down'">▼</template>
      <template v-else-if="direction === 'flat'">-</template>
    </span>
    {{ formatted }}
    <span class="sr-only">{{ label }}</span>
  </span>
</template>
