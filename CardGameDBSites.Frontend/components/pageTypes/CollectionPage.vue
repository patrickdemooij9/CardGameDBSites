<script setup lang="ts">
import { type CollectionSummaryApiModel } from "~/api/default";
import { DoFetch, DoOptionalServerFetch, DoServerFetch } from "~/helpers/RequestsHelper";
import SetOverview from "../overviews/SetOverview.vue";

const accountStore = useAccountStore();
const isLoggedIn = await useAccountStore().checkLogin();

const summaryData = ref<CollectionSummaryApiModel>({
  uniqueCards: 0,
  totalCards: 0,
  packsOpened: 0,
  marketPrice: 0,
});

onMounted(async () => {
  if (isLoggedIn) {
    summaryData.value = await DoServerFetch<CollectionSummaryApiModel>(
      "/api/collection/summary"
    );
  }
});
</script>

<template>
  <div class="container px-4 md:px-8 py-4">
    <div class="flex md:flex-row md:items-center flex-col gap-6">
      <!--@if (_collectionService.ShowProgressBar())
        {
            <div id="progress-bar" class="grow">
                @await Html.PartialAsync("~/Views/Partials/components/progressBar.cshtml", new ProgressBarViewModel(progress))
                <small>Your progress towards a full collection.</small>
            </div>
        }-->
      <div class="flex gap-4" v-if="accountStore.isLoggedIn">
        <!--@if (presets.Length > 0)
                {
                    <div class="rounded text-white" x-data="{ open: false }" x-on:click.outside="open = false">
                        <button type="button" class="btn flex gap-2 align-center" x-on:click="open = !open">
                            <span>Add preset</span>
                            <i class="ph ph-caret-down"></i>
                        </button>
                        <div class="absolute mt-2 z-10 max-h-72 rounded overflow-auto scrollbar:!w-1.5 scrollbar-thumb:!rounded scrollbar-thumb:!bg-slate-300 md:shadow-xl" :class="open ? '' : 'invisible'">
                            @foreach (var preset in presets)
                            {
                                <button class="btn px-3 py-2 cursor-pointer js-open-modal" js-modal="modal-preset-@preset.Key">
                                    @preset.DisplayName
                                </button>
                            }
                        </div>
                    </div>
                }
                <button class="btn js-open-modal" js-modal="modal-cardPack">
                    Add pack
                </button>
                @if (Model.ShowImportExport)
                {
                    <a class="btn" href="/umbraco/api/collection/exportcollection?exportType=Grouped">
                        Export
                    </a>
                    <button class="btn js-open-modal" js-modal="modal-import">
                        Import
                    </button>
                }-->
      </div>
    </div>
    <div
      class="flex md:flex-row flex-col mt-6 gap-4 justify-between text-white"
    >
      <div
        class="flex flex-col items-center px-4 py-2 w-full bg-red-600 rounded-md"
      >
        <p class="font-bold text-lg">{{ summaryData.uniqueCards }}</p>
        <p>Unique cards</p>
      </div>
      <div
        class="flex flex-col items-center px-4 py-2 w-full bg-blue-600 rounded-md"
      >
        <p class="font-bold text-lg">{{ summaryData.totalCards }}</p>
        <p>Total cards</p>
      </div>
      <div
        class="flex flex-col items-center px-4 py-2 w-full bg-green-600 rounded-md"
      >
        <p class="font-bold text-lg">{{ summaryData.packsOpened }}</p>
        <p>Packs opened</p>
      </div>
      <div
        class="flex flex-col items-center px-4 py-2 w-full bg-yellow-600 rounded-md"
      >
        <p class="font-bold text-lg">${{ summaryData.marketPrice }}</p>
        <p>Market price</p>
      </div>
    </div>
    <div class="mt-12">
      <div v-if="accountStore.isLoggedIn">
        <h2 class="text-lg font-bold">All sets</h2>
        <SetOverview />
      </div>
      <div v-else>
        <p class="text-lg font-bold text-center">
          <NuxtLink to="/login">Log in</NuxtLink> to keep track of your
          collection.
        </p>
      </div>
    </div>
  </div>
</template>
