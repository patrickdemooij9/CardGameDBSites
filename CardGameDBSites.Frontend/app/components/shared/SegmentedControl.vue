<script setup lang="ts" generic="T extends string">
const model = defineModel<T>({ required: true });

defineProps<{
  options: { value: T; label: string }[];
  label: string;
}>();
</script>

<template>
  <div role="group" :aria-label="label" class="inline-flex rounded-lg border border-line bg-canvas p-1 text-sm">
    <button
      v-for="option in options"
      :key="option.value"
      type="button"
      :aria-pressed="model === option.value"
      :class="model === option.value ? 'bg-surface text-ink shadow-sm' : 'text-ink-muted hover:text-ink'"
      class="rounded-md px-3 py-1 font-medium transition-colors"
      @click="model = option.value"
    >
      {{ option.label }}
    </button>
  </div>
</template>
