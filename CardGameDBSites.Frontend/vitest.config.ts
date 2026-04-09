import { defineConfig } from "vitest/config";
import { resolve } from "path";

export default defineConfig({
  test: {
    environment: "node",
    globals: true,
  },
  esbuild: {
    tsconfigRaw: {
      compilerOptions: {
        target: "ESNext",
        module: "ESNext",
        moduleResolution: "Bundler",
        strict: true,
        skipLibCheck: true,
      },
    },
  },
  resolve: {
    alias: {
      "~": resolve(__dirname, "."),
    },
  },
});
