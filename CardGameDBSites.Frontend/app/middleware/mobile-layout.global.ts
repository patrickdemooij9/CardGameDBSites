import { Capacitor } from '@capacitor/core';

/**
 * Global middleware that automatically switches to the mobile layout
 * when running inside a Capacitor native shell.
 * 
 * This allows pages to use the default layout on web and automatically
 * get the mobile layout in the native app, without per-page configuration.
 * 
 * Pages can opt out by explicitly setting their layout:
 * definePageMeta({ layout: 'default' })
 */
export default defineNuxtRouteMiddleware((to) => {
  // Only apply on client side and in native context
  if (!import.meta.client) return;
  if (!Capacitor.isNativePlatform()) return;

  // Don't override if the page explicitly set a layout
  if (to.meta.layout && to.meta.layout !== 'default') return;

  // Switch to mobile layout for native app
  to.meta.layout = 'mobile';
});
