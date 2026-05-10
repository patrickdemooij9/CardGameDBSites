<script setup lang="ts">
import type { SquadSettingsOptionApiModel } from '~/api/default';

defineProps<{
  options: SquadSettingsOptionApiModel[];
}>();

const emit = defineEmits<{
  select: [option: SquadSettingsOptionApiModel];
}>();

const fallbackIcons = ['🃏', '⚡', '🔥', '🌙', '🛡️', '✨'];

function getFallbackIcon(index: number) {
  return fallbackIcons[index % fallbackIcons.length];
}
</script>

<template>
  <div class="relative overflow-hidden">
    <div class="absolute -top-20 -left-12 w-52 h-52 bg-purple-200/60 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -top-10 right-0 w-52 h-52 bg-cyan-200/60 rounded-full blur-3xl pointer-events-none"></div>

    <div class="container py-10 relative z-10">
      <div class="max-w-3xl mb-8">
        <p class="inline-flex items-center gap-2 text-xs font-semibold uppercase tracking-[0.2em] px-3 py-1 rounded-full bg-white/80 border border-main-color/20 text-gray-700 mb-4">
          <span>✨</span>
          Build your next favorite deck
        </p>
        <h1 class="text-3xl md:text-4xl font-extrabold text-gray-900 leading-tight mb-3">
          Pick a deck style to get started
        </h1>
        <p class="text-gray-600 text-base md:text-lg">
          Choose the vibe you want to play with — you can jump straight into building right after.
        </p>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
        <button
          v-for="(option, index) in options"
          :key="option.id"
          type="button"
          class="group relative text-left rounded-2xl border border-gray-200/80 bg-white/90 p-5 shadow-sm hover:shadow-xl hover:-translate-y-1 hover:border-main-color/50 transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-main-color focus-visible:ring-offset-2"
          @click="emit('select', option)"
        >
          <div class="absolute inset-0 rounded-2xl bg-gradient-to-br from-main-color/0 via-main-color/5 to-purple-200/20 opacity-0 group-hover:opacity-100 transition-opacity"></div>

          <div class="relative z-10 flex items-start gap-4">
            <div class="shrink-0 w-20 h-20 rounded-xl border border-gray-200 bg-gradient-to-br from-gray-50 to-white flex items-center justify-center overflow-hidden">
              <img
                v-if="option.imageUrl"
                :src="option.imageUrl"
                :alt="option.name"
                class="w-full h-full object-contain p-2 transition-transform duration-300 group-hover:scale-110"
              />
              <span v-else class="text-3xl transition-transform duration-300 group-hover:scale-110">{{ getFallbackIcon(index) }}</span>
            </div>

            <div class="min-w-0 flex-1">
              <h2 class="text-lg font-bold text-gray-900 mb-1 group-hover:text-main-color transition-colors">
                {{ option.name }}
              </h2>
              <p class="text-sm text-gray-600 line-clamp-3 min-h-[60px]">
                {{ option.description || 'A fun and unique deckbuilding format waiting for your strategy.' }}
              </p>
            </div>
          </div>

          <div class="relative z-10 mt-4 inline-flex items-center gap-2 text-sm font-semibold text-main-color">
            Start building
            <span class="transition-transform duration-300 group-hover:translate-x-1">→</span>
          </div>
        </button>
      </div>
    </div>
  </div>
</template>
