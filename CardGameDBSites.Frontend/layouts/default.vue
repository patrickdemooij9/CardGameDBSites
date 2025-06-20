<script setup lang="ts">
import Navigation from "~/components/navigation/Navigation.vue";
import SiteService from "~/services/SiteService";

const siteService = new SiteService();
const siteSettings = await siteService.getSettings();
const navigationViewModel = await siteService.getNavigation();
</script>

<template>
  <div
    :style="{
      '--main-color': siteSettings.mainColor,
      '--main-color-hover': siteSettings.hoverMainColor,
      '--nav-border-color': siteSettings.mainColor,
    }"
    class="min-h-screen"
  >
    <Navigation :content="navigationViewModel"> </Navigation>
    <div>
      <slot />
    </div>
  </div>
</template>
