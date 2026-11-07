<script setup lang="ts">
import type { SiteSettingsApiModel } from "~/api/default";

defineProps<{
  settings: SiteSettingsApiModel;
}>();
</script>

<template>
  <footer class="mt-auto border-t border-line bg-canvas text-ink">
    <div class="container flex flex-col gap-8 px-4 py-10 md:flex-row md:justify-between md:px-8">
      <div class="md:max-w-md">
        <p class="mb-2 text-base font-bold">{{ settings.siteName }}</p>
        <p class="text-xs leading-relaxed text-ink-muted">{{ settings.footerText }}</p>
      </div>
      <div class="flex gap-16 text-sm">
        <nav v-if="settings.navigation?.length" :aria-label="settings.siteName">
          <p class="mb-2 font-semibold">{{ settings.siteName }}</p>
          <ul class="ml-0 flex list-none flex-col gap-1.5">
            <li v-for="item in settings.navigation" :key="item.url">
              <NuxtLink :to="item.url" class="text-ink-muted no-underline hover:text-ink">
                {{ item.name }}
              </NuxtLink>
            </li>
          </ul>
        </nav>
        <nav v-if="settings.footerLinks?.length" aria-label="Other sites">
          <p class="mb-2 font-semibold">Other sites</p>
          <ul class="ml-0 flex list-none flex-col gap-1.5">
            <li v-for="link in settings.footerLinks" :key="`${link.name}-${link.url}`">
              <a
                :href="link.url"
                :target="link.target ?? undefined"
                :rel="link.target === '_blank' ? 'noopener noreferrer' : undefined"
                class="text-ink-muted no-underline hover:text-ink"
              >
                {{ link.name }}
              </a>
            </li>
          </ul>
        </nav>
      </div>
    </div>
  </footer>
</template>
