<script setup lang="ts">
/**
 * Mobile layout - Used when the app runs inside Capacitor native shell.
 * 
 * Key differences from default layout:
 * - No desktop navigation bar
 * - Bottom tab navigation instead
 * - Safe area insets for notched devices
 * - No footer (mobile apps don't have website footers)
 * - Extra bottom padding to account for the bottom nav bar
 * 
 * This layout is selected via the `usePlatform` composable or can be
 * explicitly set on a per-page basis using `definePageMeta({ layout: 'mobile' })`.
 */
import MobileBottomNav from '~/components/mobile/MobileBottomNav.vue';
import MobileHeader from '~/components/mobile/MobileHeader.vue';
import { useSite } from '~/composables/useSite';

const { getSettings } = useSite();
const siteSettings = await getSettings();
</script>

<template>
  <div
    :style="{
      '--main-color': siteSettings.mainColor,
      '--main-color-hover': siteSettings.hoverMainColor,
    }"
    class="min-h-screen flex flex-col bg-gray-50 dark:bg-gray-950"
    id="root"
  >
    <MobileHeader :title="siteSettings.siteName ?? ''" />

    <!-- Main content area with padding for bottom nav -->
    <main class="flex-1 pb-20 overflow-y-auto">
      <slot />
    </main>

    <MobileBottomNav />
  </div>
</template>

<style>
/* Global mobile-specific styles */
.mobile-page-enter-active,
.mobile-page-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.mobile-page-enter-from {
  opacity: 0;
  transform: translateX(10px);
}

.mobile-page-leave-to {
  opacity: 0;
  transform: translateX(-10px);
}
</style>
