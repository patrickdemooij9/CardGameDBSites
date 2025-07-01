<script setup lang="ts">
import type { DeckApiModel } from '~/api/default';
import SiteService from '~/services/SiteService';
import FullHeaderDeckCard from './FullHeaderDeckCard.vue';
import CompactDeckCard from './CompactDeckCard.vue';

const props = defineProps<{
    deck: DeckApiModel
}>();

const deckTypeSettings = await new SiteService().getDeckTypeSettings(props.deck.typeId!);
</script>

<template>
    <CompactDeckCard :deck="deck" :settings="deckTypeSettings!"></CompactDeckCard>
</template>