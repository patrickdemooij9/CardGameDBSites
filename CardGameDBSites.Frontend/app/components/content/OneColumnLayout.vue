<script setup lang="ts">
import type {
  ApiBlockGridAreaModel,
  OneColumnLayoutElementModel,
} from "~/api/umbraco";
import Cards from "./Cards.vue";
import DeckEmbed from "./DeckEmbed.vue";
import Form from "./Form.vue";
import PriceChanges from "./PriceChanges.vue";
import Text from "./Text.vue";
import WinningDecks from "./WinningDecks.vue";

const props = defineProps<{
  content: OneColumnLayoutElementModel;
  areas: ApiBlockGridAreaModel[];
  banded?: boolean;
}>();

const componentMap: { [key: string]: Component } = {
  cards: Cards,
  'embeddedDeck': DeckEmbed,
  form: Form,
  'priceChanges': PriceChanges,
  text: Text,
  winningDecks: WinningDecks,
};

const items = computed(() =>
  props.areas.flatMap((area) => area.items!).filter((item) => componentMap[item.content!.contentType]),
);
</script>

<template>
  <section v-if="items.length" :class="{ 'border-t border-line py-12 text-ink even:bg-surface': banded }">
    <div :class="{ 'mb-16': !banded }" class="container px-4 md:px-8">
      <component
        :is="componentMap[item.content!.contentType]"
        v-for="item in items"
        :content="item.content?.properties"
      ></component>
    </div>
  </section>
</template>
