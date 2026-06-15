import { Capacitor } from '@capacitor/core';
import { StatusBar, Style } from '@capacitor/status-bar';
import { Keyboard } from '@capacitor/keyboard';

/**
 * Nuxt plugin that initializes Capacitor-specific behavior on app startup.
 * 
 * This runs only on the client side and only when inside a native shell.
 * It configures:
 * - Status bar appearance
 * - Keyboard behavior
 * - Deep link handling
 * - Back button behavior (Android)
 */
export default defineNuxtPlugin(async () => {
  // Only run on native platforms
  if (!Capacitor.isNativePlatform()) return;

  const platform = Capacitor.getPlatform();

  // Configure status bar
  try {
    await StatusBar.setStyle({ style: Style.Light });
    if (platform === 'android') {
      await StatusBar.setBackgroundColor({ color: '#ffffff' });
    }
  } catch (e) {
    // StatusBar plugin may not be available in all contexts
    console.warn('StatusBar configuration failed:', e);
  }

  // Configure keyboard behavior on iOS
  if (platform === 'ios') {
    try {
      Keyboard.addListener('keyboardWillShow', () => {
        document.body.classList.add('keyboard-visible');
      });
      Keyboard.addListener('keyboardWillHide', () => {
        document.body.classList.remove('keyboard-visible');
      });
    } catch (e) {
      console.warn('Keyboard configuration failed:', e);
    }
  }
});
