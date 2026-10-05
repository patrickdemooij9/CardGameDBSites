<script setup lang="ts">
import { PhCaretRight, PhSignIn, PhSignOut, PhUser } from "@phosphor-icons/vue";
import { AppTabPrefixes } from "~/composables/useAppSection";
import { useSite } from "~/composables/useSite";
import { useAccountStore } from "~/stores/AccountStore";

definePageMeta({ middleware: "native-only" });

const { getSettings, getNavigation } = useSite();
const siteSettings = await getSettings();
const navigation = await getNavigation();

useHead({ title: "More" });

const accountStore = useAccountStore();
const router = useRouter();

const sections = computed(() =>
  navigation.items.filter(
    (item) => !AppTabPrefixes.some((prefix) => item.url.startsWith(prefix))
  )
);

async function logout() {
  await accountStore.logout();
  router.push("/");
}
</script>

<template>
  <div class="pb-8">
    <div class="bg-white border-b border-gray-200 px-4 py-4">
      <template v-if="accountStore.isLoggedIn">
        <p class="flex items-center gap-2 font-semibold">
          <PhUser :size="20" />
          {{ accountStore.member!.name }}
        </p>
        <div class="mt-3 flex flex-col">
          <NuxtLink
            v-for="item in navigation.accountItems"
            :key="item.url"
            :to="item.url"
            class="flex items-center justify-between py-3 no-underline"
          >
            {{ item.name }}
            <PhCaretRight :size="16" class="text-gray-400" />
          </NuxtLink>
          <button class="flex items-center gap-2 py-3 text-left text-red-600" @click="logout">
            <PhSignOut :size="18" />
            Logout
          </button>
        </div>
      </template>

      <NuxtLink
        v-else-if="siteSettings.loginPageUrl"
        :to="siteSettings.loginPageUrl"
        class="flex items-center gap-2 py-2 font-semibold no-underline"
      >
        <PhSignIn :size="20" />
        Sign in
      </NuxtLink>
    </div>

    <div v-for="section in sections" :key="section.url" class="mt-4 bg-white border-y border-gray-200">
      <template v-if="section.children.length > 0">
        <p class="px-4 pt-3 text-xs uppercase tracking-wide text-gray-500">
          {{ section.name }}
        </p>
        <NuxtLink
          v-for="child in section.children"
          :key="child.url"
          :to="child.url"
          class="flex items-center justify-between px-4 py-3 no-underline"
        >
          {{ child.name }}
          <PhCaretRight :size="16" class="text-gray-400" />
        </NuxtLink>
      </template>
      <NuxtLink
        v-else
        :to="section.url"
        class="flex items-center justify-between px-4 py-3 no-underline"
      >
        {{ section.name }}
        <PhCaretRight :size="16" class="text-gray-400" />
      </NuxtLink>
    </div>

    <div class="mt-4 bg-white border-y border-gray-200">
      <NuxtLink to="/app/scanner" class="flex items-center justify-between px-4 py-3 no-underline">
        Card scanner
        <PhCaretRight :size="16" class="text-gray-400" />
      </NuxtLink>
    </div>

    <div class="mt-4 bg-white border-y border-gray-200">
      <NuxtLink to="/app/about" class="flex items-center justify-between px-4 py-3 no-underline">
        About
        <PhCaretRight :size="16" class="text-gray-400" />
      </NuxtLink>
    </div>
  </div>
</template>
