<script setup lang="ts">
import { useAccountStore } from "~/stores/AccountStore";
import { DoServerFetch } from "~/helpers/RequestsHelper";

const accountStore = useAccountStore();

const formatId = ref<number | null>(null);
const type = ref("");
const source = ref("");
const externalId = ref("");

const isLoading = ref(false);
const successMessage = ref<string | null>(null);
const errorMessage = ref<string | null>(null);

async function importTournament() {
  if (!formatId.value || !type.value || !source.value || !externalId.value)
    return;

  isLoading.value = true;
  successMessage.value = null;
  errorMessage.value = null;

  try {
    await DoServerFetch("/api/management", true, {
      method: "POST",
      body: {
        formatId: formatId.value,
        type: type.value,
        source: source.value,
        externalId: externalId.value,
      },
    });
    successMessage.value = "Tournament imported successfully.";
  } catch {
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
  <div class="container mx-auto px-4 py-8 max-w-md">
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

      <p v-if="successMessage" class="text-green-600 mb-4">{{ successMessage }}</p>
      <p v-if="errorMessage" class="text-red-600 mb-4">{{ errorMessage }}</p>

      <button
        :disabled="!formatId || !type || !source || !externalId || isLoading"
        class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed w-full"
        @click="importTournament"
      >
        {{ isLoading ? "Importing…" : "Import Tournament" }}
      </button>
    </div>
  </div>
</template>
