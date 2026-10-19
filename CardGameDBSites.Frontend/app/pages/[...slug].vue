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
const seo = data.value.seoToolkit;
const canonicalUrl = `${useRequestURL().origin}${route.path}`;
// Without max-image-preview:large, Discover only shows small thumbnails.
const robots = [...(seo.robots ?? []), 'max-image-preview:large', 'max-snippet:-1', 'max-video-preview:-1'].join(', ');

useHead({
    title: seo.title,
    meta: [
        { name: 'description', content: seo.metaDescription },
        { name: 'robots', content: robots },
        { property: 'og:title', content: seo.openGraphTitle || seo.title },
        { property: 'og:description', content: seo.openGraphDescription || seo.metaDescription },
        ...(seo.openGraphImage ? [{ property: 'og:image', content: seo.openGraphImage }] : []),
        { property: 'og:url', content: seo.openGraphUrl || canonicalUrl }
    ],
    link: [
        { rel: 'icon', href: `${config.public.API_BASE_URL}/favicon.ico` },
        { rel: 'canonical', href: canonicalUrl }
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