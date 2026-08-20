<script setup lang="ts">
import type { IApiContentModel } from "~/api/umbraco";
import type {
  CardDetailApiModel,
  PagedResultCardDetailApiModel,
} from "~/api/default";
import SetService from "~/services/SetService";
import CardOverview from "../overviews/CardOverview.vue";
import {
  OverviewFilterType,
  type OverviewFilterModel,
} from "../overviews/OverviewFilterModel";
import { useSite } from "~/composables/useSite";
import SetPriceHistoryChart from "../cards/SetPriceHistoryChart.vue";
import FaqSection from "~/components/shared/FaqSection.vue";
import { buildFaqSchema } from "~/components/shared/faq";
import {
  PhCalendar,
  PhCards,
  PhCaretDown,
  PhHash,
  PhMoney,
} from "@phosphor-icons/vue";

const props = defineProps<{
  content: IApiContentModel;
}>();

const showPriceGraph = ref(false);

const setService = new SetService();
const set = await setService.get(props.content.id!);
const priceHistory = await setService.getPriceHistory(set.id);
const currentPrice =
  priceHistory && priceHistory.length > 0
    ? priceHistory[priceHistory.length - 1]!.price
    : null;
const cardAmount = await setService.getCardAmount(set.id);
const releaseDate = computed(() => {
  const parsedReleaseDate = set.releaseDate
    ? new Date(set.releaseDate)
    : undefined;
  if (!parsedReleaseDate || parsedReleaseDate.getUTCMilliseconds() == 0) {
    return undefined;
  }
  return parsedReleaseDate;
});

const settings = await useSite().getSetOverviewSettings();
const filters: OverviewFilterModel[] =
  settings.filters?.map<OverviewFilterModel>((filter) => {
    return {
      Alias: filter.alias,
      DisplayName: filter.displayName,
      Type: filter.isInline
        ? OverviewFilterType.INLINE
        : OverviewFilterType.DROPDOWN,
      AutoFillValues: filter.autoFillValues ?? false,
      Items:
        filter.options?.map((item) => {
          return {
            DisplayName: item.displayName,
            Value: item.value,
            IconUrl: item.iconUrl ?? "",
          };
        }) ?? [],
    };
  }) ?? [];
const sortings =
  settings.sortOptions?.map((sort) => {
    return {
      Name: sort.name,
      Value: sort.value,
    };
  }) ?? [];
const baseVariantTypeIds =
  (set.mainVariants?.length ?? 0) > 0 ? set.mainVariants! : undefined;

const requestUrl = useRequestURL();
const pageUrl = `${requestUrl.origin}${requestUrl.pathname}`;

const SCHEMA_CARD_LIMIT = 100;

const overviewCards = ref<CardDetailApiModel[]>([]);
function onCardsLoaded(cards: PagedResultCardDetailApiModel) {
  overviewCards.value = cards.items ?? [];
}
function toAbsoluteUrl(url: string | null | undefined) {
  return url ? new URL(url, requestUrl.origin).toString() : undefined;
}

function toPlainText(value: string | null | undefined) {
  const text = value
    ?.replace(/<[^>]*>/g, " ")
    .replace(/\s+/g, " ")
    .trim();
  return text ? text : undefined;
}

const faqEntries = computed(() =>
  (set.frequentlyAskedQuestions ?? []).map((faq) => ({
    question: faq.heading ?? "",
    answer: faq.content ?? "",
  }))
);

const structuredData = computed(() => {
  const description =
    toPlainText(set.subHeading) ?? toPlainText(set.description);
  const faqs = faqEntries.value;

  const graph: Record<string, unknown>[] = [
    {
      "@type": "CollectionPage",
      "@id": `${pageUrl}#webpage`,
      url: pageUrl,
      name: set.displayName,
      ...(description ? { description } : {}),
      ...(set.imageUrl ? { image: toAbsoluteUrl(set.imageUrl) } : {}),
      mainEntity: { "@id": `${pageUrl}#cards` },
      ...(faqs.length > 0 ? { hasPart: { "@id": `${pageUrl}#faq` } } : {}),
    },
    {
      "@type": "ItemList",
      "@id": `${pageUrl}#cards`,
      name: `Cards in ${set.displayName}`,
      numberOfItems: cardAmount,
      itemListOrder: "https://schema.org/ItemListOrderAscending",
      itemListElement: overviewCards.value
        .slice(0, SCHEMA_CARD_LIMIT)
        .map((card, index) => ({
          "@type": "ListItem",
          position: index + 1,
          name: card.displayName,
          ...(card.urlSegment ? { url: toAbsoluteUrl(card.urlSegment) } : {}),
          ...(card.imageUrl?.url
            ? { image: toAbsoluteUrl(card.imageUrl.url) }
            : {}),
        })),
    },
  ];

  const faqNode = buildFaqSchema(faqs, pageUrl);
  if (faqNode) graph.push(faqNode);

  return {
    "@context": "https://schema.org",
    "@graph": graph,
  };
});

useHead(() => ({
  script: [
    {
      type: "application/ld+json",
      innerHTML: JSON.stringify(structuredData.value),
    },
  ],
}));

onMounted(async () => {
  const accountStore = useAccountStore();
  await accountStore.checkLogin();
  if (accountStore.isLoggedIn) {
    filters.push({
      Alias: "collection",
      DisplayName: "Collection",
      Type: OverviewFilterType.DROPDOWN,
      Items: [
        {
          DisplayName: "In collection",
          Value: "inCollection",
        },
        {
          DisplayName: "No copies",
          Value: "none",
        },
        {
          DisplayName: "Missing copies",
          Value: "missing",
        },
      ],
      AutoFillValues: false,
    });
  }
});
</script>

<template>
  <div class="mt-8">
    <div class="container px-4 md:px-8 mb-12">
      <div class="flex flex-col gap-2">
        <h1 class="mb-2">{{ set.displayName }}</h1>
        <div v-if="set.subHeading" v-html="set.subHeading"></div>
        <div class="flex gap-2">
          <div
            v-if="set.code"
            class="flex gap-1 items-center px-2 py-1 border bg-white rounded"
          >
            <PhHash />
            {{ set.code }}
          </div>
          <div
            v-if="cardAmount > 0"
            class="flex gap-1 items-center px-2 py-1 border bg-white rounded"
          >
            <PhCards />
            {{ cardAmount }} cards
          </div>
          <div
            v-if="releaseDate"
            class="flex gap-1 items-center px-2 py-1 border bg-white rounded"
          >
            <PhCalendar />
            {{
              releaseDate.toLocaleDateString(undefined, {
                year: "numeric",
                month: "long",
                day: "numeric",
              })
            }}
          </div>
          <div
            class="flex gap-1 items-center px-2 py-1 border bg-white rounded cursor-pointer"
            @click="showPriceGraph = !showPriceGraph"
          >
            <PhMoney />
            Set value: ${{ currentPrice?.toFixed(2) }}
            <PhCaretDown v-if="priceHistory && priceHistory.length > 1" />
          </div>
        </div>
        <SetPriceHistoryChart
          class="mt-2"
          v-if="showPriceGraph && priceHistory && priceHistory.length > 1"
          :set-id="set.id"
        />
      </div>
    </div>
    <CardOverview
      :filters="filters"
      :sortings="sortings"
      :set-id="set.id"
      :page-size="1000"
      :variant-type-ids="baseVariantTypeIds"
      @reloaded="onCardsLoaded"
    ></CardOverview>
    <div
      class="container px-4 md:px-8 mb-12 mt-4"
      v-if="set.description || (set.frequentlyAskedQuestions?.length ?? 0) > 0"
    >
      <div v-if="set.description" v-html="set.description"></div>
      <FaqSection v-if="faqEntries.length > 0" :entries="faqEntries" class="mt-8" />
    </div>
  </div>
</template>
