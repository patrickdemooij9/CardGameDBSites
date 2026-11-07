<script setup lang="ts">
defineProps<{
  title: string;
  subtitle?: string;
}>();
</script>

<template>
  <div class="mb-6 flex flex-wrap items-center justify-between gap-4">
    <h2 class="text-2xl font-bold tracking-tight text-ink">
      {{ title }}
      <span v-if="subtitle" class="ml-1 text-sm font-normal text-ink-muted">{{ subtitle }}</span>
    </h2>
    <slot></slot>
  </div>
</template>
