<script setup lang="ts">
import type {
  CardAttributeContentModel,
  CardOverviewContentModel,
  SortingItemElementModel,
} from "~/api/umbraco";
import CardOverview, { type CardOverviewTableColumn } from "../overviews/CardOverview.vue";
import type { OverviewSortModel } from "../overviews/OverviewSortModel";
import type { OverviewUrlOptions } from "~/composables/useOverviewState";
import type { CardFilterPageContentModel } from "~/models/CardFilterPageContentModel";
import { NormalizePath } from "~/composables/useCardFilterLandings";
import { ToOverviewModel } from "~/helpers/OverviewHelper";

const PASSTHROUGH_PARAMS = ["search", "page", "sortBy", "view"];

const props = defineProps<{
  content: CardOverviewContentModel;
  landingContent?: CardFilterPageContentModel;
}>();

const route = useRoute();
const { collections, findLanding, findByUrl } = await useCardFilterLandings();
const overviewPath = NormalizePath(props.content.route.path);
const initialLanding = props.landingContent ? findByUrl(props.landingContent.route.path) : undefined;

const activeLandingPath = ref(initialLanding?.landing.url);
const landingContents = shallowReactive(new Map<string, CardFilterPageContentModel>());
if (initialLanding && props.landingContent) {
  landingContents.set(NormalizePath(initialLanding.landing.url), props.landingContent);
}
const displayedLanding = computed(() =>
  activeLandingPath.value ? landingContents.get(NormalizePath(activeLandingPath.value)) : undefined,
);

async function onLandingChange(path: string | undefined) {
  activeLandingPath.value = path;
  if (!path || landingContents.has(NormalizePath(path))) return;
  const content = await useCmsPage().loadContent(path);
  landingContents.set(NormalizePath(path), content as CardFilterPageContentModel);
}

const urlOptions: OverviewUrlOptions = {
  basePath: overviewPath,
  initialFilters: initialLanding
    ? { [initialLanding.collection.attribute]: [initialLanding.landing.value] }
    : undefined,
  resolveLandingPath: (alias, value) => findLanding(alias, value)?.landing.url,
  onLandingChange,
};

const browseCollections = computed(() => collections.value.filter((it) => it.landings.length > 0));

const filters = useCollectionFilter(
  ToOverviewModel(props.content.properties?.filters?.items ?? []),
);

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
    <template v-if="displayedLanding">
      <h1>{{ displayedLanding.properties?.title || `${displayedLanding.name} cards` }}</h1>
      <span
        v-if="displayedLanding.properties?.description"
        v-html="displayedLanding.properties.description.markup"
      ></span>
    </template>
    <template v-else>
      <h1>{{ content.properties?.title ?? "Cards" }}</h1>
      <span
        v-if="content.properties?.description"
        v-html="content.properties?.description.markup"
      ></span>
    </template>
  </div>
  <CardOverview
    :filters="filters"
    :sortings="sortings"
    :table-columns="tableColumns"
    :url-options="urlOptions"
  ></CardOverview>
  <div class="container px-4 md:px-8 my-8">
    <div
      v-if="displayedLanding?.properties?.content"
      class="prose max-w-3xl text-gray-800"
      v-html="displayedLanding.properties.content.markup"
    ></div>
    <nav v-else-if="!activeLandingPath && browseCollections.length > 0">
      <h2 class="mb-4 text-xl">Browse cards by</h2>
      <ul class="flex flex-wrap gap-2 list-none m-0">
        <li v-for="collection in browseCollections" :key="collection.url">
          <NuxtLink
            :to="collection.url"
            class="px-3 py-1 rounded bg-main-color text-white inline-block no-underline"
          >
            {{ collection.name }}
          </NuxtLink>
        </li>
      </ul>
    </nav>
  </div>
</template>
