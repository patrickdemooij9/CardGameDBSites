<script setup lang="ts">
import type { SquadSettingsOptionApiModel } from '~/api/default';

defineProps<{
  options: SquadSettingsOptionApiModel[];
}>();

const emit = defineEmits<{
  select: [option: SquadSettingsOptionApiModel];
}>();
</script>

<template>
  <div class="container py-8">
    <h1 class="text-2xl font-bold mb-6">Choose a deck type</h1>
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
      <button
        v-for="option in options"
        :key="option.id"
        type="button"
        class="flex flex-col items-center border rounded-lg p-6 hover:border-main-color hover:shadow-md transition-all text-left cursor-pointer bg-white"
        @click="emit('select', option)"
      >
        <img
          v-if="option.imageUrl"
          :src="option.imageUrl"
          :alt="option.name"
          class="w-32 h-32 object-contain mb-4"
        />
        <div v-else class="w-32 h-32 bg-gray-100 rounded-lg mb-4 flex items-center justify-center">
          <span class="text-gray-400 text-4xl">🃏</span>
        </div>
        <h2 class="text-lg font-semibold mb-2">{{ option.name }}</h2>
        <p v-if="option.description" class="text-sm text-gray-600 text-center">{{ option.description }}</p>
      </button>
    </div>
  </div>
</template>
