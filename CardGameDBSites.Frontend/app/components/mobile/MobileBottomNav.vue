<script setup lang="ts">
/**
 * MobileBottomNav - Native-style bottom tab navigation for the mobile app.
 * 
 * Uses fixed positioning with safe-area insets to handle notched devices.
 * Includes haptic feedback on tab selection for a native feel.
 * 
 * Icons are rendered directly in the template (not via dynamic `component :is`)
 * to ensure they display correctly on real devices with Capacitor builds.
 */
import { PhHouse, PhCards, PhMagnifyingGlass, PhUser, PhStack } from '@phosphor-icons/vue';
import { useHaptics } from '~/composables/mobile/useHaptics';

const route = useRoute();
const { impact, ImpactStyle } = useHaptics();

interface TabItem {
  label: string;
  path: string;
}

const tabs: TabItem[] = [
  { label: 'Home', path: '/' },
  { label: 'Cards', path: '/cards' },
  { label: 'Search', path: '/search' },
  { label: 'Decks', path: '/decks' },
  { label: 'Profile', path: '/profile' },
];

const homeTab = tabs[0]!;
const cardsTab = tabs[1]!;
const searchTab = tabs[2]!;
const decksTab = tabs[3]!;
const profileTab = tabs[4]!;

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
        class="flex flex-col items-center justify-center flex-1 h-full pt-2 transition-colors duration-150"
        :class="isActive(homeTab) ? 'text-blue-600 dark:text-blue-400' : 'text-gray-500 dark:text-gray-400'"
        :aria-current="isActive(homeTab) ? 'page' : undefined"
        aria-label="Home"
        @click="onTabTap(homeTab)"
      >
        <PhHouse :size="24" :weight="isActive(homeTab) ? 'fill' : 'regular'" />
        <span class="text-xs mt-1 font-medium">Home</span>
      </button>

      <button
        class="flex flex-col items-center justify-center flex-1 h-full pt-2 transition-colors duration-150"
        :class="isActive(cardsTab) ? 'text-blue-600 dark:text-blue-400' : 'text-gray-500 dark:text-gray-400'"
        :aria-current="isActive(cardsTab) ? 'page' : undefined"
        aria-label="Cards"
        @click="onTabTap(cardsTab)"
      >
        <PhCards :size="24" :weight="isActive(cardsTab) ? 'fill' : 'regular'" />
        <span class="text-xs mt-1 font-medium">Cards</span>
      </button>

      <button
        class="flex flex-col items-center justify-center flex-1 h-full pt-2 transition-colors duration-150"
        :class="isActive(searchTab) ? 'text-blue-600 dark:text-blue-400' : 'text-gray-500 dark:text-gray-400'"
        :aria-current="isActive(searchTab) ? 'page' : undefined"
        aria-label="Search"
        @click="onTabTap(searchTab)"
      >
        <PhMagnifyingGlass :size="24" :weight="isActive(searchTab) ? 'fill' : 'regular'" />
        <span class="text-xs mt-1 font-medium">Search</span>
      </button>

      <button
        class="flex flex-col items-center justify-center flex-1 h-full pt-2 transition-colors duration-150"
        :class="isActive(decksTab) ? 'text-blue-600 dark:text-blue-400' : 'text-gray-500 dark:text-gray-400'"
        :aria-current="isActive(decksTab) ? 'page' : undefined"
        aria-label="Decks"
        @click="onTabTap(decksTab)"
      >
        <PhStack :size="24" :weight="isActive(decksTab) ? 'fill' : 'regular'" />
        <span class="text-xs mt-1 font-medium">Decks</span>
      </button>

      <button
        class="flex flex-col items-center justify-center flex-1 h-full pt-2 transition-colors duration-150"
        :class="isActive(profileTab) ? 'text-blue-600 dark:text-blue-400' : 'text-gray-500 dark:text-gray-400'"
        :aria-current="isActive(profileTab) ? 'page' : undefined"
        aria-label="Profile"
        @click="onTabTap(profileTab)"
      >
        <PhUser :size="24" :weight="isActive(profileTab) ? 'fill' : 'regular'" />
        <span class="text-xs mt-1 font-medium">Profile</span>
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
