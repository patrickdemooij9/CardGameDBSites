<script setup lang="ts">
import { PhCaretDown, PhSignIn, PhUser } from "@phosphor-icons/vue";
import type NavigationModel from "./NavigationModel";

defineProps<{
  content: NavigationModel;
}>();

const isOpen = ref(false);
const isAccountsOpen = ref(false);
const accountStore = useAccountStore();

function closeMenu() {
  isOpen.value = false;
  isAccountsOpen.value = false;
}

function logout() {
  accountStore.logout();
  closeMenu();
}
</script>

<template>
  <nav
    :class="{
      'md:text-white': content.textColorIsWhite,
      'md:text-black': !content.textColorIsWhite,
      active: isOpen,
      'bg-gray-500': content.homepageMode,
      'bg-main-color border-b border-nav-color': !content.homepageMode,
    }"
    class="h-14 text-base z-20"
  >
    <div class="container md:px-8">
      <div class="flex justify-between overflow-auto items-center">
        <NuxtLink to="/" class="ml-2 no-underline" @click="closeMenu">
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
                <NuxtLink
                  :to="item.url"
                  class="no-underline"
                  @click="closeMenu"
                >
                  {{ item.name }}
                </NuxtLink>
                <PhCaretDown v-if="item.children.length > 0" />
              </p>
              <div
                v-if="item.children.length > 0"
                class="group-hover:flex flex-col rounded-md z-20 md:bg-main-color md:absolute md:shadow-lg md:ring-black md:ring-1 hidden"
              >
                <NuxtLink
                  v-for="child in item.children"
                  :to="child.url"
                  class="px-6 py-3 rounded-md no-underline hover:text-slate-400"
                  @click="closeMenu"
                >
                  {{ child.name }}
                </NuxtLink>
              </div>
            </div>
          </div>
          <div v-if="content.loginPageUrl" class="group mt-4 mx-2 md:mt-0">
            <p
              class="flex items-center hover:text-slate-400 md:h-full gap-2 py-3 md:py-0"
              v-if="!accountStore.isLoggedIn"
            >
              <NuxtLink
                :to="content.loginPageUrl"
                class="no-underline"
                @click="closeMenu"
              >
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
              class="group-hover:flex flex-col rounded-md z-20 md:bg-main-color md:absolute md:shadow-lg md:ring-black md:ring-1 md:hidden"
            >
              <NuxtLink
                v-for="child in content.accountItems"
                :to="child.url"
                class="px-6 py-3 rounded-md no-underline hover:text-slate-400"
                @click="closeMenu"
              >
                {{ child.name }}
              </NuxtLink>
              <button
                class="px-6 py-3 rounded-md no-underline hover:text-slate-400"
                @click="logout"
              >
                Logout
              </button>
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
