<script setup lang="ts">
import type { CardFilterPageContentModel } from "~/models/CardFilterPageContentModel";
import { NormalizePath, ToGeneratedLandingContent } from "~/composables/useCardFilterLandings";
import CardFilterLandingPage from "./CardFilterLandingPage.vue";

const props = defineProps<{
  content: CardFilterPageContentModel;
}>();

const route = useRoute();
const { findCollectionByUrl, findByUrl } = await useCardFilterLandings();
const landings = findCollectionByUrl(props.content.route.path)?.landings ?? [];

// Generated landings have no CMS node, so the delivery API resolves them to this collection.
const generatedLanding =
  NormalizePath(route.path) !== NormalizePath(props.content.route.path)
    ? findByUrl(route.path)?.landing
    : undefined;
const generatedLandingContent = generatedLanding?.isGenerated
  ? ToGeneratedLandingContent(generatedLanding, props.content)
  : undefined;
</script>

<template>
  <CardFilterLandingPage v-if="generatedLandingContent" :content="generatedLandingContent"></CardFilterLandingPage>
  <template v-else>
    <div class="container px-4 pt-8 md:px-8 mb-6">
      <h1>{{ content.properties?.title || content.name }}</h1>
      <span
        v-if="content.properties?.description"
        v-html="content.properties.description.markup"
      ></span>
    </div>
    <div class="container px-4 md:px-8">
      <ul class="grid gap-3 grid-cols-2 md:grid-cols-3 lg:grid-cols-4 list-none m-0">
        <li v-for="landing in landings" :key="landing.url">
          <NuxtLink
            :to="landing.url"
            class="flex items-center justify-between gap-2 p-4 rounded border border-gray-300 bg-white no-underline hover:border-main-color"
          >
            <span class="font-bold">{{ landing.name }}</span>
            <span class="text-sm text-gray-600">{{ landing.cardCount }} cards</span>
          </NuxtLink>
        </li>
      </ul>
    </div>
    <div class="container px-4 md:px-8 my-8">
      <div
        v-if="content.properties?.content"
        class="prose max-w-3xl text-gray-800"
        v-html="content.properties.content.markup"
      ></div>
    </div>
  </template>
</template>
