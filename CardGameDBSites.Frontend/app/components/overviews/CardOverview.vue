<script setup lang="ts">
import type { OverviewFilterModel } from "./OverviewFilterModel";
import type { OverviewSortModel } from "./OverviewSortModel";
import {
  CardSearchFilterClauseType,
  type CardDetailApiModel,
  type CardsQueryFilterClauseApiModel,
  type CardVariantTypeApiModel,
  type PagedResultCardDetailApiModel,
  type SetViewModel,
} from "~/api/default";
import BaseCardOverview from "./BaseCardOverview.vue";
import { GetCrop } from "~/helpers/CropUrlHelper";
import { PhBooks, PhMinus, PhPlus } from "@phosphor-icons/vue";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";
import CollectionCardVariantPopup from "../popups/CollectionCardVariantPopup.vue";
import { useCards } from "~/composables/useCards";
import { useCollection } from "~/composables/useCollection";
import { GetCardValue } from "~/helpers/CardHelper";
import SetService from "~/services/SetService";
import { useAccountStore } from "~/stores/AccountStore";
import { useCollectionStore } from "~/stores/CollectionStore";

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
const collectionColumns = computed<CollectionColumn[]>(() => [
  {
    variantTypeId: null,
    displayName: "Normal",
  },
  ...variantTypes.value.map((item) => ({
    variantTypeId: item.id,
    displayName: item.displayName,
  })),
]);
const collectionSelectedCard = ref<CardDetailApiModel | null>(null);
const isLoading = ref(true);
const isLoggedIn = ref(false);
const collectionStore = useCollectionStore();
const updatingCollectionKeys = ref(new Set<string>());

onMounted(async () => {
  isLoggedIn.value = await accountService.checkLogin();
  variantTypes.value = await cards.loadVariantTypes();
  isLoading.value = false;
});

function loadCollectionCards(cards: PagedResultCardDetailApiModel) {
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
  const variantAmounts = Object.fromEntries(
    getCollectionSetVariants(card)
      .filter((item) => item.cardVariantId)
      .map((item) => [
        item.cardVariantId!,
        cardCollectionEntries.find((entry) => entry.variantId === item.cardVariantId)?.amount ?? 0,
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
    v-slot="{ cards, viewMode }"
    @reloaded="loadCollectionCards"
  >
    <!-- Image grid view -->
    <div
      v-if="viewMode === 'images'"
      class="container px-4 md:px-8 grid grid-cols-2 gap-4 sm:grid-cols-4 md:grid-cols-6"
    >
      <div class="relative" v-for="card in cards.items">
        <NuxtLink :href="card.urlSegment" class="no-underline">
          <div class="missing-card-image" v-if="!card.imageUrl">
            <h2>{{ card.displayName }}</h2>
            <p>No image yet</p>
          </div>
          <img v-else :src="GetCrop(card.imageUrl, undefined)" loading="lazy" />
        </NuxtLink>
        <div class="flex justify-between items-center mt-2">
          <p>
            <span v-if="siteSettings.cardOverviewIdentifier">{{ getCardIdentifier(card) }}</span>
          </p>
          <a
            v-if="card.price"
            :href="card.price.referenceUrl"
            target="_blank"
            class="block bg-green-600 px-2.5 py-1 rounded-md text-white no-underline"
          >
            <p>$ {{ card.price.marketPrice }}</p>
          </a>
        </div>
        <div v-if="isLoggedIn">
          <hr class="mt-2" />
          <div class="flex mt-2 gap-2 items-center justify-between">
            <p class="relative w-4 h-6" :style="{'width': (mainVariants.length * 8) + 'px'}">
                <span
                  :class="[
                    ownsVariant(card, null)
                      ? 'bg-red-600'
                      : 'bg-[#cfcfcf]',
                  ]"
                  class="absolute top-0 flex justify-center border border-white rounded h-6 w-4 pt-1 z-10"
                  title="Base card"
                >
                  <span class="bg-white rounded-full w-2 h-2"></span>
                </span>
                <span
                  v-for="(variant, index) in getMainVariants(card)"
                  :key="variant.id"
                  class="absolute top-0 flex justify-center border border-white rounded h-6 w-4 pt-1"
                  :title="variant.displayName"
                  :style="{
                    'background-color': ownsVariant(card, variant.id)
                      ? variant.color!
                      : '#cfcfcf',
                    left: 10 + index * 10 + 'px',
                    'z-index': mainVariants.length - index,
                  }"
                >
                  <span
                    v-if="variant.initial"
                    class="text-white text-xs font-bold text-center"
                    >{{ variant.initial }}</span
                  >
                  <span v-else class="bg-white rounded-full w-2 h-2"></span>
                </span>
              </p>
            <span class="ml-2"
              >{{ collectionService.getAmountForSet(card) }}
              <span class="md:inline hidden">copies</span></span
            >
            <Button
              :button-type="ButtonType.Outline"
              class="flex justify-center"
              @click="collectionSelectedCard = card"
            >
              <PhBooks />
            </Button>
          </div>
        </div>
      </div>
    </div>

    <div
      v-else-if="viewMode === 'collection' && setId && isLoggedIn"
      class="container px-4 md:px-8 overflow-x-auto"
    >
      <table class="w-full min-w-max text-left border-collapse">
        <thead>
          <tr class="border-b-2 border-gray-300">
            <th class="py-2 pr-4 font-semibold">Name</th>
            <th
              v-for="collectionColumn in collectionColumns"
              :key="collectionColumn.variantTypeId ?? 'normal'"
              class="py-2 pr-4 font-semibold"
            >
              {{ collectionColumn.displayName }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="card in cards.items"
            :key="card.baseId"
            class="border-b border-gray-200 hover:bg-gray-50"
          >
            <td class="py-2 pr-4">
              <NuxtLink :href="card.urlSegment" class="no-underline font-medium hover:underline">
                {{ card.displayName }}
              </NuxtLink>
            </td>
            <td
              v-for="collectionCell in getCollectionCells(card)"
              :key="collectionCell.key"
              class="py-2 pr-4"
            >
              <div
                v-if="collectionCell.amount !== null"
                class="flex items-center gap-2"
              >
                <button
                  type="button"
                  class="border rounded px-1.5 py-0.5 hover:bg-gray-100 disabled:bg-gray-100 disabled:text-gray-400"
                  :aria-label="`Decrease ${collectionCell.displayName} amount for ${card.displayName}`"
                  :disabled="
                    collectionCell.isUpdating ||
                    collectionCell.amount === 0
                  "
                  @click="updateCollectionAmount(card, collectionCell.variantTypeId, -1)"
                >
                  <PhMinus :size="12" />
                </button>
                <span
                  class="min-w-6 text-center text-sm font-semibold"
                  :aria-label="`${collectionCell.amount} ${collectionCell.displayName} copies`"
                >
                  {{ collectionCell.amount }}
                </span>
                <button
                  type="button"
                  class="border rounded px-1.5 py-0.5 hover:bg-gray-100 disabled:bg-gray-100 disabled:text-gray-400"
                  :aria-label="`Increase ${collectionCell.displayName} amount for ${card.displayName}`"
                  :disabled="collectionCell.isUpdating"
                  @click="updateCollectionAmount(card, collectionCell.variantTypeId, 1)"
                >
                  <PhPlus :size="12" />
                </button>
              </div>
              <span v-else class="text-sm text-gray-400">-</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Table / rows view -->
    <div
      v-else-if="viewMode === 'rows' && tableColumns && tableColumns.length > 0"
      class="container px-4 md:px-8 overflow-x-auto"
    >
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="border-b-2 border-gray-300">
            <th class="py-2 pr-4 font-semibold">Name</th>
            <th
              v-for="col in tableColumns"
              :key="col.alias"
              class="py-2 pr-4 font-semibold"
            >
              {{ col.displayName }}
            </th>
            <th v-if="isLoggedIn" class="py-2 pr-4 font-semibold">Collection</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="card in cards.items"
            :key="card.baseId"
            class="border-b border-gray-200 hover:bg-gray-50"
          >
            <td class="py-2 pr-4">
              <NuxtLink :href="card.urlSegment" class="no-underline font-medium hover:underline">
                {{ card.displayName }}
              </NuxtLink>
            </td>
            <td
              v-for="col in tableColumns"
              :key="col.alias"
              class="py-2 pr-4 text-sm text-gray-700"
            >
              {{ card.attributes?.[col.alias]?.join(", ") ?? "-" }}
            </td>
            <td v-if="isLoggedIn" class="py-2 pr-4">
              <div class="flex items-center gap-2">
                <span :aria-label="`${collectionService.getAmountForSet(card)} copies`">
                  {{ collectionService.getAmountForSet(card) }}
                  <span class="md:inline hidden" aria-hidden="true">copies</span>
                </span>
                <Button
                  :button-type="ButtonType.Outline"
                  class="flex justify-center"
                  @click="collectionSelectedCard = card"
                >
                  <PhBooks />
                </Button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </BaseCardOverview>
  <CollectionCardVariantPopup
    v-if="collectionSelectedCard"
    :card="collectionSelectedCard"
    @close="collectionSelectedCard = null"
  >
  </CollectionCardVariantPopup>
</template>
