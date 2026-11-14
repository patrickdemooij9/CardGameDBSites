<script setup lang="ts">
import MobileTabBar from "~/components/navigation/MobileTabBar.vue";
import Navigation from "~/components/navigation/Navigation.vue";
import SiteFooter from "~/components/navigation/SiteFooter.vue";
import ImpersonationBanner from "~/components/shared/ImpersonationBanner.vue";
import { useSite } from "~/composables/useSite";
import { isNativeApp } from "~/helpers/NativeApp";

const { getSettings, getNavigation, getNavigationUrl } = useSite();
const siteSettings = await getSettings();
const navigationViewModel = await getNavigation();
const collectionUrl = await getNavigationUrl("/collection");

const isNative = isNativeApp();
const route = useRoute();
const isHomepage = computed(() => route.path === "/");

onMounted(async () => {
  if (!isNative) {
    return;
  }
  try {
    // No top bar in the app, so the status bar sits over the page background — dark icons.
    const { StatusBar, Style } = await import("@capacitor/status-bar");
    await StatusBar.setStyle({ style: Style.Light });
  } catch {
    // StatusBar is unavailable on platforms that don't implement it.
  }
});
</script>

<template>
  <div
    :style="{
      '--site-main-color': siteSettings.mainColor,
      '--site-main-color-hover': siteSettings.hoverMainColor,
    }"
    :class="{ 'has-tab-bar': isNative }"
    class="min-h-screen flex flex-col"
    id="root"
  >
    <div v-if="isNative" class="sticky top-0 z-20 bg-surface pt-safe-top"></div>
    <ImpersonationBanner />
    <Navigation v-if="!isNative" :content="navigationViewModel" :transparent="isHomepage" />
    <div class="grow">
      <slot />
    </div>
    <SiteFooter v-if="!isNative" :settings="siteSettings" class="[.hide-footer_&]:hidden" />
    <MobileTabBar v-if="isNative" :collection-url="collectionUrl" />
  </div>
</template>
