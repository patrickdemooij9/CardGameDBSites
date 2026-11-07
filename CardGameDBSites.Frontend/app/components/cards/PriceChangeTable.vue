<script setup lang="ts">
import { PhTriangle } from "@phosphor-icons/vue";
import type { CardPriceChangeApiModel } from "~/models/CardPriceChangeApiModel";

const props = defineProps<{
  title: string;
  changes: CardPriceChangeApiModel[];
  rising: boolean;
}>();

const changeClass = computed(() => (props.rising ? "text-positive" : "text-negative"));

const formatPrice = (price: number) => `$${price.toFixed(2)}`;
const formatSigned = (value: number, prefix = "") => `${value >= 0 ? "+" : "−"}${prefix}${Math.abs(value).toFixed(2)}`;
</script>

<template>
  <div class="overflow-hidden rounded-lg border border-line bg-surface text-ink">
    <h3 :class="changeClass" class="flex items-center gap-2 px-4 py-3 text-sm font-semibold">
      <PhTriangle weight="fill" :size="10" :class="{ 'rotate-180': !rising }" aria-hidden="true" />
      {{ title }}
    </h3>
    <table class="w-full text-sm">
      <thead class="border-t border-line text-[11px] uppercase tracking-wider text-ink-muted">
        <tr>
          <th class="px-4 py-2 text-left font-medium">Card</th>
          <th class="px-4 py-2 text-right font-medium">Price</th>
          <th class="px-4 py-2 text-right font-medium">Change</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="card in changes" :key="`${card.cardId}-${card.variantId}`" class="border-t border-line">
          <td class="px-4 py-2.5">
            <NuxtLink :to="card.urlSegment" class="no-underline hover:text-main-color">{{ card.cardName }}</NuxtLink>
          </td>
          <td class="px-4 py-2.5 text-right font-semibold tabular-nums">{{ formatPrice(card.currentPrice) }}</td>
          <td :class="changeClass" class="whitespace-nowrap px-4 py-2.5 text-right tabular-nums">
            {{ formatSigned(card.priceChange, "$") }} ({{ formatSigned(card.priceChangePercent) }}%)
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
