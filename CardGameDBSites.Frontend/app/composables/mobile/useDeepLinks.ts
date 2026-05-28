import { App, type URLOpenListenerEvent } from '@capacitor/app';
import { Capacitor } from '@capacitor/core';

/**
 * Composable for handling deep links in the native app.
 * 
 * Deep links allow external URLs (e.g., swunlimiteddb://cards/123 or
 * https://swunlimiteddb.com/cards/123) to open directly inside the app.
 * 
 * Usage:
 * ```ts
 * const { initDeepLinks } = useDeepLinks();
 * initDeepLinks((path) => {
 *   navigateTo(path);
 * });
 * ```
 */
export function useDeepLinks() {
  const isAvailable = Capacitor.isNativePlatform();

  function initDeepLinks(onDeepLink: (path: string) => void) {
    if (!isAvailable) return;

    App.addListener('appUrlOpen', (event: URLOpenListenerEvent) => {
      // Extract the path from the full URL
      // e.g., https://swunlimiteddb.com/cards/123 -> /cards/123
      const url = new URL(event.url);
      const path = url.pathname + url.search + url.hash;

      if (path) {
        onDeepLink(path);
      }
    });
  }

  /**
   * Listen for the app being brought back from background.
   * Useful for refreshing data or checking auth state.
   */
  function onAppStateChange(callback: (isActive: boolean) => void) {
    if (!isAvailable) return;

    App.addListener('appStateChange', (state) => {
      callback(state.isActive);
    });
  }

  /**
   * Handle the hardware back button on Android.
   * Return true to prevent default behavior, false to allow it.
   */
  function onBackButton(callback: () => void) {
    if (!isAvailable) return;

    App.addListener('backButton', () => {
      callback();
    });
  }

  return {
    isAvailable,
    initDeepLinks,
    onAppStateChange,
    onBackButton,
  };
}
