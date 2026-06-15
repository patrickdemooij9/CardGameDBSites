<script setup lang="ts">
import { onMounted, type Component } from 'vue';
import { useRoute } from 'vue-router';
import type { IApiContentModelBase } from '~/api/umbraco';
import { DoFetch } from '~/helpers/RequestsHelper';
import type { ApiContentModel } from '~/models/ApiContentModel';
import type { PageSeoModel } from '~/models/PageSeoModel';
import { useAccountStore } from '~/stores/AccountStore';
import { Capacitor } from '@capacitor/core';

const route = useRoute()
let slug = route.params.slug;
if (Array.isArray(slug)){
    slug = slug.join('/');
}

console.time("fetching content");
const { data } = await useAsyncData('mainContentFetch-' + slug, () => DoFetch<ApiContentModel>("/umbraco/delivery/api/v2/content/item/" + slug));
console.timeEnd("fetching content");

const config = useRuntimeConfig();

useHead({
    title: data.value?.seoToolkit.title,
    meta: [
        { name: 'description', content: data.value?.seoToolkit.metaDescription },
        { property: 'og:title', content: data.value?.seoToolkit.openGraphTitle || data.value?.seoToolkit.title },
        { property: 'og:description', content: data.value?.seoToolkit.metaDescription },
        { property: 'og:image', content: data.value?.seoToolkit.openGraphImage },
        { property: 'og:url', content: data.value?.seoToolkit.canonicalUrl }
    ],
    link: [
        { rel: 'icon', href: `${config.public.API_BASE_URL}/favicon.ico` }
    ]
});

onMounted(() => {
    useAccountStore().checkLogin();
});

const componentName = data.value!.contentType;

// Mobile-specific page type overrides.
// When running in a Capacitor native shell, use mobile-optimized variants if available.
const mobilePageComponents: {[key: string]: Component} = {
    'cardOverview': defineAsyncComponent(() => import('~/components/mobile/MobileCardOverviewPage.vue')),
}

const pageComponents: {[key: string]: Component} = {
    'cardOverview': defineAsyncComponent(() => import('~/components/pageTypes/CardOverviewPage.vue')),
    'contentPage': defineAsyncComponent(() => import('~/components/pageTypes/contentPage.vue')),
    'deckDetail': defineAsyncComponent(() => import('~/components/pageTypes/DeckDetail.vue')),
    'deckOverview': defineAsyncComponent(() => import('~/components/pageTypes/DeckOverviewPage.vue')),
    'homepage': defineAsyncComponent(() => import('~/components/pageTypes/homepage.vue')),
    'card': defineAsyncComponent(() => import('~/components/pageTypes/CardDetailPage.vue')),
    'cardVariant': defineAsyncComponent(() => import('~/components/pageTypes/CardDetailPage.vue')),
    'createSquad': defineAsyncComponent(() => import('~/components/pageTypes/CreateDeck.vue')),
    'login': defineAsyncComponent(() => import('~/components/pageTypes/LoginPage.vue')),
    'accountDecks': defineAsyncComponent(() => import('~/components/pageTypes/AccountDecks.vue')),
    'register': defineAsyncComponent(() => import('~/components/pageTypes/RegisterPage.vue')),
    'forgotPassword': defineAsyncComponent(() => import('~/components/pageTypes/ForgotPasswordPage.vue')),
    'setOverview': defineAsyncComponent(() => import('~/components/pageTypes/SetOverviewPage.vue')),
    'set': defineAsyncComponent(() => import('~/components/pageTypes/SetPage.vue')),
    'collectionPage': defineAsyncComponent(() => import('~/components/pageTypes/CollectionPage.vue')),
    'blogOverview': defineAsyncComponent(() => import('~/components/pageTypes/BlogOverviewPage.vue')),
    'blogDetail': defineAsyncComponent(() => import('~/components/pageTypes/BlogDetailPage.vue')),
    'dailyGame': defineAsyncComponent(() => import('~/components/pageTypes/DailyGamePage.vue')),
    'proxyCards': defineAsyncComponent(() => import('~/components/pageTypes/ProxyCardsPage.vue')),
    'accountCards': defineAsyncComponent(() => import('~/components/pageTypes/AccountCards.vue'))
}

// Use mobile variant when in native app, fall back to default
const isNative = import.meta.client && Capacitor.isNativePlatform();
const pageComponent = (isNative && mobilePageComponents[componentName]) || pageComponents[componentName];
</script>

<template>
    <component :is="pageComponent" :content="data"></component>
</template>