<script setup lang="ts">
import { useAccountStore } from "~/stores/AccountStore";

const accountStore = useAccountStore();

async function stopImpersonating() {
  await accountStore.stopImpersonating();
}
</script>

<template>
  <div
    v-if="accountStore.member?.impersonatedBy"
    class="bg-yellow-400 text-black px-4 py-2 flex items-center justify-between"
    role="alert"
  >
    <span>
      You are impersonating <strong>{{ accountStore.member?.name }}</strong>
    </span>
    <button
      class="bg-white text-black px-3 py-1 rounded hover:bg-gray-100 font-semibold"
      @click="stopImpersonating"
    >
      Stop Impersonating
    </button>
  </div>
</template>
