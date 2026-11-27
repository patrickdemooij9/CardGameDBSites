<script setup lang="ts">
import { PhMoon, PhSun } from "@phosphor-icons/vue";
import { useColorMode } from "~/composables/useColorMode";

const { isDark, toggle } = useColorMode();
</script>

<template>
  <button
    type="button"
    :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
    :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
    @click="toggle"
  >
    <ClientOnly>
      <PhSun v-if="isDark" :size="20" />
      <PhMoon v-else :size="20" />
      <template #fallback>
        <PhMoon :size="20" />
      </template>
    </ClientOnly>
    <slot />
  </button>
</template>
