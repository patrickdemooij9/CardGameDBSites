<script setup lang="ts">
import { PhArrowSquareOut } from "@phosphor-icons/vue";
import { useSite } from "~/composables/useSite";

definePageMeta({ middleware: "native-only" });

const siteSettings = await useSite().getSettings();

useHead({ title: "About" });
</script>

<template>
  <div class="pb-8">
    <div class="bg-white border-b border-gray-200 px-4 py-4">
      <h1 class="text-xl">About</h1>
      <p class="mt-3 text-sm text-gray-600">{{ siteSettings.footerText }}</p>
    </div>

    <div
      v-if="siteSettings.footerLinks?.length"
      class="mt-4 bg-white border-y border-gray-200"
    >
      <p class="px-4 pt-3 text-xs uppercase tracking-wide text-gray-500">Other sites</p>
      <a
        v-for="link in siteSettings.footerLinks"
        :key="`${link.name}-${link.url}`"
        :href="link.url"
        target="_blank"
        rel="noopener noreferrer"
        class="flex items-center justify-between px-4 py-3 no-underline"
      >
        {{ link.name }}
        <PhArrowSquareOut :size="16" class="text-gray-400" />
      </a>
    </div>
  </div>
</template>
