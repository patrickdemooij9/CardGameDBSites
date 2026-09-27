<script setup lang="ts">
import type { VariantOption } from "~/services/scanner/ScanQueue";

const props = defineProps<{
  options: VariantOption[];
}>();

const variantId = defineModel<number>({ required: true });

const matched = computed(() => props.options.filter((option) => option.matched));
const others = computed(() => props.options.filter((option) => !option.matched));
</script>

<template>
  <select
    v-model="variantId"
    :disabled="!options.length"
    class="max-w-[45%] shrink-0 rounded-md border px-2 py-1.5 text-sm"
    aria-label="Variant"
  >
    <option v-if="!options.length" :value="variantId">Loading…</option>
    <option v-for="option in matched" :key="option.variantId" :value="option.variantId">
      {{ option.name }}
    </option>
    <optgroup v-if="others.length" label="Other printings">
      <option v-for="option in others" :key="option.variantId" :value="option.variantId">
        {{ option.name }}
      </option>
    </optgroup>
  </select>
</template>
