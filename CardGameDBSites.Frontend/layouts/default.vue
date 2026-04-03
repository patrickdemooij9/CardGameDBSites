<script setup lang="ts">
import Navigation from "~/components/navigation/Navigation.vue";
import { useSite } from "~/composables/useSite";

const { getSettings, getNavigation } = useSite();
const siteSettings = await getSettings();
const navigationViewModel = await getNavigation();
</script>

<template>
  <div
    :style="{
      '--main-color': siteSettings.mainColor,
      '--main-color-hover': siteSettings.hoverMainColor,
      '--nav-border-color': siteSettings.mainColor,
    }"
    class="min-h-screen"
    id="root"
  >
    <Navigation :content="navigationViewModel"> </Navigation>
    <div>
      <slot />
    </div>
  </div>
</template>
