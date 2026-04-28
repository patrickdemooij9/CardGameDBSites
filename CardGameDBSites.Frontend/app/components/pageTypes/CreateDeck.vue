<script setup lang="ts">
import type { CreateSquadContentModel, CreateSquadPropertiesModel, SquadSettingsContentModel } from '~/api/umbraco';
import DeckBuilder from '../decks/deckBuilder/DeckBuilder.vue';
import { ToOverviewModel } from '~/helpers/OverviewHelper';

const props = defineProps<{
  content: CreateSquadContentModel;
}>();

const filters = ToOverviewModel(props.content.properties?.filters?.items ?? []);
</script>

<template>
    <ClientOnly>
        <DeckBuilder :typeId="(content.properties?.settingType![0] as SquadSettingsContentModel)!.id!" :filters="filters"></DeckBuilder>
    </ClientOnly>
</template>