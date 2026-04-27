<script setup lang="ts">
import type {
  CardAttributeContentModel,
  CardOverviewContentModel,
  IApiContentModel,
  OverviewFilterElementModel,
  OverviewFilterItemPropertiesModel,
  SortingItemElementModel,
} from "~/api/umbraco";
import CardOverview, { type CardOverviewTableColumn } from "../overviews/CardOverview.vue";
import {
  OverviewFilterType,
  type OverviewFilterItemModel,
  type OverviewFilterModel,
} from "../overviews/OverviewFilterModel";
import type { OverviewSortModel } from "../overviews/OverviewSortModel";
import { GetAbsoluteUrl } from "~/helpers/CropUrlHelper";
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
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <h1>{{ content.properties?.title ?? "Cards" }}</h1>
    <span
      v-if="content.properties?.description"
      v-html="content.properties?.description.markup"
    ></span>
  </div>
  <CardOverview :filters="filters" :sortings="sortings" :table-columns="tableColumns"></CardOverview>
</template>
