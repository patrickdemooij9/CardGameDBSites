// https://nuxt.com/docs/api/configuration/nuxt-config
import { readFileSync } from 'fs';
import { defineNuxtConfig } from 'nuxt/config';
import { resolve } from 'path';

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
      API_BASE_URL: process.env.NUXT_PUBLIC_API_BASE_URL
    }
  },
  routeRules: {
    "/": { swr: 300 },
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
  }
})