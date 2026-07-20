<script setup lang="ts">
import type { SquadSettingsOptionApiModel } from "~/api/default";
import CmsImage from "~/components/shared/CmsImage.vue";

defineProps<{
  options: SquadSettingsOptionApiModel[];
}>();

const emit = defineEmits<{
  select: [option: SquadSettingsOptionApiModel];
}>();
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <div class="max-w-3xl mb-8">
      <h1
        class="text-3xl md:text-4xl font-extrabold text-gray-900 mb-3"
      >
        Build your own deck, your way
      </h1>
      <p class="text-gray-600 text-base md:text-lg">
        Choose the format you would like to build a deck for and get started
        with your deckbuilding journey. Each format has its own unique rules and
        card pool, so choose wisely and let your creativity run wild!
      </p>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
      <button
        v-for="option in options"
        :key="option.id"
        type="button"
        class="group relative text-left rounded-2xl border border-gray-200/80 bg-white/90 p-5 shadow-sm hover:shadow-xl hover:-translate-y-1 hover:border-main-color/50 transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-main-color focus-visible:ring-offset-2"
        @click="emit('select', option)"
      >
        <div class="relative z-10 flex items-start gap-4">
          <div
            class="shrink-0 w-20 h-20 rounded-xl border border-gray-200 flex items-center justify-center overflow-hidden"
          >
            <CmsImage
              v-if="option.imageUrl"
              :src="option.imageUrl"
              :alt="option.name"
              class="w-full h-full rounded-md"
            />
          </div>

          <div class="flex-1">
            <h2
              class="text-lg font-bold text-gray-900 mb-1 group-hover:text-main-color transition-colors"
            >
              {{ option.name }}
            </h2>
            <p class="text-sm text-gray-600 min-h-[60px]">
              {{
                option.description
              }}
            </p>
          </div>
        </div>

        <div
          class="relative z-10 mt-4 inline-flex items-center gap-2 text-sm font-semibold text-main-color"
        >
          Start building
          <span
            class="transition-transform duration-300 group-hover:translate-x-1"
            >→</span
          >
        </div>
      </button>
    </div>
  </div>
</template>
