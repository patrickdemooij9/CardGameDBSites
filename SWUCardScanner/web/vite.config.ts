import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import basicSsl from '@vitejs/plugin-basic-ssl'

export default defineConfig(({ command, mode }) => ({
  plugins: [
    vue(),
    tailwindcss(),
    // getUserMedia needs a secure context. localhost already counts, so plain `npm run
    // dev` stays on http; a phone on the LAN does not, so `npm run dev:https` switches
    // this on and serves a self-signed cert you accept once on the device.
    ...(mode === 'https' ? [basicSsl()] : []),
  ],
  // onnxruntime-web references its wasm and worker files with `new URL(..., import.meta.url)`,
  // which Vite resolves and emits on its own. Excluding it from dep pre-bundling keeps
  // those URLs intact - otherwise esbuild rewrites them and the wasm 404s at runtime.
  optimizeDeps: {
    exclude: ['onnxruntime-web'],
  },
  server: {
    host: true,
    // Cross-origin isolation unlocks SharedArrayBuffer, which lets onnxruntime-web
    // use multi-threaded wasm. Without these headers it silently falls back to a
    // single thread and inference is several times slower.
    headers: {
      'Cross-Origin-Opener-Policy': 'same-origin',
      'Cross-Origin-Embedder-Policy': 'require-corp',
    },
  },
  preview: {
    headers: {
      'Cross-Origin-Opener-Policy': 'same-origin',
      'Cross-Origin-Embedder-Policy': 'require-corp',
    },
  },
  // ONNX models are already compressed; inlining or re-processing them wastes memory.
  assetsInclude: ['**/*.onnx'],
  build: {
    target: 'es2022',
    sourcemap: command === 'build',
  },
}))
