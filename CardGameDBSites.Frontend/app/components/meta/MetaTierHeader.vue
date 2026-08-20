<script setup lang="ts">
import type { MetaTierListApiModel } from "~/services/MetaService";
import { formatNumber, formatDay } from "./format";

const props = defineProps<{
  tierList: MetaTierListApiModel;
  heading: string;
}>();

const dateRange = computed(() => {
  const from = formatDay(props.tierList.firstEventUtc);
  const to = formatDay(props.tierList.lastEventUtc);
  if (!from || !to) return "";
  return from === to ? from : `${from} - ${to}`;
});
</script>

<template>
  <section class="bg-gray-900 text-white py-12 px-4">
    <div class="container md:px-8">
      <h1 class="text-white text-3xl md:text-5xl font-bold mb-3">{{ heading }}</h1>

      <p v-if="tierList.lastUpdatedUtc" class="text-gray-300 mb-6">
        Updated
        <time :datetime="tierList.lastUpdatedUtc" class="font-semibold text-white">
          {{ formatDay(tierList.lastUpdatedUtc) }}
        </time>
        <span class="text-gray-400"> — reflects the most recent event included, not today's date.</span>
      </p>

      <dl class="grid grid-cols-2 sm:grid-cols-4 gap-4 max-w-3xl">
        <div class="bg-white/10 rounded-lg px-4 py-3">
          <dt class="text-xs uppercase tracking-wide text-gray-400 font-semibold">Decks analysed</dt>
          <dd class="text-2xl font-bold tabular-nums">{{ formatNumber(tierList.totalDecks) }}</dd>
        </div>
        <div class="bg-white/10 rounded-lg px-4 py-3">
          <dt class="text-xs uppercase tracking-wide text-gray-400 font-semibold">Events</dt>
          <dd class="text-2xl font-bold tabular-nums">{{ formatNumber(tierList.totalEvents) }}</dd>
        </div>
        <div class="bg-white/10 rounded-lg px-4 py-3">
          <dt class="text-xs uppercase tracking-wide text-gray-400 font-semibold">Leaders ranked</dt>
          <dd class="text-2xl font-bold tabular-nums">
            {{ tierList.leaders.filter((l) => l.tier !== "Unranked").length }}
          </dd>
        </div>
        <div class="bg-white/10 rounded-lg px-4 py-3">
          <dt class="text-xs uppercase tracking-wide text-gray-400 font-semibold">Date range</dt>
          <dd class="text-sm font-semibold pt-1.5">{{ dateRange || "-" }}</dd>
        </div>
      </dl>

      <p
        v-if="!tierList.deltasAvailable && tierList.deltasUnavailableReason"
        class="mt-6 bg-yellow-400/15 border border-yellow-400/40 rounded-lg px-4 py-3 text-sm text-yellow-100 max-w-3xl"
      >
        {{ tierList.deltasUnavailableReason }}
        <template v-if="tierList.totalDecks > 0">
          Rankings so far are based on {{ formatNumber(tierList.totalDecks) }} decks from
          {{ tierList.totalEvents }} event{{ tierList.totalEvents === 1 ? "" : "s" }} and will move quickly.
        </template>
      </p>
    </div>
  </section>
</template>
