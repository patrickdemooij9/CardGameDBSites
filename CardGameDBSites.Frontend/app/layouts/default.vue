<script setup lang="ts">
import MobileTabBar from "~/components/navigation/MobileTabBar.vue";
import Navigation from "~/components/navigation/Navigation.vue";
import ImpersonationBanner from "~/components/shared/ImpersonationBanner.vue";
import { useSite } from "~/composables/useSite";
import { isNativeApp } from "~/helpers/NativeApp";

const { getSettings, getNavigation } = useSite();
const siteSettings = await getSettings();
const navigationViewModel = await getNavigation();

const isNative = isNativeApp();
const navUrl = (prefix: string) =>
  siteSettings.navigation?.find((item) => item.url.startsWith(prefix))?.url;

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
      '--main-color': siteSettings.mainColor,
      '--main-color-hover': siteSettings.hoverMainColor,
      '--nav-border-color': siteSettings.mainColor,
    }"
    :class="{ 'has-tab-bar': isNative }"
    class="min-h-screen flex flex-col"
    id="root"
  >
    <div v-if="isNative" class="pt-safe-top"></div>
    <ImpersonationBanner />
    <Navigation v-if="!isNative" :content="navigationViewModel"> </Navigation>
    <div class="grow">
      <slot />
    </div>
    <footer
      v-if="!isNative"
      :class="{
        'text-white': siteSettings.textColorWhite,
        'text-black': !siteSettings.textColorWhite,
      }"
      class="mt-auto md:flex px-4 md:px-8 bg-main-color"
    >
      <div class="py-4 md:w-2/3 text-sm">
        <p>{{ siteSettings.footerText }}</p>
      </div>
      <nav class="pl-4 py-4" aria-label="Other sites">
        <p class="font-bold">Other sites</p>
        <ul>
          <li v-for="link in siteSettings.footerLinks ?? []" :key="`${link.name}-${link.url}`">
            <a
              :href="link.url"
              :target="link.target ?? undefined"
              :rel="link.target === '_blank' ? 'noopener noreferrer' : undefined"
              class="no-underline"
            >
              {{ link.name }}
            </a>
          </li>
        </ul>
      </nav>
    </footer>
    <MobileTabBar v-if="isNative" :collection-url="navUrl('/collection')" />
  </div>
</template>
