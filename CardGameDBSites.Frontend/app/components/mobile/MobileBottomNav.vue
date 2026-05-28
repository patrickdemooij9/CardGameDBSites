<script setup lang="ts">
/**
 * MobileBottomNav - Native-style bottom tab navigation for the mobile app.
 * 
 * Uses fixed positioning with safe-area insets to handle notched devices.
 * Includes haptic feedback on tab selection for a native feel.
 */
import { PhHouse, PhCards, PhMagnifyingGlass, PhUser, PhStack } from '@phosphor-icons/vue';
import { useHaptics } from '~/composables/mobile/useHaptics';

const route = useRoute();
const { impact, ImpactStyle } = useHaptics();

interface TabItem {
  label: string;
  icon: any;
  path: string;
}

const tabs: TabItem[] = [
  { label: 'Home', icon: PhHouse, path: '/' },
  { label: 'Cards', icon: PhCards, path: '/cards' },
  { label: 'Search', icon: PhMagnifyingGlass, path: '/search' },
  { label: 'Decks', icon: PhStack, path: '/decks' },
  { label: 'Profile', icon: PhUser, path: '/profile' },
];

function isActive(tab: TabItem): boolean {
  if (tab.path === '/') return route.path === '/';
  return route.path.startsWith(tab.path);
}

async function onTabTap(tab: TabItem) {
  await impact(ImpactStyle.Light);
  navigateTo(tab.path);
}
</script>

<template>
  <nav
    class="mobile-bottom-nav fixed bottom-0 left-0 right-0 z-50 bg-white border-t border-gray-200 dark:bg-gray-900 dark:border-gray-700"
    aria-label="Mobile navigation"
  >
    <div class="flex justify-around items-center h-16 pb-safe">
      <button
        v-for="tab in tabs"
        :key="tab.path"
        class="flex flex-col items-center justify-center flex-1 h-full pt-2 transition-colors duration-150"
        :class="isActive(tab) ? 'text-blue-600 dark:text-blue-400' : 'text-gray-500 dark:text-gray-400'"
        :aria-current="isActive(tab) ? 'page' : undefined"
        :aria-label="tab.label"
        @click="onTabTap(tab)"
      >
        <component :is="tab.icon" :size="24" :weight="isActive(tab) ? 'fill' : 'regular'" />
        <span class="text-xs mt-1 font-medium">{{ tab.label }}</span>
      </button>
    </div>
  </nav>
</template>

<style scoped>
.mobile-bottom-nav {
  /* Ensure safe area padding on devices with home indicator */
  padding-bottom: env(safe-area-inset-bottom, 0px);
}
</style>
