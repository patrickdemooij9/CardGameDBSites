<script setup lang="ts">
import type { FaqEntry } from "./faq";

withDefaults(
  defineProps<{
    entries: FaqEntry[];
    heading?: string;
    anchorId?: string;
  }>(),
  { heading: "Frequently asked questions", anchorId: "faq" }
);
</script>

<template>
  <section v-if="entries.length > 0" :id="anchorId">
    <h2 class="mb-6">{{ heading }}</h2>
    <div class="space-y-3 max-w-3xl">
      <details
        v-for="entry in entries"
        :key="entry.question"
        class="group border border-gray-200 rounded-lg bg-white px-4 py-3"
      >
        <summary
          class="cursor-pointer list-none marker:content-none font-semibold text-gray-900 flex items-start justify-between gap-4"
        >
          <span>{{ entry.question }}</span>
          <span
            class="shrink-0 text-gray-400 leading-none text-xl transition-transform group-open:rotate-45"
            aria-hidden="true"
            >+</span
          >
        </summary>
        <p class="text-gray-700 pt-3 mb-0 whitespace-pre-line">{{ entry.answer }}</p>
      </details>
    </div>
  </section>
</template>
