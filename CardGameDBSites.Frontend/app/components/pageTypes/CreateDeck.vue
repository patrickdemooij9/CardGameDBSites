<script setup lang="ts">
import type { CreateSquadContentModel, SquadSettingsContentModel } from '~/api/umbraco';
import type { SquadSettingsOptionApiModel } from '~/api/default';
import DeckBuilder from '../decks/deckBuilder/DeckBuilder.vue';
import DeckTypeSelector from '../decks/DeckTypeSelector.vue';
import { ToOverviewModel } from '~/helpers/OverviewHelper';
import DeckService from '~/services/DeckService';

const props = defineProps<{
  content: CreateSquadContentModel;
}>();

const route = useRoute();

const filters = ToOverviewModel(props.content.properties?.filters?.items ?? []);

// Determine if we are editing an existing deck
const idQuery = route.query["id"];
const deckId: number | undefined = (idQuery && !Array.isArray(idQuery)) ? parseInt(idQuery) : undefined;

// Resolve which squad settings GUID to use
const selectedTypeId = ref<number | null>(null);

if (deckId) {
  const deck = await new DeckService().get(deckId);
  if (deck?.typeId) {
    selectedTypeId.value = deck!.typeId!;
  }
}

const squadOptions = ref<SquadSettingsOptionApiModel[]>([]);
if (!selectedTypeId.value) {
  squadOptions.value = await useSite().getSquadSettingsOptions();
  if (squadOptions.value.length === 1) {
    // If there's only one option, select it by default
    selectedTypeId.value = squadOptions.value[0]!.typeId!;
  }
}

function onSelectOption(option: SquadSettingsOptionApiModel) {
  selectedTypeId.value = option.typeId!;
}
</script>

<template>
  <DeckTypeSelector
      v-if="!selectedTypeId"
      :options="squadOptions"
      @select="onSelectOption"
    />
  <ClientOnly>
    <DeckBuilder
      v-if="selectedTypeId"
      :typeId="selectedTypeId"
      :filters="filters"
    />
  </ClientOnly>
</template>