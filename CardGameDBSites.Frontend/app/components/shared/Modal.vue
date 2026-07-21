<script setup lang="ts">
import { PhX } from "@phosphor-icons/vue";

withDefaults(
  defineProps<{
    title?: string;
    /**
     * Stacking layer for the overlay. Defaults to `z-50`. Pass a higher layer
     * (e.g. `z-[60]`) when a modal must sit on top of another already-open modal.
     */
    zClass?: string;
  }>(),
  { zClass: "z-50" },
);

const emit = defineEmits<{ close: [] }>();
</script>

<template>
  <!-- The parent controls mounting via v-if, so the Teleport is only ever in the
       tree while the modal is open (mirrors DeckDetailCardModal) — this keeps it
       out of SSR and avoids hydration mismatches. -->
  <Teleport to="#root">
    <div
      class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-60"
      :class="zClass"
      role="dialog"
      aria-modal="true"
      @click.self="emit('close')"
    >
      <div
        class="relative bg-white rounded-lg shadow-lg w-full max-w-md mx-4 p-6"
      >
        <button
          class="absolute top-3 right-3 text-gray-400 hover:text-gray-700"
          aria-label="Close"
          @click="emit('close')"
        >
          <PhX class="h-5 w-5" />
        </button>
        <h2 v-if="title" class="text-lg font-semibold mb-4">{{ title }}</h2>
        <slot></slot>
      </div>
    </div>
  </Teleport>
</template>
