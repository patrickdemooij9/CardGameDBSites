import { Share } from '@capacitor/share';
import { Capacitor } from '@capacitor/core';

/**
 * Composable for native share sheet.
 * 
 * On native platforms, opens the OS share sheet.
 * On web, falls back to the Web Share API if available, otherwise copies to clipboard.
 */
export function useNativeShare() {
  const isNative = Capacitor.isNativePlatform();

  async function share(options: { title?: string; text?: string; url?: string; dialogTitle?: string }) {
    if (isNative) {
      await Share.share(options);
      return;
    }

    // Web fallback: use Web Share API if available
    if (import.meta.client && navigator.share) {
      await navigator.share({
        title: options.title,
        text: options.text,
        url: options.url,
      });
      return;
    }

    // Final fallback: copy URL to clipboard
    if (import.meta.client && options.url) {
      await navigator.clipboard.writeText(options.url);
    }
  }

  async function canShare(): Promise<boolean> {
    if (isNative) return true;
    if (import.meta.client) return !!navigator.share;
    return false;
  }

  return {
    share,
    canShare,
  };
}
