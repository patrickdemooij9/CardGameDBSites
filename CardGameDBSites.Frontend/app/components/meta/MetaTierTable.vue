<script setup lang="ts">
import type { MetaTierLeaderApiModel, MetaTierName } from "~/services/MetaService";
import CmsImage from "~/components/shared/CmsImage.vue";
import MetaDeltaBadge from "./MetaDeltaBadge.vue";
import { formatNumber } from "./format";
import { PhArrowRight } from "@phosphor-icons/vue";

const props = defineProps<{
  tier: MetaTierName;
  leaders: MetaTierLeaderApiModel[];
  deltasAvailable: boolean;
  deltasUnavailableReason?: string | null;
}>();

const TIER_META: Record<MetaTierName, { label: string; blurb: string; accent: string, startsOpen: boolean }> = {
  S: {
    label: "S Tier",
    blurb: "Decks that define the format. They have strong results across a large sample.",
    accent: "bg-yellow-400 text-gray-900",
    startsOpen: true
  },
  A: {
    label: "A Tier",
    blurb: "Consistently competitive and a safe tournament choice.",
    accent: "bg-emerald-500 text-white",
    startsOpen: true
  },
  B: {
    label: "B Tier",
    blurb: "Viable, but needs a favourable field or a strong pilot.",
    accent: "bg-sky-500 text-white",
    startsOpen: true
  },
  C: {
    label: "C Tier",
    blurb: "Underperforming against the rest of the field.",
    accent: "bg-orange-500 text-white",
    startsOpen: false
  },
  D: {
    label: "D Tier",
    blurb: "Struggling decks which are well below an even win rate.",
    accent: "bg-red-500 text-white",
    startsOpen: false
  },
  Unranked: {
    label: "Not enough data",
    blurb: "Played, but not on enough decks or at enough events to rank honestly.",
    accent: "bg-gray-300 text-gray-800",
    startsOpen: false
  },
};

const meta = computed(() => TIER_META[props.tier]);

function formatPercent(value: number): string {
  if (value <= 0) return "0%";
  if (value < 1) return "<1%";
  return `${Math.round(value)}%`;
}

function formatShare(value: number): string {
  if (value <= 0) return "0%";
  if (value < 1) return "<1%";
  return `${value.toFixed(1)}%`;
}

function sampleTitle(leader: MetaTierLeaderApiModel): string {
  const drawNote = leader.draws > 0 ? `, ${leader.draws} draws excluded` : "";
  return `${leader.wins} game wins, ${leader.losses} game losses${drawNote}, across ${leader.deckCount} decks at ${leader.eventCount} events`;
}
</script>

<template>
  <details v-if="leaders.length > 0" :open="meta.startsOpen" class="group/tier mb-4">
    <summary
      class="cursor-pointer list-none marker:content-none flex items-center gap-3 flex-wrap py-2"
    >
      <span
        class="inline-block text-sm font-bold px-3 py-1 rounded-full uppercase tracking-wider shrink-0"
        :class="meta.accent"
      >
        {{ meta.label }}
      </span>
      <span class="text-sm text-gray-600 flex-1 min-w-0">{{ meta.blurb }}</span>
      <span class="text-sm text-gray-500 tabular-nums shrink-0">{{ leaders.length }}</span>
      <span
        class="shrink-0 text-gray-400 leading-none text-xl transition-transform group-open/tier:rotate-45"
        aria-hidden="true"
        >+</span
      >
    </summary>

    <div class="md:overflow-x-auto">
      <table
        class="block md:table w-full md:border-collapse md:bg-white md:rounded-lg md:shadow-sm"
      >
        <caption class="sr-only">
          {{ meta.label }} — {{ meta.blurb }}
        </caption>
        <thead class="hidden md:table-header-group">
          <tr class="border-b border-gray-200 text-left text-xs uppercase tracking-wide text-gray-500">
            <th scope="col" class="px-4 py-3 font-semibold">Leader</th>
            <th scope="col" class="px-4 py-3 font-semibold text-right">
              <abbr title="Share of games won. These are game records, not match records.">Win rate</abbr>
            </th>
            <th scope="col" class="px-4 py-3 font-semibold text-right">Meta share</th>
            <th scope="col" class="px-4 py-3 font-semibold text-right">
              <span :title="deltasUnavailableReason ?? undefined">Week</span>
            </th>
            <th scope="col" class="px-4 py-3 font-semibold text-right">Decks</th>
            <th scope="col" class="px-4 py-3 font-semibold text-right">Events</th>
            <th scope="col" class="px-4 py-3 font-semibold w-10"><span class="sr-only">Details</span></th>
          </tr>
        </thead>
        <tbody class="block md:table-row-group">
          <tr
            v-for="leader in leaders"
            :key="leader.cardId"
            class="block md:table-row bg-white rounded-lg shadow-sm mb-3 p-3 md:p-0 md:mb-0 md:rounded-none md:shadow-none border border-gray-200 md:border-0 md:border-b md:border-gray-100 md:last:border-0 md:hover:bg-gray-50"
          >
            <th scope="row" class="block md:table-cell md:px-4 md:py-3 font-normal text-left">
              <div class="flex items-center gap-3">
                <CmsImage
                  v-if="leader.imageUrl"
                  :src="leader.imageUrl"
                  crop="icon"
                  alt=""
                  loading="lazy"
                  class="w-10 h-10 rounded object-cover shrink-0"
                />
                <div class="min-w-0 flex-1">
                  <NuxtLink
                    v-if="leader.metaUrl"
                    :href="leader.metaUrl"
                    class="font-semibold text-gray-900 hover:underline"
                  >
                    {{ leader.name }}
                  </NuxtLink>
                  <span v-else class="font-semibold text-gray-900">{{ leader.name }}</span>
                  <div v-if="leader.firstPlaceCount > 0" class="text-xs text-gray-500">
                    {{ leader.firstPlaceCount }} event win{{ leader.firstPlaceCount === 1 ? "" : "s" }}
                  </div>
                </div>
                <!-- Mobile only: the row's arrow lives beside the name, where it is reachable. -->
                <NuxtLink
                  v-if="leader.metaUrl"
                  :href="leader.metaUrl"
                  class="md:hidden shrink-0 text-gray-400 hover:text-gray-900 text-xl leading-none px-1"
                >
                  <span aria-hidden="true">→</span>
                  <span class="sr-only">View {{ leader.name }} decks and stats</span>
                </NuxtLink>
              </div>
            </th>

            <td
              class="flex justify-between items-baseline gap-4 pt-2 md:table-cell md:pt-0 md:px-4 md:py-3 md:text-right tabular-nums"
              :title="sampleTitle(leader)"
            >
              <span class="md:hidden text-xs uppercase tracking-wide text-gray-500">Win rate</span>
              <span class="font-semibold">{{ formatPercent(leader.winratePercentage) }}</span>
            </td>

            <td
              class="flex justify-between items-baseline gap-4 pt-1 md:table-cell md:pt-0 md:px-4 md:py-3 md:text-right tabular-nums"
            >
              <span class="md:hidden text-xs uppercase tracking-wide text-gray-500">Meta share</span>
              <span>{{ formatShare(leader.metaSharePercentage) }}</span>
            </td>

            <td
              class="flex justify-between items-baseline gap-4 pt-1 md:table-cell md:pt-0 md:px-4 md:py-3 md:text-right"
            >
              <span class="md:hidden text-xs uppercase tracking-wide text-gray-500">This week</span>
              <MetaDeltaBadge
                :value="leader.metaShareDeltaPoints"
                :is-new="leader.isNewEntry"
                :unavailable-reason="deltasAvailable ? null : deltasUnavailableReason"
              />
            </td>

            <td
              class="flex justify-between items-baseline gap-4 pt-1 md:table-cell md:pt-0 md:px-4 md:py-3 md:text-right tabular-nums text-gray-600"
              :title="sampleTitle(leader)"
            >
              <span class="md:hidden text-xs uppercase tracking-wide text-gray-500">Decks</span>
              <span>{{ formatNumber(leader.deckCount) }}</span>
            </td>

            <td
              class="flex justify-between items-baseline gap-4 pt-1 md:table-cell md:pt-0 md:px-4 md:py-3 md:text-right tabular-nums text-gray-600"
            >
              <span class="md:hidden text-xs uppercase tracking-wide text-gray-500">Events</span>
              <span>{{ leader.eventCount }}</span>
            </td>

            <td class="hidden md:table-cell md:px-4 md:py-3 text-right">
              <NuxtLink
                v-if="leader.metaUrl"
                :href="leader.metaUrl"
                class="inline-block text-gray-400 hover:text-gray-900 text-xl leading-none"
              >
                <PhArrowRight class="aria-hidden"/>
                <span class="sr-only">View {{ leader.name }} decks and stats</span>
              </NuxtLink>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </details>
</template>
