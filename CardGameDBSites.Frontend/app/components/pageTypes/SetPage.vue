<script setup lang="ts">
import type { IApiContentModel } from "~/api/umbraco";
import SetService from "~/services/SetService";
import CardOverview from "../overviews/CardOverview.vue";
import { OverviewFilterType, type OverviewFilterModel } from "../overviews/OverviewFilterModel";
import { useSite } from "~/composables/useSite";

const props = defineProps<{
  content: IApiContentModel;
}>();

const set = await new SetService().get(props.content.id!);
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
const sortings = settings.sortOptions?.map((sort) => {
  return {
    Name: sort.name,
    Value: sort.value,
  };
}) ?? [];

onMounted(async () => {
  const accountStore = useAccountStore();
  await accountStore.checkLogin();
  if (accountStore.isLoggedIn) {
    filters.push({
      Alias: "collection",
      DisplayName: "Collection",
      Type: OverviewFilterType.DROPDOWN,
      Items: [
        {
          DisplayName: "In collection",
          Value: "inCollection",
        },
        {
          DisplayName: "No copies",
          Value: "none",
        },
        {
          DisplayName: "Missing copies",
          Value: "missing",
        }
      ],
      AutoFillValues: false
    });
  }
});
</script>

<template>
  <div class="mt-8">
    <div class="container px-4 md:px-8 mb-12">
      <div class="flex md:flex-row flex-col gap-6">
        <div class="" v-if="set.imageUrl">
          <img :src="set.imageUrl" />
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
    <CardOverview :filters="filters" :sortings="sortings" :set-id="set.id" :page-size="1000"></CardOverview>
  </div>
</template>
