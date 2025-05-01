<script setup lang="ts">
import type {
  ApiBlockGridAreaModel,
  OneColumnLayoutElementModel,
} from "~/api/umbraco";
import Cards from "./Cards.vue";

defineProps<{
  content: OneColumnLayoutElementModel;
  areas: ApiBlockGridAreaModel[];
}>();

const componentMap: { [key: string]: Component } = {
  cards: Cards,
};
</script>

<template>
  <div class="container px-4 md:px-8 mb-16">
    <template v-for="item in areas.flatMap((area) => area.items!)">
      <component
        :is="componentMap[item.content!.contentType]"
        :content="item.content?.properties"
      ></component>
    </template>
  </div>
</template>
