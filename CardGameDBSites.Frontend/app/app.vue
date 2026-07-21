<template>
  <NuxtLayout>
    <NuxtLoadingIndicator :color="'#2563eb'" />
    <NuxtPage />
  </NuxtLayout>
  <div
    id="cursor-image"
    class="pointer-events-none fixed h-80 w-56 bg-contain bg-no-repeat"
    style="display: none"
  ></div>
</template>
