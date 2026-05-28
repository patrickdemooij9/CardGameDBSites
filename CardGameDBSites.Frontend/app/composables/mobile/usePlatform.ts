import { ref, computed } from 'vue';
import { Capacitor } from '@capacitor/core';

export type Platform = 'native-ios' | 'native-android' | 'mobile-web' | 'desktop-web';

/**
 * Composable for detecting the current platform.
 * 
 * Strategies:
 * - Native app: Uses Capacitor.isNativePlatform() which is true only when running
 *   inside a native iOS/Android shell.
 * - Mobile web vs desktop: Uses user-agent detection as a fallback for web contexts.
 * 
 * Why not use CSS media queries alone?
 * - We need platform detection in JS for conditional logic (e.g., which plugins to call,
 *   which layout to render, which navigation to show).
 * - CSS handles responsive styling; this composable handles behavioral differences.
 */
export function usePlatform() {
  const isNative = Capacitor.isNativePlatform();
  const nativePlatform = Capacitor.getPlatform(); // 'ios' | 'android' | 'web'

  const isMobileWeb = ref(false);

  // Only run UA detection on client side
  if (import.meta.client) {
    const ua = navigator.userAgent || '';
    isMobileWeb.value = /Android|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop/i.test(ua);
  }

  const platform = computed<Platform>(() => {
    if (isNative && nativePlatform === 'ios') return 'native-ios';
    if (isNative && nativePlatform === 'android') return 'native-android';
    if (isMobileWeb.value) return 'mobile-web';
    return 'desktop-web';
  });

  const isIOS = computed(() => platform.value === 'native-ios');
  const isAndroid = computed(() => platform.value === 'native-android');
  const isMobile = computed(() => isNative || isMobileWeb.value);
  const isDesktop = computed(() => platform.value === 'desktop-web');

  return {
    platform,
    isNative,
    isIOS,
    isAndroid,
    isMobile,
    isMobileWeb,
    isDesktop,
  };
}
