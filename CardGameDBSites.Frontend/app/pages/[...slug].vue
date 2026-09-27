<script setup lang="ts">
import { onMounted } from 'vue';
import { useRoute } from 'vue-router';
import type { IApiContentModelBase } from '~/api/umbraco';
import { DoFetch } from '~/helpers/RequestsHelper';
import { useCmsPage } from '~/composables/useCmsPage';
import type { ApiContentModel } from '~/models/ApiContentModel';
import type { PageSeoModel } from '~/models/PageSeoModel';
import { useAccountStore } from '~/stores/AccountStore';

const route = useRoute()
let slug = route.params.slug;
if (Array.isArray(slug)){
    slug = slug.join('/');
}

console.time("fetching content");
const { data } = await useAsyncData('mainContentFetch-' + slug, () => DoFetch<ApiContentModel>("/umbraco/delivery/api/v2/content/item/" + slug));
console.timeEnd("fetching content");

if (!data.value) {
    throw createError({
    statusCode: 404,
    statusMessage: "Resource Not Found",
  });
}

const config = useRuntimeConfig();

useHead({
    title: data.value.seoToolkit.title,
    meta: [
        { name: 'description', content: data.value.seoToolkit.metaDescription },
        { property: 'og:title', content: data.value.seoToolkit.openGraphTitle || data.value?.seoToolkit.title },
        { property: 'og:description', content: data.value.seoToolkit.metaDescription },
        { property: 'og:image', content: data.value.seoToolkit.openGraphImage },
        { property: 'og:url', content: data.value.seoToolkit.canonicalUrl }
    ],
    link: [
        { rel: 'icon', href: `${config.public.API_BASE_URL}/favicon.ico` }
    ]
});

onMounted(() => {
    useAccountStore().checkLogin();
});

const pageComponent = useCmsPage().resolveComponent(data.value.contentType);
</script>

<template>
    <component :is="pageComponent" :content="data"></component>
</template>