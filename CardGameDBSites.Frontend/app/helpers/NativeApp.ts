import { Capacitor } from "@capacitor/core";

export function isNativeApp() {
  return import.meta.client && Capacitor.isNativePlatform();
}
