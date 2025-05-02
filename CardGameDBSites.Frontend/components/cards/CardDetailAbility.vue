<script setup lang="ts">
import type { CardDetailApiModel, CardSectionApiModel } from "~/api/default";
import SpecialTextFormatter from "../shared/SpecialTextFormatter.vue";

const props = defineProps<{
  section: CardSectionApiModel;
  card: CardDetailApiModel;
}>();

const abilityValue = props.card.attributes![props.section.ability!];
</script>

<template>
  <template v-if="abilityValue">
    <h3 v-if="section.namePosition === 'Heading'">
      {{ props.section.ability }}
    </h3>
    <p>
      <b v-if="section.namePosition === 'Inline'"
        >{{ props.section.ability }}:</b
      >
      <template v-if="!section.showAsTags">
        <SpecialTextFormatter :content="abilityValue.join(', ')" />
      </template>
      <template v-else v-for="(item, index) in abilityValue" :key="index">
        <p
          v-if="!section.overviewPageUrl"
          class="p-1 rounded bg-main-color text-white inline-block text-xs"
        >
          {{ item }}
        </p>
        <a
          v-else
          :href="section.overviewPageUrl"
          class="p-1 rounded bg-main-color text-white inline-block text-xs no-underline"
          >{{ item }}</a
        >
      </template>
    </p>
  </template>
</template>
