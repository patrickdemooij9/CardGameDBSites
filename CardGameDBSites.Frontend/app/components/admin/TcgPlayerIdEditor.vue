<script setup lang="ts">
import { DoServerFetch } from "~/helpers/RequestsHelper";

const props = defineProps<{
  variantId: number;
  tcgPlayerId: string | null;
}>();

const emit = defineEmits<{
  saved: [tcgPlayerId: string | null];
}>();

const isEditing = ref(false);
const value = ref("");
const isSaving = ref(false);
const errorMessage = ref<string | null>(null);
const input = ref<HTMLInputElement | null>(null);

async function startEditing() {
  value.value = props.tcgPlayerId ?? "";
  errorMessage.value = null;
  isEditing.value = true;
  await nextTick();
  input.value?.focus();
}

async function save() {
  const tcgPlayerId = value.value.trim() || null;
  if (tcgPlayerId !== null && !/^\d+$/.test(tcgPlayerId)) {
    errorMessage.value = "Must be a number";
    return;
  }

  isSaving.value = true;
  errorMessage.value = null;
  try {
    await DoServerFetch("/api/admin/pricing/tcgplayerid", true, {
      method: "POST",
      body: { variantId: props.variantId, tcgPlayerId },
    });
    isEditing.value = false;
    emit("saved", tcgPlayerId);
  } catch (error: any) {
    errorMessage.value =
      typeof error?.data === "string" ? error.data : "Failed to save";
  } finally {
    isSaving.value = false;
  }
}
</script>

<template>
  <div v-if="isEditing" class="flex flex-col gap-1">
    <div class="flex items-center gap-1">
      <input
        ref="input"
        v-model="value"
        type="text"
        inputmode="numeric"
        placeholder="TCGplayer ID"
        class="border border-gray-300 rounded px-2 py-0.5 text-sm w-28 focus:outline-none focus:ring-2 focus:ring-blue-400"
        @keyup.enter="save"
        @keyup.esc="isEditing = false"
      />
      <button
        class="bg-blue-600 text-white px-2 py-0.5 rounded hover:bg-blue-700 text-xs font-medium disabled:opacity-50"
        :disabled="isSaving"
        @click="save"
      >
        {{ isSaving ? "Saving…" : "Save" }}
      </button>
      <button
        class="text-gray-500 hover:text-gray-700 text-xs px-1"
        :disabled="isSaving"
        @click="isEditing = false"
      >
        Cancel
      </button>
    </div>
    <span v-if="errorMessage" class="text-red-600 text-xs max-w-xs">{{ errorMessage }}</span>
  </div>
  <div v-else class="flex items-center gap-2">
    <slot>
      <span v-if="tcgPlayerId">{{ tcgPlayerId }}</span>
      <span v-else class="text-gray-400">—</span>
    </slot>
    <button class="text-xs text-blue-500 hover:underline" @click="startEditing">
      {{ tcgPlayerId ? "Edit" : "Add" }}
    </button>
  </div>
</template>
