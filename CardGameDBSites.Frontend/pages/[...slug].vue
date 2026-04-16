<script setup lang="ts">
import { onMounted, type Component } from 'vue';
import { useRoute } from 'vue-router';
import type { IApiContentModelBase } from '~/api/umbraco';
import { DoFetch } from '~/helpers/RequestsHelper';
import type { PageSeoModel } from '~/models/PageSeoModel';
import { useAccountStore } from '~/stores/AccountStore';

const route = useRoute()
let slug = route.params.slug;
if (Array.isArray(slug)){
    slug = slug.join('/');
}
const { data } = await useAsyncData('mainContentFetch-' + slug, () => DoFetch<IApiContentModelBase>("/umbraco/delivery/api/v2/content/item/" + slug));
const { data: seo } = await useAsyncData('seoFetch-' + slug, () => DoFetch<PageSeoModel>("/api/seo?contentGuid=" + data.value!.id));
const config = useRuntimeConfig();

useHead({
    title: seo.value!.metaFields.title,
    meta: [
        { name: 'description', content: seo.value!.metaFields.metaDescription },
        { property: 'og:title', content: seo.value!.metaFields.openGraphTitle || seo.value!.metaFields.title },
        { property: 'og:description', content: seo.value!.metaFields.metaDescription },
        { property: 'og:image', content: seo.value!.metaFields.openGraphImage },
        { property: 'og:url', content: seo.value!.metaFields.canonicalUrl }
    ],
    link: [
        { rel: 'icon', href: `${config.public.API_BASE_URL}/favicon.ico` },
        { rel: 'canonical', href: seo.value!.metaFields.canonicalUrl }
    ]
});

onMounted(() => {
    useAccountStore().checkLogin();
});

const componentName = data.value!.contentType;
const pageComponents: {[key: string]: Component} = {
    'cardOverview': defineAsyncComponent(() => import('~/components/pageTypes/CardOverviewPage.vue')),
    'contentPage': defineAsyncComponent(() => import('~/components/pageTypes/contentPage.vue')),
    'deckDetail': defineAsyncComponent(() => import('~/components/pageTypes/DeckDetail.vue')),
    'deckOverview': defineAsyncComponent(() => import('~/components/pageTypes/DeckOverviewPage.vue')),
    'homepage': defineAsyncComponent(() => import('~/components/pageTypes/homepage.vue')),
    'card': defineAsyncComponent(() => import('~/components/pageTypes/CardDetailPage.vue')),
    'createSquad': defineAsyncComponent(() => import('~/components/pageTypes/CreateDeck.vue')),
    'login': defineAsyncComponent(() => import('~/components/pageTypes/LoginPage.vue')),
    'accountDecks': defineAsyncComponent(() => import('~/components/pageTypes/AccountDecks.vue')),
    'register': defineAsyncComponent(() => import('~/components/pageTypes/RegisterPage.vue')),
    'forgotPassword': defineAsyncComponent(() => import('~/components/pageTypes/ForgotPasswordPage.vue')),
    'setOverview': defineAsyncComponent(() => import('~/components/pageTypes/SetOverviewPage.vue')),
    'set': defineAsyncComponent(() => import('~/components/pageTypes/SetPage.vue')),
    'collectionPage': defineAsyncComponent(() => import('~/components/pageTypes/CollectionPage.vue')),
    'blogOverview': defineAsyncComponent(() => import('~/components/pageTypes/BlogOverviewPage.vue')),
    'blogDetail': defineAsyncComponent(() => import('~/components/pageTypes/BlogDetailPage.vue'))
}
const pageComponent = pageComponents[componentName];
</script>

<template>
    <component :is="pageComponent" :content="data"></component>
</template>