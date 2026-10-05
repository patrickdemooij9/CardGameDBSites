<script setup lang="ts">
import CameraScanner from './components/CameraScanner.vue'
</script>

<template>
  <div class="min-h-screen bg-slate-950 text-slate-100">
    <header class="border-b border-slate-800">
      <div class="mx-auto flex max-w-6xl flex-wrap items-baseline gap-x-3 gap-y-1 px-4 py-4">
        <h1 class="text-lg font-semibold">SWU Card Scanner</h1>
        <p class="text-sm text-slate-400">
          Milestone 1 — find cards in the camera feed
        </p>
      </div>
    </header>

    <main class="mx-auto max-w-6xl px-4 py-6">
      <CameraScanner />
    </main>
  </div>
</template>
