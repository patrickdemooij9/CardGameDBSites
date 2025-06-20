// https://nuxt.com/docs/api/configuration/nuxt-config
import { readFileSync } from 'fs';
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
      API_BASE_URL: process.env.NUXT_API_BASE_URL
    }
  },
  devServer: {
    https: {
      key: readFileSync(resolve(__dirname, 'AidalonDB+2-key.pem'), 'utf8'),
      cert: readFileSync(resolve(__dirname, 'AidalonDB+2.pem'), 'utf8')
    },
    port: 3000,
    host: 'aidalon.local'
  }
})