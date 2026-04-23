// https://nuxt.com/docs/api/configuration/nuxt-config
import { readFileSync } from 'fs';
import { defineNuxtConfig } from 'nuxt/config';
import { resolve } from 'path';

// Select the active theme at build time so only that theme's CSS is bundled.
// Set NUXT_THEME_KEY in the deployment environment (e.g. "cyberpunk").
// Defaults to "default" when the variable is absent.
const themeKey = process.env.NUXT_THEME_KEY ?? 'default';

export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },
  typescript: {
    typeCheck: true
  },
  css: [`~/assets/css/themes/${themeKey}.css`],
  components: {
    global: true,
    dirs: ["~/components/pageTypes"]
  },
  modules: ['@nuxtjs/tailwindcss', '@pinia/nuxt', 'floating-vue/nuxt'],
  runtimeConfig: {
    public: {
      API_BASE_URL: process.env.NUXT_PUBLIC_API_BASE_URL
    }
  },
  routeRules: {
    "/": { swr: true },
  },
})