<script setup lang="ts">
import { PhCaretDown, PhSignIn, PhUser } from "@phosphor-icons/vue";
import type NavigationModel from "./NavigationModel";

defineProps<{
  content: NavigationModel;
}>();

const isOpen = ref(false);
const isAccountsOpen = ref(false);
const accountStore = useAccountStore();
</script>

<template>
  <nav
    :class="{
      'md:text-white': content.textColorIsWhite,
      'md:text-black': !content.textColorIsWhite,
      active: isOpen,
    }"
    class="h-14 bg-main-color text-base z-20 border-b border-nav-color"
  >
    <div class="container md:px-8">
      <div class="flex justify-between overflow-auto">
        <NuxtLink to="/" class="ml-2 no-underline">
          <img :src="content.navigationLogoUrl" class="h-14 py-2 pr-8" />
        </NuxtLink>

        <div
          :class="{
            'fixed top-14 bg-slate-50 h-full w-full z-20 flex-col': isOpen,
            hidden: !isOpen || content.createDeckMode,
          }"
          class="grow md:justify-between md:flex"
        >
          <div
            class="flex flex-col mt-4 mx-2 md:gap-8 md:h-full md:flex-row md:mt-0"
          >
            <div
              v-for="item in content.items"
              :class="{ group: item.children.length > 0 }"
            >
              <p
                class="flex items-center hover:text-slate-400 md:h-full gap-2 py-3 md:py-0"
              >
                <NuxtLink :to="item.url" class="no-underline">
                  {{ item.name }}
                </NuxtLink>
                <PhCaretDown v-if="item.children.length > 0" />
              </p>
              <div
                v-if="item.children.length > 0"
                class="group-hover:flex flex-col rounded-md z-20 md:bg-main-color md:absolute md:shadow-lg md:ring-black md:ring-1 md:-mt-2 hidden"
              >
                <NuxtLink
                  v-for="child in item.children"
                  :to="child.url"
                  class="px-6 py-3 rounded-md no-underline hover:text-slate-400"
                >
                  {{ child.name }}
                </NuxtLink>
              </div>
            </div>
          </div>
          <div v-if="content.loginPageUrl" class="group">
            <p
              class="flex items-center hover:text-slate-400 md:h-full gap-2 py-3 md:py-0"
              v-if="!accountStore.isLoggedIn"
            >
              <NuxtLink :to="content.loginPageUrl" class="no-underline">
                Login
              </NuxtLink>
              <PhSignIn />
            </p>
            <p
              class="flex items-center hover:text-slate-400 md:h-full gap-2 py-3 md:py-0"
              v-if="accountStore.isLoggedIn"
            >
              Hello {{ accountStore.member!.name }}
              <PhUser />
            </p>
            <div
                v-if="accountStore.isLoggedIn && content.accountItems.length > 0"
                class="group-hover:flex flex-col rounded-md z-20 md:bg-main-color md:absolute md:shadow-lg md:ring-black md:ring-1 md:-mt-2 hidden"
              >
              
                <NuxtLink
                  v-for="child in content.accountItems"
                  :to="child.url"
                  class="px-6 py-3 rounded-md no-underline hover:text-slate-400"
                >
                  {{ child.name }}
                </NuxtLink>
              </div>
          </div>
        </div>
        <div
          @click="isOpen = !isOpen"
          :class="{ hidden: content.createDeckMode }"
          class="md:hidden w-8 mobile-content flex center-items mr-2"
        >
          <div id="hamburger" class="hamburger" aria-label="Show navigation">
            <div class="top-bun" :class="{ 'rotate-45': isOpen }"></div>
            <div class="meat" :class="{ 'scale-y-0': isOpen }"></div>
            <div class="bottom-bun" :class="{ 'rotate-45': isOpen }"></div>
          </div>
        </div>
      </div>
    </div>
  </nav>
</template>
