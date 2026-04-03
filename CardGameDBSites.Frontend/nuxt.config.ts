// https://nuxt.com/docs/api/configuration/nuxt-config
import { readFileSync } from 'fs';
import { defineNuxtConfig } from 'nuxt/config';
import { resolve } from 'path';

export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },
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
  }
})