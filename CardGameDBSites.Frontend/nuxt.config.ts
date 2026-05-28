// https://nuxt.com/docs/api/configuration/nuxt-config
import { readFileSync } from 'fs';
import { defineNuxtConfig } from 'nuxt/config';
import { resolve } from 'path';

// When building for Capacitor (mobile), use SPA mode.
// Native apps can't run a Node.js server, so SSR must be disabled.
// Set NUXT_CAPACITOR=true in your environment when building for mobile.
const isCapacitorBuild = process.env.NUXT_CAPACITOR === 'true';

export default defineNuxtConfig({
  compatibilityDate: '2025-04-29',
  devtools: { enabled: true, 
    timeline: {
      enabled: true,
    }, },
  css: ['~/assets/css/tailwind.css'],
  typescript: {
    typeCheck: true
  },
  components: {
    global: true,
    dirs: ["~/components/pageTypes"]
  },
  modules: ['@nuxtjs/tailwindcss', '@pinia/nuxt', 'floating-vue/nuxt'],
  runtimeConfig: {
    public: {
      API_BASE_URL: process.env.NUXT_PUBLIC_API_BASE_URL,
      IS_CAPACITOR: isCapacitorBuild ? 'true' : 'false',
    }
  },
  vite: {
    optimizeDeps: {
      include: [
        '@vue/devtools-core',
        '@vue/devtools-kit',
        'vue-toastification',
        '@phosphor-icons/vue',
        'vue-chartjs',
        'chart.js'
      ]
    }
  },

  // Capacitor-specific configuration
  ...(isCapacitorBuild && {
    ssr: false, // SPA mode — required for native app shell
    app: {
      head: {
        meta: [
          // Prevent zoom on mobile inputs
          { name: 'viewport', content: 'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no, viewport-fit=cover' },
          // iOS web app capable
          { name: 'apple-mobile-web-app-capable', content: 'yes' },
          { name: 'apple-mobile-web-app-status-bar-style', content: 'default' },
        ],
      },
    },
  }),
})