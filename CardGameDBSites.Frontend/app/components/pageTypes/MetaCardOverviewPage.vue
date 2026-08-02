<script setup lang="ts">
import { ref } from "vue";
import {
  CardSearchFilterClauseType,
  CardSearchFilterMode,
  type CardsQueryFilterClauseApiModel,
  type PagedResultCardDetailApiModel,
} from "~/api/default";
import type { MetaCardOverviewContentModel } from "~/api/umbraco";
import BaseCardOverview from "../overviews/BaseCardOverview.vue";
import type { OverviewFilterModel } from "../overviews/OverviewFilterModel";
import CmsImage from "~/components/shared/CmsImage.vue";
import TournamentService, {
  type PeriodApiModel,
} from "~/services/TournamentService";
import MetaService, { type MetaCardStatApiModel } from "~/services/MetaService";
import { ToRequirements } from "~/helpers/RequirementHelper";
import { GetFilters } from "~/services/requirements/RequirementService";
import type { OverviewSortModel } from "../overviews/OverviewSortModel.js";

const props = defineProps<{
  content: MetaCardOverviewContentModel;
}>();

// The meta feature currently supports a single format per site (see MetaPage.vue).
const DEFAULT_FORMAT_ID = 1;

const metaService = new MetaService();
const tournamentService = new TournamentService();

const internalFilters: CardsQueryFilterClauseApiModel[] = GetFilters(
  [],
  ToRequirements(props.content.properties?.cardRequirement),
  false
);

internalFilters.push({
  filters: [{
    alias: 'Usage',
    mode: CardSearchFilterMode.HIGHER,
    values: ['0.1']
  }],
  clauseType: CardSearchFilterClauseType.AND
})

// Usage/winrate come from the current period's snapshot, overlaid onto the rendered cards.
const { data: periods } = await useAsyncData(
  `meta-overview-periods:${DEFAULT_FORMAT_ID}`,
  () => tournamentService.getPeriods(DEFAULT_FORMAT_ID).catch(() => []),
  { default: () => [] as PeriodApiModel[] }
);
const selectedPeriodId = ref<number | undefined>(
  periods.value.find((p) => p.isCurrent)?.id ?? periods.value[0]?.id
);

const noFilters: OverviewFilterModel[] = [];
const sorting: OverviewSortModel[] = [{
  Name: 'Usage desc',
  Value: 'usageDesc'
}]
const shownCardIds = ref<number[]>([]);

// Stats for the currently visible leaders. Client-side only: the leader ids only exist after
// BaseCardOverview emits @reloaded, so the overlay is fetched reactively whenever the leaders or the
// selected period change.
const statsByCardId = ref<Record<number, MetaCardStatApiModel>>({});

watch(
  [shownCardIds, selectedPeriodId],
  async () => {
    if (selectedPeriodId.value == null || shownCardIds.value.length === 0) {
      statsByCardId.value = {};
      return;
    }
    try {
      const stats = await metaService.getCardStats(
        selectedPeriodId.value,
        shownCardIds.value
      );
      statsByCardId.value = Object.fromEntries(stats.map((s) => [s.cardId, s]));
    } catch {
      statsByCardId.value = {};
    }
  },
  { immediate: true }
);

function onReloaded(paged: PagedResultCardDetailApiModel) {
  shownCardIds.value = (paged.items ?? [])
    .map((c) => c.baseId)
    .filter((id): id is number => id != null);
}

function statFor(cardId: number | undefined) {
  return cardId != null ? statsByCardId.value[cardId] : undefined;
}

// Links to this leader's meta detail page: {thisOverviewUrl}/{last segment of the card's url}.
// Dirty but fine for now — leader url segments are single tokens.
const overviewBaseUrl = (props.content.route?.path ?? "").replace(/\/$/, "");
function metaUrlFor(card: { urlSegment?: string }) {
  const parts = (card.urlSegment ?? "").split("/").filter(Boolean);
  const segment = parts[parts.length - 1];
  return segment ? `${overviewBaseUrl}/${segment}` : card.urlSegment;
}

// Renders a true 0 as "0%" but any positive value below 1 as "<1%" (so 0.4% doesn't read as 0%).
function formatPercent(value: number | undefined): string {
  if (value == null || value <= 0) return "0%";
  if (value < 1) return "<1%";
  return `${Math.round(value)}%`;
}
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <h1>{{ content.name ?? "Leaders" }}</h1>
    <div
      v-if="periods.length > 0"
      class="flex items-center justify-end gap-3 mt-4"
    >
      <label for="period-select" class="text-sm text-gray-600 font-medium"
        >Period</label
      >
      <select
        id="period-select"
        v-model="selectedPeriodId"
        class="border border-gray-300 rounded px-3 py-2 text-sm bg-white"
      >
        <option v-for="period in periods" :key="period.id" :value="period.id">
          {{ period.name }}
        </option>
      </select>
    </div>
  </div>

  <BaseCardOverview
    :filters="noFilters"
    :internal-filters="internalFilters"
    :sortings="sorting"
    :white-background="true"
    :enable-query-string-sync="true"
    :hide-inline-filters="true"
    :available-views="['images']"
    :page-size="30"
    @reloaded="onReloaded"
    v-slot="{ cards }"
  >
    <div
      class="container px-4 md:px-8 grid grid-cols-2 gap-4 sm:grid-cols-4 md:grid-cols-6"
    >
      <div
        v-for="(card, index) in cards.items ?? []"
        :key="card.baseId"
        class="flex flex-col"
      >
        <NuxtLink :href="metaUrlFor(card)" class="no-underline">
          <CmsImage
            :src="card.imageUrl"
            :alt="card.displayName"
            :loading="index < 12 ? 'eager' : 'lazy'"
            sizes="50vw sm:25vw md:16vw"
            class="w-full object-cover"
          >
            <template #fallback>
              <div class="missing-card-image aspect-[2/3]">
                <h2>{{ card.displayName }}</h2>
                <p>No image yet</p>
              </div>
            </template>
          </CmsImage>
        </NuxtLink>
        <div class="pt-2">
          <div class="font-semibold text-sm leading-tight">
            {{ card.displayName }}
          </div>
          <div class="flex justify-between text-xs text-gray-600 mt-1">
            <span>Usage: {{ formatPercent(statFor(card.baseId)?.usagePercentage) }}</span>
            <span>Winrate: {{ formatPercent(statFor(card.baseId)?.winratePercentage) }}</span>
          </div>
        </div>
      </div>
    </div>
  </BaseCardOverview>
</template>
