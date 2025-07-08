<script setup lang="ts">
import CardService from "~/services/CardService";
import type { OverviewFilterModel } from "./OverviewFilterModel";
import type { PagedResultCardDetailApiModel } from "~/api/default";
import type OverviewRefreshModel from "./OverviewRefreshModel";
import BaseCardOverview from "./BaseCardOverview.vue";
import { GetCrop } from "~/helpers/CropUrlHelper";

const route = useRoute();
const cardService = new CardService();

defineProps<{
  filters: OverviewFilterModel[];
}>();

const pageNumber = ref(1);
const pageNumberString = route.query["page"];
if (pageNumberString) {
  pageNumber.value = Number.parseInt(pageNumberString as string);
}

const hasPrices = false;
const showCollection = false;
</script>

<template>
  <BaseCardOverview
    :filters="filters"
    :white-background="true"
    v-slot="{ cards }"
  >
    <div
      class="container px-4 md:px-8 grid grid-cols-2 gap-4 sm:grid-cols-4 md:grid-cols-6"
    >
      <div class="relative" v-for="card in cards.items">
        <NuxtLink :href="card.urlSegment" class="no-underline">
          <div class="missing-card-image" v-if="!card.imageUrl">
            <h2>{{ card.displayName }}</h2>
            <p>No image yet</p>
          </div>
          <img v-else :src="GetCrop(card.imageUrl, undefined)" />
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
  </BaseCardOverview>
</template>
