<script setup lang="ts">
import { PhHeart } from "@phosphor-icons/vue";
import type { DeckApiModel } from "~/api/default";

defineProps<{
  deck: DeckApiModel;
}>();

//TODO: Add logic for whenever you are logged in
</script>

<template>
  <div class="flex items-baseline gap-0.5">
    <span class="shrink-0">
      {{ deck.amountOfLikes }}
    </span>
    <template v-if="(deck.amountOfLikes ?? 0) > 0">
        <PhHeart weight="fill" class="text-red-500" />
    </template>
    <template v-else>
        <PhHeart />
    </template>
  </div>
</template>
