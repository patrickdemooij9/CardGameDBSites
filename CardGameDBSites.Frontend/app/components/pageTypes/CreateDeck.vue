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
const selectedTypeId = ref<string | null>(null);

if (deckId) {
  // Editing: get the deck to find its typeId, then find the matching squad settings option
  const deck = await new DeckService().get(deckId);
  if (deck?.typeId) {
    const options = await useSite().getSquadSettingsOptions();
    const match = options.find((o) => o.typeId === deck.typeId);
    if (match) {
      selectedTypeId.value = match.id;
    }
  }
} else {
  // Creating: check if the page has a pre-configured settingType
  const pageSettingType = props.content.properties?.settingType?.[0] as SquadSettingsContentModel | undefined;
  if (pageSettingType?.id) {
    selectedTypeId.value = pageSettingType.id;
  }
}

// Squad settings options for the selector (only loaded when needed)
const squadOptions = ref<SquadSettingsOptionApiModel[]>([]);
if (!selectedTypeId.value) {
  squadOptions.value = await useSite().getSquadSettingsOptions();
}

function onSelectOption(option: SquadSettingsOptionApiModel) {
  selectedTypeId.value = option.id;
}

const resolvedTypeId = computed(() => selectedTypeId.value ?? '');
</script>

<template>
  <ClientOnly>
    <DeckTypeSelector
      v-if="!selectedTypeId"
      :options="squadOptions"
      @select="onSelectOption"
    />
    <DeckBuilder
      v-else
      :typeId="resolvedTypeId"
      :filters="filters"
    />
  </ClientOnly>
</template>