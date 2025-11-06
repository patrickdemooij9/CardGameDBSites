<script setup lang="ts">
import type { IApiContentModel } from "~/api/umbraco";
import SetService from "~/services/SetService";
import CardOverview from "../overviews/CardOverview.vue";
import type { OverviewFilterModel } from "../overviews/OverviewFilterModel";
import SiteService from "~/services/SiteService";

const props = defineProps<{
  content: IApiContentModel;
}>();

const set = await new SetService().get(props.content.id!);
const settings = await new SiteService().getSetOverviewSettings();
const filters: OverviewFilterModel[] = settings.filters?.map<OverviewFilterModel>((filter) => {
  return {
    Alias: filter.alias,
    DisplayName: filter.displayName,
    IsInline: filter.isInline ?? false,
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
        </div>
      </div>
    </div>
    <CardOverview :filters="filters" :set-id="set.id"></CardOverview>
  </div>
</template>
