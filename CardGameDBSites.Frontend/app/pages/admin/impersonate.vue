<script setup lang="ts">
import { useAccountStore } from "~/stores/AccountStore";

const accountStore = useAccountStore();
await accountStore.checkLogin();

if (!accountStore.member?.isAdmin) {
  throw createError({ statusCode: 403, statusMessage: "Forbidden" });
}

const memberId = ref<number | null>(null);
const errorMessage = ref<string | null>(null);
const isLoading = ref(false);

async function startImpersonating() {
  if (!memberId.value) return;
  errorMessage.value = null;
  isLoading.value = true;
  try {
    await accountStore.impersonate(memberId.value);
    navigateTo("/");
  } catch {
    errorMessage.value = "Could not impersonate this member. Please check the member ID and try again.";
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <div class="container mx-auto px-4 py-8 max-w-md">
    <h1 class="text-2xl font-bold mb-6">Impersonate Member</h1>

    <div class="bg-white rounded shadow p-6">
      <p class="text-gray-600 mb-4">
        Enter the numeric ID of the member you want to impersonate. The session
        will expire after 1 hour and can be stopped at any time via the banner
        shown at the top of the page.
      </p>

      <label class="block mb-2 font-semibold" for="memberId">Member ID</label>
      <input
        id="memberId"
        v-model.number="memberId"
        type="number"
        min="1"
        placeholder="e.g. 1234"
        class="border border-gray-300 rounded px-3 py-2 w-full mb-4 focus:outline-none focus:ring-2 focus:ring-blue-400"
        @keyup.enter="startImpersonating"
      />

      <p v-if="errorMessage" class="text-red-600 mb-4">{{ errorMessage }}</p>

      <button
        :disabled="!memberId || isLoading"
        class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed w-full"
        @click="startImpersonating"
      >
        {{ isLoading ? "Impersonating…" : "Impersonate" }}
      </button>
    </div>
  </div>
</template>
