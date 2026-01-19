<script setup lang="ts">
import CardService from "~/services/CardService";
import type { OverviewFilterModel } from "./OverviewFilterModel";
import {
  CardSearchFilterClauseType,
  type CardDetailApiModel,
  type CardsQueryFilterClauseApiModel,
  type PagedResultCardDetailApiModel,
} from "~/api/default";
import type OverviewRefreshModel from "./OverviewRefreshModel";
import BaseCardOverview from "./BaseCardOverview.vue";
import { GetCrop } from "~/helpers/CropUrlHelper";
import { PhBooks } from "@phosphor-icons/vue";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";
import CollectionCardVariantPopup from "../popups/CollectionCardVariantPopup.vue";

const route = useRoute();
const accountService = useAccountStore();
const collectionService = useCollectionStore();
const cardService = new CardService();

const props = defineProps<{
  filters: OverviewFilterModel[];
  setId?: number;
}>();

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
let showCollection = false;
const collectionSelectedCard = ref<CardDetailApiModel | null>(null);

onMounted(async () => {
  const isLoggedIn = await accountService.checkLogin();
  showCollection = isLoggedIn; //Eventually also check for collection settings
});
</script>

<template>
  <BaseCardOverview
    :filters="filters"
    :internal-filters="internalFilters"
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
        <div class="flex justify-between align-center mt-2">
          <p v-if="false">
            @card.SetCode.ToUpper() @(card.GetAbilityByType("SWU Id")?.Value)
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
        <div v-if="showCollection">
          <hr class="mt-2" />
          <div class="flex mt-2 gap-2 items-center justify-between">
            <p>
              <span>{{
                collectionService.amount(card.baseId!)
              }}</span>
              <span class="md:inline hidden ml-2">copies</span>
            </p>
            <Button :button-type="ButtonType.Outline" class="flex justify-center" @click="collectionSelectedCard = card">
              <PhBooks />
            </Button>
          </div>
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
  <CollectionCardVariantPopup
    v-if="collectionSelectedCard"
    :card="collectionSelectedCard"
    @close="collectionSelectedCard = null">
    </CollectionCardVariantPopup>
</template>
