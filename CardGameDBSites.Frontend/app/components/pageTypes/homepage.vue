<script setup lang="ts">
import type { HomepageContentModel } from '~/api/umbraco';
import ContentGrid from '../content/ContentGrid.vue';
import type { PageSeoMetaField } from '~/models/PageSeoModel';
import { buildWebSiteSchema, siteNameFromTitle } from '~/helpers/StructuredDataHelper';

const props = defineProps<{
    content: HomepageContentModel & { seoToolkit?: PageSeoMetaField }
}>();

const requestUrl = useRequestURL();
useJsonLd([
    buildWebSiteSchema(requestUrl.origin, siteNameFromTitle(props.content.seoToolkit?.title, requestUrl.hostname)),
]);
</script>

<template>
    <!-- Own wrapper so the layout's loading indicator doesn't shift the even/odd band colors -->
    <div>
        <ContentGrid :content="props.content.properties!.grid!" banded></ContentGrid>
    </div>
</template>