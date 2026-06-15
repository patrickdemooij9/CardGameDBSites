<script setup lang="ts">
/**
 * MobileHeader - Native-style top header/status bar area for mobile.
 * 
 * Provides a consistent header with safe area support for the notch/status bar,
 * optional back button, and action slots.
 */
import { PhCaretLeft } from '@phosphor-icons/vue';
import { useHaptics } from '~/composables/mobile/useHaptics';

const props = withDefaults(defineProps<{
  title?: string;
  showBack?: boolean;
  transparent?: boolean;
}>(), {
  title: '',
  showBack: false,
  transparent: false,
});

const router = useRouter();
const { impact, ImpactStyle } = useHaptics();

async function goBack() {
  await impact(ImpactStyle.Light);
  router.back();
}
</script>

<template>
  <header
    class="mobile-header sticky top-0 z-40 w-full"
    :class="transparent ? 'bg-transparent' : 'bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700'"
  >
    <div class="pt-safe" />
    <div class="flex items-center h-12 px-4">
      <!-- Left slot: back button or custom -->
      <div class="w-12 flex items-center">
        <button
          v-if="showBack"
          class="p-2 -ml-2 text-gray-700 dark:text-gray-200 active:bg-gray-100 dark:active:bg-gray-800 rounded-full transition-colors"
          aria-label="Go back"
          @click="goBack"
        >
          <PhCaretLeft :size="20" weight="bold" />
        </button>
        <slot name="left" />
      </div>

      <!-- Center: title -->
      <div class="flex-1 text-center">
        <h1 v-if="title" class="text-base font-semibold text-gray-900 dark:text-white truncate">
          {{ title }}
        </h1>
        <slot name="center" />
      </div>

      <!-- Right slot: actions -->
      <div class="w-12 flex items-center justify-end">
        <slot name="right" />
      </div>
    </div>
  </header>
</template>

<style scoped>
.mobile-header {
  /* Safe area for devices with notch/dynamic island */
  padding-top: env(safe-area-inset-top, 0px);
}

.pt-safe {
  padding-top: env(safe-area-inset-top, 0px);
}
</style>
