<script setup lang="ts">
import { CardSearchCollectionMode } from "~/api/default";
import type { AccountCardsContentModel } from "~/api/umbraco";
import CardOverview from "~/components/overviews/CardOverview.vue";
import type { OverviewFilterModel } from "~/components/overviews/OverviewFilterModel";
import { ToOverviewModel } from "~/helpers/OverviewHelper";
import { useAccountStore } from "~/stores/AccountStore";

const props = defineProps<{
    content: AccountCardsContentModel
}>();

const accountStore = useAccountStore();
const isLoading = ref(true);
const isLoggedIn = ref(false);
const filters = ToOverviewModel(props.content.properties?.filters?.items ?? []);

onMounted(async () => {
  isLoggedIn.value = await accountStore.checkLogin();
  isLoading.value = false;
});
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <h1>{{ content.properties?.title ?? "Your cards" }}</h1>
    <span
      v-if="content.properties?.description"
      v-html="content.properties?.description.markup"
    ></span>
  </div>

  <div v-if="isLoading" class="flex justify-center items-center py-12">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>

  <CardOverview
    v-else-if="isLoggedIn"
    :filters="filters"
    :collection-mode="CardSearchCollectionMode.IN_COLLECTION"
  />

  <div v-else class="container px-4 md:px-8 pb-8">
    <p class="text-lg font-bold text-center">
      <NuxtLink to="/login">Log in</NuxtLink> to see your collection cards.
    </p>
  </div>
</template>
