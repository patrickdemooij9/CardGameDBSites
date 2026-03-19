<script setup lang="ts">
import type { CardDetailApiModel, DeckApiModel, DeckTypeSettingsApiModel, MemberApiModel } from '~/api/default';
import FullHeaderDeckCard from './FullHeaderDeckCard.vue';
import CompactDeckCard from './CompactDeckCard.vue';

const props = defineProps<{
    deck: DeckApiModel,
    member?: MemberApiModel,
    cards?: Record<number, CardDetailApiModel>,
    settings?: DeckTypeSettingsApiModel
}>();
</script>

<template>
    <CompactDeckCard :deck="deck" :settings="settings!" :member="member" :cards="cards"></CompactDeckCard>
</template>