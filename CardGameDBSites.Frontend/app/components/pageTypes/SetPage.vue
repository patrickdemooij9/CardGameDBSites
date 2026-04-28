<script setup lang="ts">
import type { IApiContentModel } from "~/api/umbraco";
import SetService from "~/services/SetService";
import CardOverview from "../overviews/CardOverview.vue";
import { OverviewFilterType, type OverviewFilterModel } from "../overviews/OverviewFilterModel";
import { useSite } from "~/composables/useSite";
import SetPriceHistoryChart from "../cards/SetPriceHistoryChart.vue";

const props = defineProps<{
  content: IApiContentModel;
}>();

const setService = new SetService();
const set = await setService.get(props.content.id!);
const priceHistory = await setService.getPriceHistory(set.id);
const currentPrice = priceHistory && priceHistory.length > 0
  ? priceHistory[priceHistory.length - 1]!.price
  : null;

const settings = await useSite().getSetOverviewSettings();
const filters: OverviewFilterModel[] = settings.filters?.map<OverviewFilterModel>((filter) => {
  return {
    Alias: filter.alias,
    DisplayName: filter.displayName,
    Type: filter.isInline ? OverviewFilterType.INLINE : OverviewFilterType.DROPDOWN,
    AutoFillValues: filter.autoFillValues ?? false,
    Items: filter.options?.map((item) => {
      return {
        DisplayName: item.displayName,
        Value: item.value,
        IconUrl: item.iconUrl ?? "",
      };
    }) ?? [],
  };
}) ?? [];
</script>

<template>
  <div class="mt-8">
    <div class="container px-4 md:px-8 mb-12">
      <div class="flex md:flex-row flex-col gap-6">
        <div class="">
          <img :src="set.imageUrl ?? '#'" />
        </div>
        <div class="w-full">
          <h1 class="mb-2">{{ set.displayName }}</h1>
          <div v-if="set.extraInformation">
            <p v-for="info in set.extraInformation">
              {{ info }}
            </p>
          </div>
          <div v-if="currentPrice !== null" class="mt-4">
            <p class="text-lg font-semibold">Set Value: <span class="text-green-700">${{ currentPrice.toFixed(2) }}</span></p>
          </div>
        </div>
      </div>
      <SetPriceHistoryChart v-if="priceHistory && priceHistory.length > 1" :set-id="set.id" />
    </div>
    <CardOverview :filters="filters" :set-id="set.id"></CardOverview>
  </div>
</template>

