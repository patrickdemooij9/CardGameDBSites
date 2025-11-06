<script setup lang="ts">
import type {
  CardOverviewContentModel,
  IApiContentModel,
  OverviewFilterElementModel,
  OverviewFilterItemPropertiesModel,
} from "~/api/umbraco";
import CardOverview from "../overviews/CardOverview.vue";
import type {
  OverviewFilterItemModel,
  OverviewFilterModel,
} from "../overviews/OverviewFilterModel";
import { GetAbsoluteUrl } from "~/helpers/CropUrlHelper";

const props = defineProps<{
  content: CardOverviewContentModel;
}>();

const filters =
  props.content.properties?.filters?.items?.map<OverviewFilterModel>((item) => {
    const castedItem = (item.content as OverviewFilterElementModel).properties!;
    return {
      Alias: castedItem.isExpansionFilter
        ? "Set Name"
        : (castedItem.ability as IApiContentModel[])[0].name ?? "",
      DisplayName: castedItem?.displayName ?? "",
      IsInline: castedItem?.isInline ?? false,
      AutoFillValues: castedItem?.autoFillValues ?? false,
      Items:
        castedItem?.items?.items?.map<OverviewFilterItemModel>((child) => {
          const castedChild = child.content
            ?.properties as OverviewFilterItemPropertiesModel;
          return {
            DisplayName: castedChild?.displayName ?? "",
            Value: castedChild?.value ?? "",
            IconUrl: castedChild?.icon
              ? GetAbsoluteUrl(castedChild!.icon![0].url!)
              : "",
          };
        }) ?? [],
    };
  }) ?? [];
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <h1>{{ content.properties?.title ?? "Cards" }}</h1>
    <span
      v-if="content.properties?.description"
      v-html="content.properties?.description.markup"
    ></span>
  </div>
  <CardOverview :filters="filters"></CardOverview>
</template>
