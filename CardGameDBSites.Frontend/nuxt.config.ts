// https://nuxt.com/docs/api/configuration/nuxt-config
import { readFileSync } from 'fs';
import { defineNuxtConfig } from 'nuxt/config';
import { resolve } from 'path';

// The Capacitor build ships as a static SPA — there is no Nitro server in the app.
const isNativeBuild = process.env.NUXT_BUILD_TARGET === 'native';
const cardScannerLib = resolve(__dirname, '../SWUCardScanner/web/src/lib');

export default defineNuxtConfig({
  compatibilityDate: '2025-04-29',
  ssr: !isNativeBuild,
  app: {
    head: {
      viewport: 'width=device-width, initial-scale=1, viewport-fit=cover'
    }
  },
  devtools: { enabled: true, 
    timeline: {
      enabled: true,
    }, },
  css: ['~/assets/css/tailwind.css'],
  typescript: {
    typeCheck: true,
    // The scanner lib is built against a looser tsconfig, so type against its generated declarations.
    tsConfig: {
      compilerOptions: {
        paths: {
          '#card-scanner/*': ['../../SWUCardScanner/web/types/*']
        }
      }
    }
  },
  hooks: {
    // onnxruntime's 27 MB wasm is over Cloudflare's per-file limit, so the scanner only ships in the app.
    'pages:extend'(pages) {
      if (isNativeBuild) {
        return;
      }
      const scanner = pages.findIndex((page) => page.path === '/app/scanner');
      if (scanner !== -1) {
        pages.splice(scanner, 1);
      }
    }
  },
  components: {
    global: true,
    dirs: ["~/components/pageTypes"]
  },
  modules: ['@nuxtjs/tailwindcss', '@pinia/nuxt', 'floating-vue/nuxt', '@nuxt/image'],
  image: {
    provider: 'umbraco',
    providers: {
      umbraco: {
        provider: '~/providers/umbraco.ts',
        options: { baseURL: process.env.NUXT_PUBLIC_API_BASE_URL }
      }
    },
    format: ['webp'],
    quality: 70
  },
  runtimeConfig: {
    cachePurgeSecret: '',
    public: {
      API_BASE_URL: process.env.NUXT_PUBLIC_API_BASE_URL,
      SCANNER_URL: process.env.NUXT_PUBLIC_SCANNER_URL
    }
  },
  routeRules: isNativeBuild ? {} : {
    '/': { swr: 3600 }
  },
  nitro: {
    prerender: {
      crawlLinks: false
    },
    storage: isNativeBuild ? {} : {
      // All sites share one KV namespace and Nitro keys the cache by path only, so prefix per site.
      cache: { driver: 'cloudflare-kv-binding', binding: 'NUXT_CACHE', base: process.env.NUXT_PUBLIC_SITE_ID }
    },
    devStorage: {
      cache: { driver: 'memory' }
    }
  },
  vite: {
    resolve: {
      alias: { '#card-scanner': cardScannerLib },
      dedupe: ['onnxruntime-web']
    },
    optimizeDeps: {
      // Pre-bundling rewrites onnxruntime's `new URL(..., import.meta.url)` wasm lookup and 404s it.
      exclude: ['onnxruntime-web'],
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