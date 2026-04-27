<script setup lang="ts">
defineProps<{
    percentFilled: number;
    description?: string;
}>();

</script>

<template>
    <div :class="[description ? 'justify-between': 'justify-end']" class="flex">
    <p class="text-sm" v-if="description">{{ description }}</p>
    <p class="text-sm">{{ percentFilled.toFixed(2) }}%</p>
</div>
<div class="w-full rounded-full bg-gray-300 border border-gray-500 h-4">
    <div class="bg-green-600 rounded-full h-full text-white text-center" :style="{width: percentFilled + '%'}"></div>
</div>
</template>