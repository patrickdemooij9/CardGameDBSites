<script setup lang="ts">
import CardService from "~/services/CardService";
import Overview from "./Overview.vue";
import type { OverviewFilterModel } from "./OverviewFilterModel";
import type { PagedResultCardDetailApiModel } from "~/api/default";
import type OverviewRefreshModel from "./OverviewRefreshModel";

const route = useRoute();
const cardService = new CardService();

defineProps<{
  filters: OverviewFilterModel[]
}>();

const pageNumber = ref(1);
const pageNumberString = route.query["page"];
if (pageNumberString) {
  pageNumber.value = Number.parseInt(pageNumberString as string);
}

const pagedCards = ref<PagedResultCardDetailApiModel>();

const hasPrices = false;
const showCollection = false;

await loadData({
  Query: "",
  SelectedFilters: {},
});

async function loadData(value: OverviewRefreshModel) {
  pagedCards.value = await cardService.query({
    query: value.Query,
    pageNumber: pageNumber.value,
    pageSize: 30,
    customFields: value.SelectedFilters,
    variantTypeId: 0
  });
  if (value.LoadedCallback){
    value.LoadedCallback();
  }
}

async function loadLazyFilter(filter: OverviewFilterModel) {
  const values = await cardService.getValues(filter.Alias);
  filter.Items = values.map((item) => {
    return {
      DisplayName: item,
      Value: item
    };
  });
}
</script>

<template>
  <Overview
    :page="pageNumber"
    :hide-search="false"
    :hide-filters="false"
    :white-background="true"
    :filters="filters"
    @reload="loadData"
    @loadLazyFilter="loadLazyFilter"
  >
    <div v-if="pagedCards">
      <div
        :class="{ 'gap-y-6': hasPrices || showCollection }"
        class="container px-4 md:px-8 grid grid-cols-2 gap-4 sm:grid-cols-4 md:grid-cols-6"
      >
        <div class="relative" v-for="card in pagedCards.items">
          <NuxtLink :href="card.urlSegment" class="no-underline">
            <div class="missing-card-image" v-if="!card.imageUrl">
              <h2>{{ card.displayName }}</h2>
              <p>No image yet</p>
            </div>
            <img v-else :src="card.imageUrl" />
          </NuxtLink>
          <div class="flex justify-between align-center mt-2" v-if="false">
            <p>
              @card.SetCode.ToUpper() @(card.GetAbilityByType("SWU Id")?.Value)
            </p>
            <a
              v-if="false"
              href="@card.Price.Url"
              target="_blank"
              class="block bg-green-600 px-2.5 py-1 rounded-md text-white no-underline"
            >
              <p>$ @card.Price.Price</p>
            </a>
          </div>
          <!--@if (Model.ShowCollection && card.Collection != null)
    {
        var cardVariants = Model.VariantTypes.Where(it => it.HasPage && card.Collection.HasVariant(it.Id)).ToArray();
        <hr class="mt-2" />
        <div class="flex mt-2 gap-2 align-center justify-between">
            @if (cardVariants.Length > 0)
            {
                var baseVariant = card.GetMainVariant().Id;
                var ownsNormalCard = card.Collection.GetAmount(baseVariant) > 0;
                <div class="relative h-6 w-8">
                    <span class="absolute top-0 flex align-center justify-center border border-white rounded h-6 w-4 @(ownsNormalCard ? "bg-red-600" : "bg-[#cfcfcf]")" style="z-index: @(Model.VariantTypes.Length + 1)">
                        <span class="bg-white rounded-full w-2 h-2"></span>
                    </span>
                    @for (var i = 0; i < cardVariants.Length; i++)
                    {
                        var variant = cardVariants[i];
                        if (!card.Collection.HasVariant(variant.Id)) continue;

                        var bgColor = card.Collection.GetAmount(variant.Id) > 0 ? variant.Color : "#cfcfcf";
                        <span class="absolute top-0 flex align-center justify-center border border-white rounded h-6 w-4" style="background-color:@bgColor; left: @(10 + i * 10)px; z-index: @(Model.VariantTypes.Length - i)">
                            @if (!string.IsNullOrWhiteSpace(variant.Initial))
                            {
                                <span class="text-white text-xs font-bold text-center">@variant.Initial</span>
                            }
                            else
                            {
                                <span class="bg-white rounded-full w-2 h-2"></span>
                            }
                        </span>
                    }
                </div>
            }
            <p><span id="total-count-@card.BaseId">@card.Collection.GetTotalAmount()</span> <span class="md:inline hidden">copies</span></p>
            <button class="btn btn-outline flex justify-center" hx-get="/umbraco/api/collection/rendermanagemodal?cardId=@(card.BaseId)&setId=@(card.SetId)" hx-target="#collection-modal">
                <i class="ph ph-books"></i>
            </button>
        </div>
    }-->
        </div>
      </div>
      <!--<div class="flex-table grid grid-rows-2" x-show="listStyle">
<div class="flex-row table-header">
<div class="flex-cell">
    Name
</div>
@foreach (var value in Model.AbilitiesToShow)
{
    if (value.Key == "Collection" && Model.ShowCollection)
    {
        <div class="flex-cell">
            Normal
        </div>
        foreach (var variant in Model.VariantTypes)
        {
            <div class="flex-cell">
                @variant.DisplayName
            </div>
        }
    }
    else
    {
        <div class="flex-cell">
            @value.Value
        </div>
    }
}
@if (Model.ShowCollection)
{
    <div class="flex-cell">
        Collection
    </div>
}
</div>
@foreach (var card in Model.Cards.Items ?? Enumerable.Empty<CardItemViewModel>())
{
<div id="row-@card.BaseId">
    @await Html.PartialAsync("~/Views/Partials/components/cardOverviewRow.cshtml", new CardOverviewRowDataModel(card)
{
AbilitiesToShow = Model.AbilitiesToShow,
VariantTypes = Model.VariantTypes,
ShowCollection = Model.ShowCollection,
})
</div>
}
</div>-->

      <div
        class="mt-8 row justify-center"
        v-if="(pagedCards.totalPages ?? 0) > 1"
      >
        <div
          class="flex items-center mt-3 border border-gray-400 rounded bg-white overflow-hidden"
        >
          <a
            v-if="pageNumber > 1"
            :href="'?page=' + (pageNumber - 1)"
            class="pointer px-4 py-2 hover:bg-gray-400 no-underline"
            @click.prevent="pageNumber = pageNumber - 1"
            >Previous</a
          >

          <template v-for="i in pageNumber + 4">
            <a
              v-if="i - 2 <= (pagedCards.totalPages ?? 0) && i - 2 > 0"
              :href="'?page=' + (i - 2)"
              :class="[
                i - 2 === pageNumber
                  ? 'bg-main-color text-white'
                  : 'hover:bg-gray-100',
              ]"
              class="pointer px-4 py-2 border-l border-gray-400 no-underline"
              @click.prevent="pageNumber = i - 2"
              >{{ i - 2 }}</a
            >
          </template>

          <a
            v-if="pageNumber < (pagedCards.totalPages ?? 0)"
            :href="'?page=' + (pageNumber + 1)"
            class="pointer px-4 py-2 border-l border-gray-400 hover:bg-gray-100 no-underline"
            @click.prevent="pageNumber = pageNumber + 1"
            >Next</a
          >
        </div>
      </div>
    </div>
  </Overview>
</template>
