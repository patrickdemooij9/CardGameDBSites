<script setup lang="ts">
/**
 * Mobile version of CardOverviewPage.
 * 
 * Differences from the web version:
 * - No title/description text — jumps straight to the card grid
 * - Optimized for touch interaction and smaller viewports
 */
import type {
  CardAttributeContentModel,
  CardOverviewContentModel,
  SortingItemElementModel,
} from "~/api/umbraco";
import CardOverview, { type CardOverviewTableColumn } from "../overviews/CardOverview.vue";
import type { OverviewSortModel } from "../overviews/OverviewSortModel";
import { ToOverviewModel } from "~/helpers/OverviewHelper";

const props = defineProps<{
  content: CardOverviewContentModel;
}>();

const filters = ToOverviewModel(props.content.properties?.filters?.items ?? []);

const sortings =
  props.content.properties?.sortings?.items?.map<OverviewSortModel>((item) => {
    const castedItem = (item.content as SortingItemElementModel).properties;
    return {
      Name: castedItem?.displayName ?? "",
      Value: castedItem?.value ?? "",
    };
  }) ?? [];

const tableColumns: CardOverviewTableColumn[] =
  props.content.properties?.attributesToShow
    ?.map<CardOverviewTableColumn>((item) => {
      const castedItem = item as CardAttributeContentModel;
      return {
        alias: castedItem.name ?? "",
        displayName: castedItem.properties?.displayName ?? castedItem.name ?? "",
      };
    })
    .filter((col) => col.alias !== "") ?? [];
</script>

<template>
  <div class="pt-4 pb-4 px-2">
    <CardOverview :filters="filters" :sortings="sortings" :table-columns="tableColumns"></CardOverview>
  </div>
</template>
