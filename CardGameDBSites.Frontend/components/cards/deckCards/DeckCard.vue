<script setup lang="ts">
import type { DeckApiModel } from '~/api/default';
import SiteService from '~/services/SiteService';
import FullHeaderDeckCard from './FullHeaderDeckCard.vue';

const props = defineProps<{
    deck: DeckApiModel
}>();

const deckTypeSettings = await new SiteService().getDeckTypeSettings(props.deck.typeId!);
</script>

<template>
    <FullHeaderDeckCard :deck="deck" :settings="deckTypeSettings!"></FullHeaderDeckCard>
</template>