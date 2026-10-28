<script setup lang="ts">
import type { CardFilterPageContentModel } from "~/models/CardFilterPageContentModel";

const props = defineProps<{
  content: CardFilterPageContentModel;
}>();

const { findCollectionByUrl } = await useCardFilterLandings();
const landings = findCollectionByUrl(props.content.route.path)?.landings ?? [];
</script>

<template>
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
