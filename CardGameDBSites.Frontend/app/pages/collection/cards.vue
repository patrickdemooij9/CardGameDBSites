<script setup lang="ts">
import { CardSearchCollectionMode } from "~/api/default";
import CardOverview from "~/components/overviews/CardOverview.vue";
import type { OverviewFilterModel } from "~/components/overviews/OverviewFilterModel";
import { useAccountStore } from "~/stores/AccountStore";

const accountStore = useAccountStore();
const isLoggedIn = await accountStore.checkLogin();
const filters: OverviewFilterModel[] = [];
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <h1>My collection cards</h1>
    <span>All cards currently in your collection.</span>
  </div>

  <CardOverview
    v-if="isLoggedIn"
    :filters="filters"
    :collection-mode="CardSearchCollectionMode.IN_COLLECTION"
  />

  <div v-else class="container px-4 md:px-8 pb-8">
    <p class="text-lg font-bold text-center">
      <NuxtLink to="/login">Log in</NuxtLink> to see your collection cards.
    </p>
  </div>
</template>
