<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import { GetCrop } from "~/helpers/CropUrlHelper";

const emit = defineEmits<{
  (e: "select", card: CardDetailApiModel): void;
}>();

const searchQuery = ref("");
const searchResults = ref<CardDetailApiModel[]>([]);
const showDropdown = ref(false);
const isLoading = ref(false);
let debounceTimeout: ReturnType<typeof setTimeout> | null = null;

async function searchCards(query: string) {
  if (query.length < 2) {
    searchResults.value = [];
    showDropdown.value = false;
    return;
  }

  isLoading.value = true;
  try {
    const result = await useCards().queryCards({
      query,
      pageNumber: 1,
      pageSize: 5,
      filterClauses: [],
      variantTypeId: 0,
    });
    searchResults.value = result.items ?? [];
    showDropdown.value = searchResults.value.length > 0;
  } catch (e) {
    searchResults.value = [];
    showDropdown.value = false;
  } finally {
    isLoading.value = false;
  }
}

function onInput() {
  if (debounceTimeout) {
    clearTimeout(debounceTimeout);
  }
  debounceTimeout = setTimeout(() => {
    searchCards(searchQuery.value);
  }, 300);
}

function selectCard(card: CardDetailApiModel) {
  emit("select", card);
  clearSearch();
}

function clearSearch() {
  searchQuery.value = "";
  searchResults.value = [];
  showDropdown.value = false;
}

function handleBlur() {
  setTimeout(() => {
    showDropdown.value = false;
  }, 200);
}
</script>

<template>
  <div class="relative w-full">
    <div class="flex gap-2">
      <input
        type="text"
        v-model="searchQuery"
        @input="onInput"
        @blur="handleBlur"
        placeholder="Search for a card..."
        class="px-3 py-2 rounded border border-gray-300 w-full"
      />
    </div>
    
    <div
      v-if="showDropdown"
      class="absolute z-50 w-full mt-1 bg-theme-surface border border-theme theme-radius shadow-lg max-h-60 overflow-auto"
      @mousedown.prevent
    >
      <div v-if="isLoading" class="px-3 py-2 text-theme-muted">Loading...</div>
      <div
        v-else
        v-for="card in searchResults"
        :key="card.baseId"
        class="flex items-center gap-2 px-3 py-2 hover:bg-theme-surface-alt cursor-pointer"
        @click="selectCard(card)"
      >
        <img
          v-if="card.imageUrl"
          :src="GetCrop(card.imageUrl, 'icon')"
          class="w-8 h-8 object-cover rounded"
        />
        <span>{{ card.displayName }}</span>
      </div>
    </div>
  </div>
</template>