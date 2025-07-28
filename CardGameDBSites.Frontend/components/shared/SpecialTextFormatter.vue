<script setup lang="ts">
import SiteService from '~/services/SiteService';

const props = defineProps<{
    content: string
}>();

const siteSettings = await new SiteService().getSettings();

const formattedText = computed(() => {
    let text = props.content.replace(/\|(.*?)\|/g, '<b>$1</b>').replace("\\r", "<br/><br/>");
    siteSettings.keywordImages?.forEach((keywordImage) => {
        const regex = new RegExp(`\\[${keywordImage.keyword}\\]`, 'gi');
        text = text.replace(regex, `<img src="${keywordImage.imageUrl}" alt="${keywordImage.keyword}" class="inline-block h-4 w-4" />`);
    });
    return text;
});
</script>

<template>
    <span v-html="formattedText"></span>
</template>