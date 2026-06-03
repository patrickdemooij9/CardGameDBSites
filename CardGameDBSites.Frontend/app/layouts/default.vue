<script setup lang="ts">
import Navigation from "~/components/navigation/Navigation.vue";
import ImpersonationBanner from "~/components/shared/ImpersonationBanner.vue";
import { useSite } from "~/composables/useSite";

const { getSettings, getNavigation } = useSite();
const siteSettings = await getSettings();
const navigationViewModel = await getNavigation();
</script>

<template>
  <div
    :style="{
      '--main-color': siteSettings.mainColor,
      '--main-color-hover': siteSettings.hoverMainColor,
      '--nav-border-color': siteSettings.mainColor,
    }"
    class="min-h-screen flex flex-col dark:bg-gray-900 dark:text-gray-200"
    id="root"
  >
    <ImpersonationBanner />
    <Navigation :content="navigationViewModel"> </Navigation>
    <div class="grow">
      <slot />
    </div>
    <footer
      :class="{
        'text-white': siteSettings.textColorWhite,
        'text-black': !siteSettings.textColorWhite,
      }"
      class="mt-auto md:flex px-4 md:px-8 bg-main-color"
    >
      <div class="py-4 md:w-2/3 text-sm">
        <p>{{ siteSettings.footerText }}</p>
      </div>
      <nav class="pl-4 py-4" aria-label="Other sites">
        <p class="font-bold">Other sites</p>
        <ul>
          <li v-for="link in siteSettings.footerLinks ?? []" :key="`${link.name}-${link.url}`">
            <a
              :href="link.url"
              :target="link.target ?? undefined"
              :rel="link.target === '_blank' ? 'noopener noreferrer' : undefined"
              class="no-underline"
            >
              {{ link.name }}
            </a>
          </li>
        </ul>
      </nav>
    </footer>
  </div>
</template>
