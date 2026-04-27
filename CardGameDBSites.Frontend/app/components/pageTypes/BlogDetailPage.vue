<script setup lang="ts">
import type { BlogDetailContentModel } from '~/api/umbraco';
import ContentGrid from '../content/ContentGrid.vue';
import { GetCropUrl } from '~/helpers/CropUrlHelper';

const props = defineProps<{
    content: BlogDetailContentModel;
}>();

const headerImageUrl = props.content.properties?.image?.length
    ? GetCropUrl(props.content.properties.image, 'Header')
    : null;
</script>

<template>
    <div v-if="headerImageUrl" class="container-fluid relative mb-16">
        <figure class="absolute top-0 left-0 w-full h-full overflow-hidden after:bg-black/60 after:absolute after:w-full after:h-full after:top-0 after:left-0">
            <img class="h-full object-cover md:h-min md:object-none" :src="headerImageUrl" :alt="content.properties?.title ?? ''" />
        </figure>
        <div class="container text-white z-10 relative py-8 px-4">
            <h1 class="pb-4">{{ content.properties?.title }}</h1>
            <div v-if="content.properties?.description" class="pb-4">{{ content.properties.description }}</div>
            <p v-if="content.properties?.author || content.properties?.publishDate">
                {{ content.properties?.author }}
                <template v-if="content.properties?.author && content.properties?.publishDate"> - </template>
                {{ content.properties?.publishDate ? new Date(content.properties.publishDate).toLocaleDateString() : '' }}
            </p>
        </div>
    </div>

    <div v-else class="container px-4 md:px-8 mb-8">
        <h1>{{ content.properties?.title }}</h1>
        <div v-if="content.properties?.description" class="mb-2">{{ content.properties.description }}</div>
        <p v-if="content.properties?.author || content.properties?.publishDate" class="text-gray-600">
            {{ content.properties?.author }}
            <template v-if="content.properties?.author && content.properties?.publishDate"> - </template>
            {{ content.properties?.publishDate ? new Date(content.properties.publishDate).toLocaleDateString() : '' }}
        </p>
    </div>

    <div class="container px-4 md:px-8">
        <ContentGrid v-if="content.properties?.grid" :content="content.properties.grid" />
    </div>
</template>
