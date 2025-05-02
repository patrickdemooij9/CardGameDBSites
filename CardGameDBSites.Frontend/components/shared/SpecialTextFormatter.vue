<script setup lang="ts">
const props = defineProps<{
    content: string
}>();

const formattedText = computed(() => {
    return props.content.replace(/\|(.*?)\|/g, '<b>$1</b>')
});
</script>

<template>
    <span v-html="formattedText"></span>
</template>