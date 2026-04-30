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
        strict: true,
      },
    },
  },
  resolve: {
    alias: {
      "~": resolve(__dirname, "app"),
    },
  },
});
