<script setup lang="ts">
import type { CreateDeckContentModel, CreateSquadContentModel, CreateSquadPropertiesModel, SquadSettingsContentModel } from '~/api/umbraco';
import DeckBuilder from '../decks/deckBuilder/DeckBuilder.vue';

defineProps<{
  content: CreateSquadContentModel;
}>();
</script>

<template>
    <ClientOnly>
        <DeckBuilder :typeId="(content.properties?.settingType![0] as SquadSettingsContentModel)!.id!"></DeckBuilder>
    </ClientOnly>
</template>