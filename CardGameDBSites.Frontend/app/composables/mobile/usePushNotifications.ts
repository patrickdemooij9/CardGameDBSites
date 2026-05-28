import { PushNotifications } from '@capacitor/push-notifications';
import { Capacitor } from '@capacitor/core';
import { ref } from 'vue';

/**
 * Composable for push notification registration and handling.
 * 
 * Only works on native platforms. On web, all methods are no-ops.
 * 
 * Usage:
 * ```ts
 * const { register, token, notifications } = usePushNotifications();
 * await register();
 * // token.value now contains the device token to send to your backend
 * ```
 */
export function usePushNotifications() {
  const isAvailable = Capacitor.isNativePlatform();
  const token = ref<string | null>(null);
  const notifications = ref<any[]>([]);

  async function register() {
    if (!isAvailable) return;

    const permission = await PushNotifications.requestPermissions();
    if (permission.receive !== 'granted') return;

    await PushNotifications.register();

    PushNotifications.addListener('registration', (registrationToken) => {
      token.value = registrationToken.value;
    });

    PushNotifications.addListener('registrationError', (error) => {
      console.error('Push registration error:', error);
    });

    PushNotifications.addListener('pushNotificationReceived', (notification) => {
      notifications.value.push(notification);
    });

    PushNotifications.addListener('pushNotificationActionPerformed', (action) => {
      // Handle notification tap — navigate to relevant page, etc.
      console.log('Push action performed:', action);
    });
  }

  async function removeAllListeners() {
    if (!isAvailable) return;
    await PushNotifications.removeAllListeners();
  }

  return {
    isAvailable,
    token,
    notifications,
    register,
    removeAllListeners,
  };
}
