<script setup lang="ts">
defineProps<{
    percentFilled: number;
    description?: string;
    dark?: boolean;
}>();
</script>

<template>
    <div class="flex flex-col gap-1.5">
        <div :class="[description ? 'justify-between' : 'justify-end', dark ? 'text-white/80' : 'text-gray-600']" class="flex text-sm">
            <p v-if="description">{{ description }}</p>
            <p>{{ percentFilled.toFixed(2) }}%</p>
        </div>
        <div class="h-2 w-full overflow-hidden rounded-full" :class="dark ? 'bg-white/20' : 'bg-gray-200'">
            <div
                class="h-full rounded-full"
                :class="dark ? 'bg-white/80' : 'bg-green-600'"
                :style="{ width: `${Math.min(percentFilled, 100)}%` }"
            ></div>
        </div>
    </div>
</template>
