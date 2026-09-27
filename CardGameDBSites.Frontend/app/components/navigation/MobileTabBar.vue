<script setup lang="ts">
import { PhBooks, PhDotsThreeOutline, PhHouse, PhMagnifyingGlass, PhStack } from "@phosphor-icons/vue";

const props = defineProps<{
  collectionUrl?: string;
}>();

const route = useRoute();

const tabs = computed(() => [
  { name: "Home", url: "/", icon: PhHouse },
  { name: "Search", url: "/app/search", icon: PhMagnifyingGlass },
  { name: "Collection", url: props.collectionUrl ?? "/collection", icon: PhBooks },
  { name: "Decks", url: "/app/decks", icon: PhStack },
  { name: "More", url: "/app/more", icon: PhDotsThreeOutline },
]);

// CMS urls carry a trailing slash, hand-written ones don't — compare them stripped.
function trimSlash(url: string) {
  return url.replace(/\/+$/, "") || "/";
}

function isActive(url: string) {
  const path = trimSlash(route.path);
  const target = trimSlash(url);
  return target === "/" ? path === "/" : path === target || path.startsWith(`${target}/`);
}
</script>

<template>
  <nav
    class="fixed bottom-0 inset-x-0 z-30 bg-white border-t border-gray-200 pb-safe-bottom"
    aria-label="Main"
  >
    <div class="flex h-14">
      <NuxtLink
        v-for="tab in tabs"
        :key="tab.url"
        :to="tab.url"
        class="flex-1 flex flex-col items-center justify-center gap-0.5 no-underline"
        :class="isActive(tab.url) ? 'text-main-color' : 'text-gray-500'"
        :aria-current="isActive(tab.url) ? 'page' : undefined"
      >
        <component :is="tab.icon" :size="24" :weight="isActive(tab.url) ? 'fill' : 'regular'" />
        <span class="text-[11px] leading-none">{{ tab.name }}</span>
      </NuxtLink>
    </div>
  </nav>
</template>
