<script setup lang="ts">
import type { CardDetailApiModel, CardSectionApiModel } from "~/api/default";
import SpecialTextFormatter from "../shared/SpecialTextFormatter.vue";

const props = defineProps<{
  section: CardSectionApiModel;
  card: CardDetailApiModel;
}>();

const abilityValue = props.card.attributes![props.section.ability!];

// Bit ugly. Should eventually replace with something like sitesettings
function trimUrlDomain(url: string) {
  try {
    const parsedUrl = new URL(url);
    return parsedUrl.pathname + parsedUrl.search + parsedUrl.hash;
  } catch (e) {
    return url;
  }
}
</script>

<template>
  <div v-if="abilityValue">
    <h3 v-if="section.namePosition === 'Heading'">
      {{ section.ability }}
    </h3>
    <div>
      <b v-if="section.namePosition === 'Inline'"> {{ section.ability }}: </b>
      <div v-if="!section.showAsTags" v-for="(item, index) in abilityValue" :key="index">
        <template v-if="item.includes(';')">
          <p><b><SpecialTextFormatter :content="item.split(';')[0]!"/></b></p>
          <p><SpecialTextFormatter :content="item.split(';')[1]!"/></p>
        </template>
        <SpecialTextFormatter v-else :content="item" />
      </div>

      <template v-else>
        <div class="flex gap-2">
          <p
            v-for="(item, index) in abilityValue"
            v-if="!section.overviewPageUrl"
            :key="index"
            class="p-1 rounded bg-main-color text-white inline-block text-xs"
          >
            {{ item }}
          </p>
          <NuxtLink
            v-for="(item, abilityIndex) in abilityValue"
            v-else
            :key="abilityIndex"
            :to="`${trimUrlDomain(section.overviewPageUrl!)}?${section.ability}=${encodeURIComponent(item)}`"
            class="p-1 rounded bg-main-color text-white inline-block text-xs no-underline"
          >
            {{ item }}
          </NuxtLink>
        </div>
      </template>
    </div>
  </div>
</template>
