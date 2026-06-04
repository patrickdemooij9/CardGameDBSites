import { Haptics, ImpactStyle, NotificationType } from '@capacitor/haptics';
import { Capacitor } from '@capacitor/core';

/**
 * Composable for native haptic feedback.
 * 
 * Falls back gracefully on web — all methods are no-ops when not on a native platform.
 * This allows you to call haptics anywhere without platform checks in consuming code.
 */
export function useHaptics() {
  const isAvailable = Capacitor.isNativePlatform();

  async function impact(style: ImpactStyle = ImpactStyle.Medium) {
    if (!isAvailable) return;
    await Haptics.impact({ style });
  }

  async function notification(type: NotificationType = NotificationType.Success) {
    if (!isAvailable) return;
    await Haptics.notification({ type });
  }

  async function vibrate(duration: number = 300) {
    if (!isAvailable) return;
    await Haptics.vibrate({ duration });
  }

  async function selectionStart() {
    if (!isAvailable) return;
    await Haptics.selectionStart();
  }

  async function selectionChanged() {
    if (!isAvailable) return;
    await Haptics.selectionChanged();
  }

  async function selectionEnd() {
    if (!isAvailable) return;
    await Haptics.selectionEnd();
  }

  return {
    isAvailable,
    impact,
    notification,
    vibrate,
    selectionStart,
    selectionChanged,
    selectionEnd,
    ImpactStyle,
    NotificationType,
  };
}
