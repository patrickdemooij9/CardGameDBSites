<script setup lang="ts">
import type NavigationModel from "./NavigationModel";
import NavigationDropdown from "./NavigationDropdown.vue";
import { useAccountStore } from "~/stores/AccountStore";
import CmsImage from "~/components/shared/CmsImage.vue";

const props = defineProps<{
  content: NavigationModel;
  transparent?: boolean;
}>();

const isOpen = ref(false);
const accountStore = useAccountStore();

const isTransparent = computed(() => props.transparent && !isOpen.value);

function closeMenu() {
  isOpen.value = false;
}

function logout() {
  accountStore.logout();
  closeMenu();
}
</script>

<template>
  <nav
    :class="[
      isTransparent
        ? 'bg-transparent text-ink'
        : ['bg-main-color', content.textColorIsWhite ? 'text-white' : 'text-black'],
      { 'border-b border-nav-color': !transparent },
    ]"
    class="z-30 h-14 text-sm transition-colors duration-200"
  >
    <div class="container h-full px-4 md:px-8">
      <div class="flex h-full items-center justify-between gap-6">
        <NuxtLink to="/" class="shrink-0 no-underline" @click="closeMenu">
          <CmsImage
            v-if="content.navigationLogoUrl"
            :src="content.navigationLogoUrl"
            class="h-14 py-2"
            loading="eager"
            :alt="content.siteName"
          />
          <span v-else class="text-base font-bold">{{ content.siteName }}</span>
        </NuxtLink>

        <div
          :class="
            isOpen && !content.createDeckMode
              ? 'fixed inset-x-0 bottom-0 top-14 flex flex-col overflow-y-auto bg-surface p-4 text-ink'
              : 'hidden'
          "
          class="md:static md:flex md:grow md:flex-row md:items-center md:justify-between md:overflow-visible md:bg-transparent md:p-0 md:text-inherit"
        >
          <div class="flex flex-col md:flex-row md:items-center md:gap-1">
            <NavigationDropdown
              v-for="item in content.items"
              :key="item.url"
              :label="item.name"
              :url="item.url"
              :items="item.children"
              @navigate="closeMenu"
            />
          </div>

          <div
            v-if="content.loginPageUrl"
            class="mt-2 border-t border-line pt-4 md:mt-0 md:border-0 md:pt-0"
          >
            <NuxtLink
              v-if="!accountStore.isLoggedIn"
              :to="content.loginPageUrl"
              class="btn-secondary no-underline px-3 py-1.5"
              @click="closeMenu"
            >
              Log in
            </NuxtLink>
            <NavigationDropdown
              v-else
              :label="`Hello ${accountStore.member!.name}`"
              :items="content.accountItems"
              align-right
              @navigate="closeMenu"
            >
              <template #append>
                <button
                  type="button"
                  class="rounded-md py-2 text-left text-ink-muted hover:text-ink md:px-3 md:hover:bg-canvas"
                  @click="logout"
                >
                  Log out
                </button>
              </template>
            </NavigationDropdown>
          </div>
        </div>

        <button
          type="button"
          :class="{ hidden: content.createDeckMode }"
          class="flex w-8 md:hidden"
          :aria-expanded="isOpen"
          aria-label="Show navigation"
          @click="isOpen = !isOpen"
        >
          <div class="hamburger">
            <div class="top-bun" :class="{ 'rotate-45': isOpen }"></div>
            <div class="meat" :class="{ 'scale-y-0': isOpen }"></div>
            <div class="bottom-bun" :class="{ '-rotate-45': isOpen }"></div>
          </div>
        </button>
      </div>
    </div>
  </nav>
</template>
