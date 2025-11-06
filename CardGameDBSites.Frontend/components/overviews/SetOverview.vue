<script setup lang="ts">
import SetService from "~/services/SetService";

const sets = await new SetService().getAllSets();
</script>

<template>
  <div class="grid md:grid-cols-5 sm:grid-cols-2 grid-cols-1 gap-6">
    <div
      v-for="set in sets"
      class="flex flex-col gap-2 rounded-md border-2 border-gray-300 bg-white p-4"
    >
      <NuxtLink
        :to="set.urlSegment"
        class="flex gap-2 items-baseline text-base no-underline"
      >
        <h2 class="text-base font-bold">{{ set.displayName }}</h2>
        <i v-if="set.code" class="text-xs">{{ set.code }}</i>
      </NuxtLink>
      <div class="flex justify-between">
        <img class="h-16" :src="set.imageUrl ?? '#'" />
        <div class="flex flex-col justify-center">
          <span v-for="info in set.extraInformation" class="text-sm">{{
            info
          }}</span>
        </div>
      </div>
      <div class="flex justify-between gap-4">
        <div class="grow self-center">
          <!--@if (isLoggedIn)
                    {
                        if (collectionSettings.AllowSetCollecting)
                        {
                            <div id="collection-@set.Id">
                                @await Html.PartialAsync("~/Views/Partials/components/collectionButton.cshtml", new CollectionButtonViewModel()
                                {
                                    SetId = set.Id,
                                    ToAdd = !currentSets.Contains(set.Id)
                                })
                            </div>
                        }
                        else if (collectionSettings.AllowCardCollecting && collectionSettings.ShowProgressBar)
                        {
                            var setProgress = _collectionService.CalculateCollectionProgressBySet(set.Id, out var totalCards, out var collectionCards);
                            @await Html.PartialAsync("~/Views/Partials/components/progressBar.cshtml", new ProgressBarViewModel(setProgress)
                            {
                                Description = $"{collectionCards}/{totalCards}"
                            })
                        }
                        }-->
        </div>
        <NuxtLink
          :to="set.urlSegment"
          class="border border-solid flex gap-2 rounded items-center px-2 py-1 bg-white"
        >
          <PhChartBar />
          <span>Details</span>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>
