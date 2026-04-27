<script setup lang="ts">
import type { ApiBlockGridModel } from '~/api/umbraco';
import OneColumnLayout from './OneColumnLayout.vue';
import Header from './Header.vue';

const props = defineProps<{
    content: ApiBlockGridModel
}>();

const componentMap: {[key: string]: Component} = {
    'header': Header,
    'oneColumnLayout': OneColumnLayout
}
</script>

<template>
    <template v-for="item in props.content.items">
        <component :is="componentMap[item.content!.contentType]" :content="item.content?.properties" :areas="item.areas"></component>
    </template>
</template>