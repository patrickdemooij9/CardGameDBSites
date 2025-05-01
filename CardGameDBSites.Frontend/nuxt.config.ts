// https://nuxt.com/docs/api/configuration/nuxt-config
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
  modules: ['@nuxtjs/tailwindcss', '@pinia/nuxt']
})