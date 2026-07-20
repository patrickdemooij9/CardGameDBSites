<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import CardSearchInput from "~/components/shared/CardSearchInput.vue";
import CmsImage from "~/components/shared/CmsImage.vue";
import { useAppToast } from "~/composables/useAppToast";
import { PhTrash, PhPlus, PhMinus } from "@phosphor-icons/vue";

interface ProxyCard {
  card: CardDetailApiModel;
  amount: number;
}

const maxAmountPerCard = 10;
const maxCardsTotal = 100;
const selectedCards = ref<ProxyCard[]>([]);
const isDownloading = ref(false);
const toast = useAppToast();

function addCard(card: CardDetailApiModel) {
  const existing = selectedCards.value.find(
    (c) => c.card.baseId === card.baseId,
  );
  if (existing) {
    existing.amount = Math.min(existing.amount + 1, maxAmountPerCard);
  } else {
    if (selectedCards.value.length >= maxCardsTotal) {
      toast.error(`You can only add up to ${maxCardsTotal} cards in total.`);
      return;
    }
    selectedCards.value.push({ card, amount: 1 });
  }
}

function removeCard(baseId: number) {
  selectedCards.value = selectedCards.value.filter(
    (c) => c.card.baseId !== baseId,
  );
}

function increaseAmount(baseId: number) {
  const entry = selectedCards.value.find((c) => c.card.baseId === baseId);
  if (entry) entry.amount = Math.min(entry.amount + 1, maxAmountPerCard);
}

function decreaseAmount(baseId: number) {
  const entry = selectedCards.value.find((c) => c.card.baseId === baseId);
  if (!entry) return;
  if (entry.amount <= 1) {
    removeCard(baseId);
  } else {
    entry.amount--;
  }
}

async function downloadProxyPdf() {
  if (selectedCards.value.length === 0) {
    toast.error("Please add at least one card.");
    return;
  }

  isDownloading.value = true;
  try {
    const payload = {
      cards: selectedCards.value.map((c) => ({
        cardId: c.card.baseId,
        amount: c.amount,
      })),
    };

    const response = await fetch("/api/proxy/umbraco/api/export/ProxyExport", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      throw new Error(`Export failed: ${response.statusText}`);
    }

    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "proxy-cards.pdf";
    a.click();
    URL.revokeObjectURL(url);
  } catch (e) {
    const message =
      e instanceof Error ? e.message : "An unexpected error occurred.";
    toast.error(`Failed to generate proxy PDF: ${message}`);
  } finally {
    isDownloading.value = false;
  }
}

const totalCards = computed(() =>
  selectedCards.value.reduce((sum, c) => sum + c.amount, 0),
);
</script>

<template>
  <div class="container px-4 py-8 md:px-8">
    <h1 class="text-2xl font-bold mb-2">Proxy cards for Star Wars Unlimited</h1>
    <p class="text-gray-600 mb-6">
      Search for Star Wars Unlimited cards, set the amount you need, and
      download a printable proxy sheet. Note that these proxies are for personal
      use only and should not be used for commercial purposes.
      <br />
      <strong
        >Make sure to help your local game store by purchasing official
        cards.</strong
      >
    </p>

    <div class="bg-white rounded p-4 mb-6">
      <h2 class="text-lg font-semibold mb-3">Add Cards</h2>
      <CardSearchInput @select="addCard" />
    </div>

    <div class="bg-white rounded p-4">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-lg font-semibold">
          Selected Cards
          <span
            v-if="selectedCards.length > 0"
            class="text-gray-500 text-sm font-normal ml-2"
          >
            ({{ selectedCards.length }}
            {{ selectedCards.length === 1 ? "card" : "cards" }},
            {{ totalCards }} {{ totalCards === 1 ? "copy" : "copies" }})
          </span>
        </h2>
        <button
          v-if="selectedCards.length > 0"
          class="bg-main-color text-white px-4 py-2 rounded hover:opacity-90 disabled:opacity-50 flex items-center gap-2"
          :disabled="isDownloading"
          @click="downloadProxyPdf"
        >
          <span v-if="isDownloading">Generating…</span>
          <span v-else>Download Proxy PDF</span>
        </button>
      </div>

      <div
        v-if="selectedCards.length === 0"
        class="text-gray-400 text-center py-8"
      >
        No cards selected yet. Use the search above to add cards.
      </div>

      <div
        v-else
        class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4"
      >
        <div v-if="selectedCards.length === maxAmountPerCard">
          <p class="text-red-600 text-sm text-center">
            Maximum of {{ maxAmountPerCard }} copies per card reached.
          </p>
        </div>
        <div
          v-for="entry in selectedCards"
          :key="entry.card.baseId"
          class="flex flex-col items-center gap-2"
        >
          <div class="relative w-full">
            <CmsImage
              :src="entry.card.imageUrl"
              :alt="entry.card.displayName ?? ''"
              class="w-full rounded"
            >
              <template #fallback>
                <div
                  class="w-full aspect-[2/3] bg-gray-100 rounded flex items-center justify-center text-gray-400 text-xs text-center p-2"
                >
                  {{ entry.card.displayName }}
                </div>
              </template>
            </CmsImage>
            <button
              class="absolute top-1 right-1 bg-red-600 text-white rounded-full p-2 hover:bg-red-700"
              :aria-label="`Remove ${entry.card.displayName}`"
              @click="removeCard(entry.card.baseId!)"
            >
              <PhTrash :size="14" />
            </button>
          </div>

          <p class="text-xs text-center leading-tight">
            {{ entry.card.displayName }}
          </p>

          <div class="flex items-center gap-1">
            <button
              class="border rounded px-1.5 py-0.5 hover:bg-gray-100"
              :aria-label="`Decrease amount for ${entry.card.displayName}`"
              @click="decreaseAmount(entry.card.baseId!)"
            >
              <PhMinus :size="12" />
            </button>
            <span class="w-6 text-center text-sm font-semibold">{{
              entry.amount
            }}</span>
            <button
              class="border rounded px-1.5 py-0.5 hover:bg-gray-100 disabled:bg-gray-300"
              :aria-label="`Increase amount for ${entry.card.displayName}`"
              :disabled="entry.amount >= maxAmountPerCard"
              @click="increaseAmount(entry.card.baseId!)"
            >
              <PhPlus :size="12" />
            </button>
          </div>
        </div>
      </div>

      <div v-if="selectedCards.length > 0" class="mt-6 flex justify-end">
        <button
          class="bg-main-color text-white px-6 py-2 rounded hover:opacity-90 disabled:opacity-50"
          :disabled="isDownloading"
          @click="downloadProxyPdf"
        >
          <span v-if="isDownloading">Generating…</span>
          <span v-else>Download Proxy PDF</span>
        </button>
      </div>
    </div>
  </div>
</template>
