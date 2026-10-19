<script setup lang="ts">
import { PhArrowLeft, PhCalendarBlank, PhUser } from '@phosphor-icons/vue';
import type { BlogDetailContentModel, IApiMediaWithCropsModel } from '~/api/umbraco';
import ContentGrid from '../content/ContentGrid.vue';
import CmsImage from '../shared/CmsImage.vue';
import { GetBaseApiUrl } from '~/helpers/RequestsHelper';
import { useSite } from '~/composables/useSite';

const props = defineProps<{
    content: BlogDetailContentModel;
}>();

const properties = props.content.properties;
const image = properties?.image?.[0];

const headerImageUrl = image ? `${GetBaseApiUrl()}${image.url}?width=1920` : null;

const siteSettings = await useSite().getSettings();
const requestUrl = useRequestURL();
const pageUrl = `${requestUrl.origin}${props.content.route.path}`;

function toIsoDate(value: string) {
    const hasOffset = /(Z|[+-]\d{2}:?\d{2})$/.test(value);
    return new Date(hasOffset ? value : `${value}Z`).toISOString();
}

const datePublished = toIsoDate(properties?.publishDate || props.content.createDate);
const updated = toIsoDate(props.content.updateDate);
const dateModified = updated > datePublished ? updated : datePublished;
// Fixed locale and UTC keep the server render and hydration identical.
const publishedLabel = new Date(datePublished).toLocaleDateString('en-GB', { day: 'numeric', month: 'long', year: 'numeric', timeZone: 'UTC' });
const overviewUrl = props.content.route.path.replace(/[^/]+\/?$/, '');

// Discover needs a large (>=1200px wide) image; Google recommends 16:9, 4:3 and 1:1 variants.
function articleImageUrl(media: IApiMediaWithCropsModel, width: number, height: number) {
    const focalPoint = media.focalPoint ? `&rxy=${media.focalPoint.left},${media.focalPoint.top}` : '';
    return `${GetBaseApiUrl()}${media.url}?width=${width}&height=${height}&rmode=crop${focalPoint}`;
}

const articleImages = image
    ? [[1200, 675], [1200, 900], [1200, 1200]].map(([width, height]) => articleImageUrl(image, width!, height!))
    : [];

function toAbsoluteApiUrl(url: string) {
    return new URL(url, GetBaseApiUrl()).toString();
}

const structuredData = {
    '@context': 'https://schema.org',
    '@type': 'BlogPosting',
    '@id': `${pageUrl}#article`,
    mainEntityOfPage: pageUrl,
    url: pageUrl,
    headline: properties?.title,
    ...(properties?.description ? { description: properties.description } : {}),
    ...(articleImages.length ? { image: articleImages } : {}),
    datePublished,
    dateModified,
    ...(properties?.author ? { author: { '@type': 'Person', name: properties.author } } : {}),
    ...(siteSettings
        ? {
            publisher: {
                '@type': 'Organization',
                name: siteSettings.siteName,
                url: requestUrl.origin,
                ...(siteSettings.navigationLogoUrl ? { logo: toAbsoluteApiUrl(siteSettings.navigationLogoUrl) } : {}),
            },
        }
        : {}),
};

useHead({
    meta: [
        { property: 'og:type', content: 'article' },
        ...(articleImages.length
            ? [
                { property: 'og:image', content: articleImages[0] },
                { property: 'og:image:width', content: '1200' },
                { property: 'og:image:height', content: '675' },
                { property: 'og:image:alt', content: properties?.title ?? '' },
            ]
            : []),
        { property: 'article:published_time', content: datePublished },
        { property: 'article:modified_time', content: dateModified },
        ...(properties?.author ? [{ property: 'article:author', content: properties.author }] : []),
        { name: 'twitter:card', content: 'summary_large_image' },
    ],
    script: [{ type: 'application/ld+json', innerHTML: JSON.stringify(structuredData) }],
});
</script>

<template>
    <header
        class="relative overflow-hidden"
        :class="headerImageUrl ? 'bg-black text-white' : 'bg-gradient-to-b from-gray-200 to-transparent'"
    >
        <template v-if="headerImageUrl">
            <CmsImage class="absolute inset-0 h-full w-full object-cover" :src="headerImageUrl" :alt="properties?.title ?? ''" loading="eager" />
            <div class="absolute inset-0 bg-gradient-to-t from-black/90 via-black/60 to-black/30"></div>
        </template>
        <div class="relative mx-auto max-w-3xl px-4 pt-12 pb-24 md:pt-20 md:pb-28">
            <NuxtLink
                :to="overviewUrl"
                class="no-underline mb-6 inline-flex items-center gap-1 text-sm font-semibold uppercase tracking-wider opacity-80 hover:opacity-100"
            >
                <PhArrowLeft /> News
            </NuxtLink>
            <h1 class="mb-4 text-3xl leading-tight md:text-5xl">{{ properties?.title }}</h1>
            <p v-if="properties?.description" class="mb-6 text-lg leading-relaxed md:text-xl" :class="headerImageUrl ? 'text-gray-200' : 'text-gray-700'">
                {{ properties.description }}
            </p>
            <div class="flex flex-wrap items-center gap-x-5 gap-y-2 text-sm" :class="headerImageUrl ? 'text-gray-300' : 'text-gray-600'">
                <span v-if="properties?.author" class="inline-flex items-center gap-1.5"><PhUser /> {{ properties.author }}</span>
                <time :datetime="datePublished" class="inline-flex items-center gap-1.5"><PhCalendarBlank /> {{ publishedLabel }}</time>
            </div>
        </div>
    </header>

    <article class="relative z-10 mx-auto -mt-16 mb-16 max-w-3xl px-4">
        <div class="article-body rounded-lg bg-white px-5 py-8 shadow-sm ring-1 ring-gray-200 md:px-10 md:py-10">
            <ContentGrid v-if="properties?.grid" :content="properties.grid" />
        </div>
    </article>
</template>

<style scoped>
.article-body {
    @apply text-base leading-relaxed text-gray-800 md:text-lg;
}

/* Grid layout blocks wrap themselves in a full-width .container; inside the article they should just flow. */
.article-body :deep(.container) {
    @apply mb-0 max-w-none p-0;
}

.article-body :deep(p) {
    @apply mb-5;
}

.article-body :deep(.rte-spacer) {
    @apply hidden;
}

.article-body :deep(h2),
.article-body :deep(h3),
.article-body :deep(h4) {
    @apply mt-10 mb-3 leading-snug text-gray-900;
}

.article-body :deep(h2) {
    @apply text-2xl md:text-3xl;
}

.article-body :deep(h3) {
    @apply text-xl md:text-2xl;
}

.article-body :deep(h4) {
    @apply text-lg md:text-xl;
}

.article-body :deep(ul),
.article-body :deep(ol) {
    @apply mb-5 ml-6 space-y-2;
}

.article-body :deep(ol) {
    @apply list-decimal;
}

.article-body :deep(a:not(.no-underline)) {
    @apply border-blue-600 text-blue-700;
}

.article-body :deep(img) {
    @apply my-6 h-auto max-w-full rounded-md;
}

.article-body :deep(blockquote) {
    @apply my-6 border-l-4 border-gray-300 pl-4 italic text-gray-700;
}

.article-body :deep(:is(h2, h3, h4):first-child) {
    @apply mt-0;
}
</style>
