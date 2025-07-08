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
  <div v-if="abilityValue">
    <h3 v-if="section.namePosition === 'Heading'">
      {{ props.section.ability }}
    </h3>
    <div>
      <b v-if="section.namePosition === 'Inline'">
        {{ props.section.ability }}:
      </b>
      <SpecialTextFormatter
        v-if="!section.showAsTags"
        :content="abilityValue.join(', ')"
      />
      <template v-else>
        <p
          v-for="(item, index) in abilityValue"
          v-if="!section.overviewPageUrl"
          :key="index"
          class="p-1 rounded bg-main-color text-white inline-block text-xs"
        >
          {{ item }}
        </p>
        <a
          v-for="(item, abilityIndex) in abilityValue"
          v-else
          :key="abilityIndex"
          :href="section.overviewPageUrl"
          class="p-1 rounded bg-main-color text-white inline-block text-xs no-underline"
        >
          {{ item }}
        </a>
      </template>
    </div>
  </div>
</template>