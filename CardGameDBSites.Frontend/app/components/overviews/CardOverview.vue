<script setup lang="ts">
import type { OverviewFilterModel } from "./OverviewFilterModel";
import type { OverviewSortModel } from "./OverviewSortModel";
import {
  CardSearchCollectionMode,
  CardSearchFilterClauseType,
  type CardDetailApiModel,
  type CardsQueryFilterClauseApiModel,
  type CardVariantTypeApiModel,
  type PagedResultCardDetailApiModel,
  type SetViewModel,
} from "~/api/default";
import BaseCardOverview from "./BaseCardOverview.vue";
import CollectionCardVariantPopup from "../popups/CollectionCardVariantPopup.vue";
import { useCards } from "~/composables/useCards";
import { useCollection } from "~/composables/useCollection";
import { GetCardValue } from "~/helpers/CardHelper";
import SetService from "~/services/SetService";
import { useAccountStore } from "~/stores/AccountStore";
import { useCollectionStore } from "~/stores/CollectionStore";
import CardOverviewImageView from "./views/CardOverviewImageView.vue";
import CardOverviewRowsView from "./views/CardOverviewRowsView.vue";
import CardOverviewCollectionView from "./views/CardOverviewCollectionView.vue";

const route = useRoute();
const accountService = useAccountStore();
const collectionService = useCollection();
const cards = useCards();
const siteSettings = await useSite().getSettings();
const sets = ref<SetViewModel[]>([]);

export type CardOverviewTableColumn = {
  alias: string;
  displayName: string;
};

type CollectionColumn = {
  variantTypeId: number | null;
  displayName: string;
};

type CollectionCell = CollectionColumn & {
  key: string;
  amount: number | null;
  isUpdating: boolean;
};

const props = defineProps<{
  filters: OverviewFilterModel[];
  sortings?: OverviewSortModel[];
  setId?: number;
  tableColumns?: CardOverviewTableColumn[];
  pageSize?: number;
  collectionMode?: CardSearchCollectionMode;
  variantTypeIds?: number[];
}>();

const availableViews = computed(() => {
  const views = ["images"];

  if (props.tableColumns && props.tableColumns.length > 0) {
    views.push("rows");
  }

  if (props.setId && isLoggedIn.value) {
    views.push("collection");
  }

  return views;
});

const pageNumber = ref(1);
const pageNumberString = route.query["page"];
if (pageNumberString) {
  pageNumber.value = Number.parseInt(pageNumberString as string);
}

const internalFilters = computed<CardsQueryFilterClauseApiModel[]>(() => {
  if (!props.setId) {
    return [];
  }
  return [
    {
      clauseType: CardSearchFilterClauseType.AND,
      filters: [
        {
          alias: "SetId",
          values: [props.setId.toString()],
        },
      ],
    },
  ];
});

const showPrices = false;
const variantTypes = ref<CardVariantTypeApiModel[]>([]);
const mainVariants = computed(() => variantTypes.value.filter((item) => item.hasPage));
const currentCards = ref<PagedResultCardDetailApiModel | null>(null);
const collectionColumns = computed<CollectionColumn[]>(() => [
  {
    variantTypeId: null,
    displayName: "Normal",
  },
  ...variantTypes.value
    .filter((item) => {
      return currentCards.value?.items?.some((card) =>
        getCollectionSetVariants(card).some((variant) => variant.variantTypeId === item.id),
      );
    })
    .map((item) => ({
      variantTypeId: item.id,
      displayName: item.displayName,
    })),
]);
const collectionSelectedCard = ref<CardDetailApiModel | null>(null);
const isLoading = ref(true);
const isLoggedIn = ref(false);
const collectionStore = useCollectionStore();
const updatingCollectionKeys = ref(new Set<string>());
const collectionVariantBadgeBaseWidth = 16;
const collectionVariantBadgeStep = 10;

onMounted(async () => {
  isLoggedIn.value = await accountService.checkLogin();
  variantTypes.value = await cards.loadVariantTypes();
  isLoading.value = false;
});

function loadCollectionCards(cards: PagedResultCardDetailApiModel) {
  currentCards.value = cards;
  sets.value = [];

  //TODO: Rewrite this so we cache the sets instead of loading them every time we load cards, otherwise we end up with a lot of requests when loading multiple pages or changing filters
  const setIds = new Set(cards.items!.map((card) => card.setId));
  setIds.forEach((setId) => {
    new SetService().getById(setId!).then((set) => {
      if (set) {
        sets.value.push(set);
      }
    });
  });
  if (!isLoggedIn.value) {
    return;
  }
  collectionService.loadCards(cards.items!.map((c) => c.baseId!));
}

function getMainVariants(card: CardDetailApiModel){
  const cardSetVariants = card.variants?.filter(v => v.setId == card.setId) ?? [];
  return mainVariants.value.filter((item) => cardSetVariants.some((v) => v.variantTypeId == item.id));
}

function getCollectionSetVariants(card: CardDetailApiModel) {
  return (card.variants ?? [])
    .filter((variant) => variant.setId === card.setId)
    .sort((a, b) => (a.variantTypeId ?? 0) - (b.variantTypeId ?? 0));
}

function getVariantAmount(card: CardDetailApiModel, variantTypeId: number | null) {
  const variant = getCollectionSetVariants(card).find(
    (item) => (item.variantTypeId ?? null) === variantTypeId,
  );

  if (!variant?.cardVariantId) {
    return null;
  }

  const cardCollectionEntries = collectionStore.getCards(card.baseId!);
  return (
    cardCollectionEntries.find((item) => item.variantId === variant.cardVariantId)?.amount ?? 0
  );
}

function getCollectionCellKey(card: CardDetailApiModel, variantTypeId: number | null) {
  return `${card.baseId}-${variantTypeId ?? "normal"}`;
}

function isCollectionCellUpdating(card: CardDetailApiModel, variantTypeId: number | null) {
  return updatingCollectionKeys.value.has(getCollectionCellKey(card, variantTypeId));
}

function getCollectionCells(card: CardDetailApiModel): CollectionCell[] {
  return collectionColumns.value.map((collectionColumn) => ({
    ...collectionColumn,
    key: getCollectionCellKey(card, collectionColumn.variantTypeId),
    amount: getVariantAmount(card, collectionColumn.variantTypeId),
    isUpdating: isCollectionCellUpdating(card, collectionColumn.variantTypeId),
  }));
}

async function updateCollectionAmount(
  card: CardDetailApiModel,
  variantTypeId: number | null,
  delta: number,
) {
  const variant = getCollectionSetVariants(card).find(
    (item) => (item.variantTypeId ?? null) === variantTypeId,
  );

  if (!variant?.cardVariantId || !card.baseId) {
    return;
  }

  const currentValue = getVariantAmount(card, variantTypeId);
  if (currentValue === null) {
    return;
  }

  const nextValue = Math.max(0, currentValue + delta);
  if (nextValue === currentValue) {
    return;
  }

  const collectionKey = getCollectionCellKey(card, variantTypeId);
  updatingCollectionKeys.value.add(collectionKey);

  const cardCollectionEntries = collectionStore.getCards(card.baseId);
  const cardCollectionEntryMap = new Map(
    cardCollectionEntries.map((entry) => [entry.variantId, entry.amount ?? 0]),
  );
  const variantAmounts = Object.fromEntries(
    getCollectionSetVariants(card)
      .filter((item) => item.cardVariantId)
      .map((item) => [
        item.cardVariantId!,
        cardCollectionEntryMap.get(item.cardVariantId) ?? 0,
      ]),
  ) as Record<number, number>;
  variantAmounts[variant.cardVariantId] = nextValue;

  try {
    await collectionService.saveCards(card.baseId, variantAmounts);
  } finally {
    updatingCollectionKeys.value.delete(collectionKey);
  }
}

function ownsVariant(card: CardDetailApiModel, variantTypeId: number | null) {
  const variant = card.variants?.find(
    (item) => item.variantTypeId == variantTypeId && item.setId == card.setId,
  );
  
  return (
    (collectionStore
      .getCards(card.baseId!)
      .find((item) => variant?.cardVariantId == item.variantId)?.amount ?? 0) > 0
  );
}

function getCardIdentifier(card: CardDetailApiModel) {
  let template = siteSettings.cardOverviewIdentifier ?? "";
  // If the template contains {setCode}, get the set code of the card and replace it in the template
  if (template.includes("{setCode}")) {
    const set = sets.value.find((s) => s.id === card.setId);
    if (set) {
      template = template.replace("{setCode}", set.code?.toUpperCase() ?? "");
    }
  }

  // Use a regex to get everything between { and } and replace it with the corresponding card value
  return template.replace(/{(.*?)}/g, (_, key) => {
    const value = GetCardValue(card, key.trim());
    return value ? value.toString() : "";
  });
}
</script>

<template>
  <div v-if="isLoading" class="flex justify-center items-center py-12">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>
  <BaseCardOverview
    v-else
    :filters="filters"
    :sortings="sortings"
    :internal-filters="internalFilters"
    :white-background="true"
    :enable-query-string-sync="true"
    :available-views="availableViews"
    :page-size="pageSize"
    :collection-mode="collectionMode"
    :variant-type-ids="variantTypeIds"
    v-slot="{ cards, viewMode }"
    @reloaded="loadCollectionCards"
  >
    <!-- Image grid view -->
    <CardOverviewImageView
      v-if="viewMode === 'images'"
      :cards="cards.items ?? []"
      :is-logged-in="isLoggedIn"
      :show-card-identifier="Boolean(siteSettings.cardOverviewIdentifier)"
      :get-card-identifier="getCardIdentifier"
      :get-main-variants="getMainVariants"
      :owns-variant="ownsVariant"
      :get-amount-for-set="collectionService.getAmountForSet"
      :collection-variant-badge-base-width="collectionVariantBadgeBaseWidth"
      :collection-variant-badge-step="collectionVariantBadgeStep"
      @open-collection="collectionSelectedCard = $event"
    />

    <CardOverviewCollectionView
      v-else-if="viewMode === 'collection' && setId && isLoggedIn"
      :cards="cards.items ?? []"
      :columns="collectionColumns"
      :get-collection-cells="getCollectionCells"
      @update-collection-amount="updateCollectionAmount"
    />

    <!-- Table / rows view -->
    <CardOverviewRowsView
      v-else-if="viewMode === 'rows' && tableColumns && tableColumns.length > 0"
      :cards="cards.items ?? []"
      :table-columns="tableColumns"
      :is-logged-in="isLoggedIn"
      :get-amount-for-set="collectionService.getAmountForSet"
      @open-collection="collectionSelectedCard = $event"
    />
  </BaseCardOverview>
  <CollectionCardVariantPopup
    v-if="collectionSelectedCard"
    :card="collectionSelectedCard"
    @close="collectionSelectedCard = null"
  >
  </CollectionCardVariantPopup>
</template>
