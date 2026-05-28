<script setup lang="ts">
import { useCardLists } from "~/composables/useCardLists";
import { useCards } from "~/composables/useCards";
import { useAccountStore } from "~/stores/AccountStore";
import type { CardListDetailModel } from "~/models/CardListModel";
import type { CardDetailApiModel } from "~/api/default";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";
import { PhTrash, PhLink, PhEye, PhEyeSlash } from "@phosphor-icons/vue";
import { GetCrop } from "~/helpers/CropUrlHelper";
import { useAppToast } from "~/composables/useAppToast";

const route = useRoute();
const router = useRouter();
const cardLists = useCardLists();
const cardsService = useCards();
const toast = useAppToast();
const isLoading = ref(true);
const listDetail = ref<CardListDetailModel | null>(null);
const cardDetails = ref<Record<number, CardDetailApiModel>>({});
const variantTypes = ref<{ id: number; displayName: string }[]>([]);

const listId = computed(() => {
  const id = route.params.slug;
  if (Array.isArray(id)) return parseInt(id[id.length - 1] ?? "0");
  return parseInt(id as string);
});

onMounted(async () => {
  const isLoggedIn = await useAccountStore().checkLogin();
  if (!isLoggedIn) {
    router.push("/login");
    return;
  }

  try {
    listDetail.value = await cardLists.getList(listId.value);
    variantTypes.value = await cardsService.loadVariantTypes();
    await loadCardDetails();
  } catch {
    router.push("/card-lists");
    return;
  }
  isLoading.value = false;
});

async function loadCardDetails() {
  if (!listDetail.value) return;
  const cardIds = [...new Set(listDetail.value.items.map((i) => i.cardId))];
  if (cardIds.length === 0) return;

  const cards = await cardsService.loadCardsByIds(cardIds);
  cards.forEach((card: CardDetailApiModel) => {
    cardDetails.value[card.baseId!] = card;
  });
}

function getVariantName(variantId: number | null | undefined): string {
  if (!variantId) return "Normal";
  const vt = variantTypes.value.find((v) => v.id === variantId);
  return vt?.displayName ?? "Unknown";
}

function getCardForItem(cardId: number): CardDetailApiModel | undefined {
  return cardDetails.value[cardId];
}

async function removeItem(itemId: number) {
  if (!listDetail.value) return;
  await cardLists.removeItem(listDetail.value.id, itemId);
  listDetail.value.items = listDetail.value.items.filter((i) => i.id !== itemId);
}

function copyPublicLink() {
  if (!listDetail.value) return;
  const url = `${window.location.origin}/card-lists/${listDetail.value.id}/public`;
  navigator.clipboard.writeText(url);
  toast.success("Public link copied to clipboard!");
}
</script>

<template>
  <div v-if="isLoading" class="flex justify-center items-center py-12">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>
  <div v-else-if="listDetail" class="container px-4 pt-8 md:px-8 mb-6">
    <!-- Header -->
    <div class="flex justify-between items-start mb-6">
      <div>
        <div class="flex items-center gap-2">
          <h1>{{ listDetail.name }}</h1>
          <PhEye v-if="listDetail.isPublic" class="w-5 h-5 text-green-600" title="Public" />
          <PhEyeSlash v-else class="w-5 h-5 text-gray-400" title="Private" />
        </div>
        <p v-if="listDetail.description" class="text-gray-600 mt-1">{{ listDetail.description }}</p>
        <p class="text-sm text-gray-500 mt-1">{{ listDetail.items.length }} cards</p>
      </div>
      <div class="flex gap-2">
        <Button
          v-if="listDetail.isPublic"
          :button-type="ButtonType.Outline"
          @click="copyPublicLink"
        >
          <PhLink class="w-4 h-4 mr-1" /> Share
        </Button>
        <Button :button-type="ButtonType.Outline" @click="router.push('/card-lists')">
          Back to Lists
        </Button>
      </div>
    </div>

    <!-- Cards grid -->
    <div v-if="listDetail.items.length === 0" class="text-center py-8">
      <p class="text-gray-500">This list is empty. Add cards from the card overview!</p>
    </div>
    <div v-else class="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6">
      <div
        v-for="item in listDetail.items"
        :key="item.id"
        class="relative border rounded bg-white overflow-hidden"
      >
        <NuxtLink
          v-if="getCardForItem(item.cardId)"
          :href="getCardForItem(item.cardId)?.urlSegment"
          class="no-underline"
        >
          <div class="missing-card-image" v-if="!getCardForItem(item.cardId)?.imageUrl">
            <h2 class="text-sm">{{ getCardForItem(item.cardId)?.displayName }}</h2>
          </div>
          <img
            v-else
            :src="GetCrop(getCardForItem(item.cardId)!.imageUrl!, undefined)"
            loading="lazy"
            class="w-full"
          />
        </NuxtLink>
        <div class="p-2">
          <p class="text-xs font-medium truncate">
            {{ getCardForItem(item.cardId)?.displayName ?? `Card #${item.cardId}` }}
          </p>
          <div class="flex justify-between items-center mt-1">
            <span class="text-xs text-gray-500">
              {{ getVariantName(item.variantId) }} × {{ item.amount }}
            </span>
            <button
              class="text-red-500 hover:text-red-700"
              @click="removeItem(item.id)"
              title="Remove from list"
            >
              <PhTrash class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
