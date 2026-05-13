<template>
  <NuxtLayout>
    <NuxtLoadingIndicator :color="'#2563eb'" />
    <NuxtPage />
  </NuxtLayout>
  <div
    id="cursor-image"
    class="pointer-events-none fixed h-72 w-48 bg-contain bg-no-repeat"
    style="display: none"
  ></div>
</template>
