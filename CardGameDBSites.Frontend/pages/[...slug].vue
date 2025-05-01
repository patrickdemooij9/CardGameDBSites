<script setup lang="ts">
import type { Component } from 'vue';
import type { IApiContentModelBase } from '~/api/umbraco';
import CardOverviewPage from '~/components/pageTypes/CardOverviewPage.vue';
import ContentPage from '~/components/pageTypes/contentPage.vue';
import DeckDetail from '~/components/pageTypes/DeckDetail.vue';
import DeckOverview from '~/components/pageTypes/DeckOverview.vue';
import Homepage from '~/components/pageTypes/homepage.vue';
import { firstCharUppercase } from '~/helpers/StringHelpers';

const route = useRoute()
let slug = route.params.slug;
if (Array.isArray(slug)){
    slug = slug.join('/');
}
const { data, error } = await useFetch<IApiContentModelBase>("https://localhost:44344/umbraco/delivery/api/v2/content/item/" + slug);

if (!data.value) {
    // Do something?
}
const componentName = data.value!.contentType;
const pageComponents: {[key: string]: Component} = {
    'cardOverview': CardOverviewPage,
    'contentPage': ContentPage,
    'deckDetail': DeckDetail,
    'deckOverview': DeckOverview,
    'homepage': Homepage
}
const pageComponent = pageComponents[componentName];
</script>

<template>
    <component :is="pageComponent" :content="data"></component>
</template>