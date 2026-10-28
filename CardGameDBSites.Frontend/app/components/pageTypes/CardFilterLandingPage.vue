<script setup lang="ts">
import type { CardOverviewContentModel } from "~/api/umbraco";
import type { CardFilterPageContentModel } from "~/models/CardFilterPageContentModel";
import CardOverviewPage from "./CardOverviewPage.vue";

const props = defineProps<{
  content: CardFilterPageContentModel;
}>();

const { findByUrl } = await useCardFilterLandings();
const match = findByUrl(props.content.route.path);
if (!match) {
  throw createError({ statusCode: 404, statusMessage: "Resource Not Found" });
}

const collectionUrl = match.collection.url;
const overviewPath = collectionUrl.substring(0, collectionUrl.lastIndexOf("/"));
const { data: overview } = await useAsyncData(`card-filter-overview-${overviewPath}`, () =>
  useCmsPage().loadContent(overviewPath),
);
if (!overview.value) {
  throw createError({ statusCode: 404, statusMessage: "Resource Not Found" });
}
</script>

<template>
  <CardOverviewPage
    :content="overview as CardOverviewContentModel"
    :landing-content="content"
  ></CardOverviewPage>
</template>
