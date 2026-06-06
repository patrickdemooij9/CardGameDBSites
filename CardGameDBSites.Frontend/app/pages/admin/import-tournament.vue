<script setup lang="ts">
import { useAccountStore } from "~/stores/AccountStore";
import { DoServerFetch } from "~/helpers/RequestsHelper";
import type { ImportTournamentResult } from "~/api/default";

const accountStore = useAccountStore();

const formatId = ref<number | null>(null);
const type = ref("");
const source = ref("");
const externalId = ref("");

const isLoading = ref(false);
const successMessage = ref<string | null>(null);
const errorMessage = ref<string | null>(null);
const missingCards = ref<any[]>([]);

async function importTournament() {
  if (!formatId.value || !type.value || !source.value || !externalId.value)
    return;

  isLoading.value = true;
  successMessage.value = null;
  errorMessage.value = null;
  missingCards.value = [];

  try {
    const response = await DoServerFetch<ImportTournamentResult>("/api/management", true, {
      method: "POST",
      body: {
        formatId: formatId.value,
        type: type.value,
        source: source.value,
        externalId: externalId.value,
      },
    });

    if (response) {
      if (response.success === false) {
        errorMessage.value = response.message || "Failed to import tournament.";
        if (response.missingCards && Array.isArray(response.missingCards)) {
          missingCards.value = response.missingCards;
        }
      } else {
        successMessage.value = response.message || "Tournament imported successfully.";
      }
    } else {
      successMessage.value = "Tournament imported successfully.";
    }
  } catch (error) {
    errorMessage.value = "Failed to import tournament. Please check the details and try again.";
  } finally {
    isLoading.value = false;
  }
}

onMounted(async () => {
  await accountStore.checkLogin();

  if (!accountStore.member?.isAdmin) {
    navigateTo("/");
  }
});
</script>

<template>
  <div class="container mx-auto px-4 py-8 max-w-2xl">
    <h1 class="text-2xl font-bold mb-6">Import Tournament</h1>

    <div class="bg-white rounded shadow p-6">
      <div class="mb-4">
        <label class="block mb-2 font-semibold" for="formatId">Format ID</label>
        <input
          id="formatId"
          v-model.number="formatId"
          type="number"
          min="1"
          placeholder="e.g. 1"
          class="border border-gray-300 rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
      </div>

      <div class="mb-4">
        <label class="block mb-2 font-semibold" for="type">Type</label>
        <input
          id="type"
          v-model="type"
          type="text"
          placeholder="e.g. Swiss"
          class="border border-gray-300 rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
      </div>

      <div class="mb-4">
        <label class="block mb-2 font-semibold" for="source">Source</label>
        <input
          id="source"
          v-model="source"
          type="text"
          placeholder="e.g. melee.gg"
          class="border border-gray-300 rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
      </div>

      <div class="mb-6">
        <label class="block mb-2 font-semibold" for="externalId">External ID</label>
        <input
          id="externalId"
          v-model="externalId"
          type="text"
          placeholder="e.g. 426872"
          class="border border-gray-300 rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
      </div>

      <button
        :disabled="!formatId || !type || !source || !externalId || isLoading"
        class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed w-full"
        @click="importTournament"
      >
        {{ isLoading ? "Importing…" : "Import Tournament" }}
      </button>

      <p v-if="successMessage" class="text-green-600 mt-4">{{ successMessage }}</p>

      <div v-if="errorMessage" class="mt-4">
        <p class="text-red-600 mb-4">{{ errorMessage }}</p>

        <div v-if="missingCards.length > 0" class="bg-red-50 border border-red-200 rounded p-4">
          <h2 class="font-semibold text-red-800 mb-3">Missing Cards:</h2>
          <div class="space-y-2 max-h-96 overflow-y-auto">
            <div v-for="(card, index) in missingCards" :key="index" class="text-sm text-red-700">
              <span class="font-medium">{{ card.name }}</span>
              <span v-if="card.subtitle" class="text-red-600">{{ card.subtitle }}</span>
              <span class="text-red-600 ml-2">(Qty: {{ card.quantity }})</span>
              <span v-if="card.entrantName" class="block text-xs text-red-500 ml-2">
                Deck for: {{ card.entrantName }}
              </span>
            </div>
          </div>
          <p class="text-xs text-red-600 mt-3">Please add these cards to the database and try again.</p>
        </div>
      </div>
    </div>
  </div>
</template>
