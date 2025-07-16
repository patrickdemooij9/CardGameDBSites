<script setup lang="ts">
import { onMounted, type Component } from 'vue';
import { useRoute } from 'vue-router';
import type { IApiContentModelBase } from '~/api/umbraco';
import AccountDecks from '~/components/pageTypes/AccountDecks.vue';
import CardDetailPage from '~/components/pageTypes/CardDetailPage.vue';
import CardOverviewPage from '~/components/pageTypes/CardOverviewPage.vue';
import ContentPage from '~/components/pageTypes/contentPage.vue';
import CreateDeck from '~/components/pageTypes/CreateDeck.vue';
import DeckDetail from '~/components/pageTypes/DeckDetail.vue';
import DeckOverviewPage from '~/components/pageTypes/DeckOverviewPage.vue';
import ForgotPasswordPage from '~/components/pageTypes/ForgotPasswordPage.vue';
import Homepage from '~/components/pageTypes/homepage.vue';
import LoginPage from '~/components/pageTypes/LoginPage.vue';
import RegisterPage from '~/components/pageTypes/RegisterPage.vue';
import { DoFetch } from '~/helpers/RequestsHelper';
import type { PageSeoModel } from '~/models/PageSeoModel';
import { useAccountStore } from '~/stores/AccountStore';

const route = useRoute()
let slug = route.params.slug;
if (Array.isArray(slug)){
    slug = slug.join('/');
}
const data = await DoFetch<IApiContentModelBase>("/umbraco/delivery/api/v2/content/item/" + slug);
const seo = await DoFetch<PageSeoModel>("/api/seo?contentGuid=" + data.id);
useHead({
    title: seo.metaFields.title,
    meta: [
        { name: 'description', content: seo.metaFields.metaDescription },
        { property: 'og:title', content: seo.metaFields.openGraphTitle || seo.metaFields.title },
        { property: 'og:description', content: seo.metaFields.metaDescription },
        { property: 'og:image', content: seo.metaFields.openGraphImage },
        { property: 'og:url', content: seo.metaFields.canonicalUrl }
    ]
});

onMounted(() => {
    useAccountStore().checkLogin();
});

const componentName = data.contentType;
const pageComponents: {[key: string]: Component} = {
    'cardOverview': CardOverviewPage,
    'contentPage': ContentPage,
    'deckDetail': DeckDetail,
    'deckOverview': DeckOverviewPage,
    'homepage': Homepage,
    'card': CardDetailPage,
    'createSquad': CreateDeck,
    'login': LoginPage,
    'accountDecks': AccountDecks,
    'register': RegisterPage,
    'forgotPassword': ForgotPasswordPage
}
const pageComponent = pageComponents[componentName];
</script>

<template>
    <component :is="pageComponent" :content="data"></component>
</template>