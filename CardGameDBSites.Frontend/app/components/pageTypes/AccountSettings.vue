<script setup lang="ts">
import type { AccountSettingsContentModel } from "~/api/umbraco";
import Button from "~/components/shared/Button.vue";
import ButtonType from "~/components/shared/ButtonType";
import { useAppToast } from "~/composables/useAppToast";
import { useSite } from "~/composables/useSite";
import { useAccountStore } from "~/stores/AccountStore";

defineProps<{
  content: AccountSettingsContentModel;
}>();

const accountStore = useAccountStore();
const toast = useAppToast();
const siteSettings = await useSite().getSettings();

const isLoading = ref(true);
const isLoggedIn = ref(false);
const isSaving = ref(false);

const defaultMissingCopiesThreshold = siteSettings.collection.missingCopiesThreshold;
// v-model.number leaves an emptied input as "", which means "use the default".
const missingCopiesThreshold = ref<number | "">("");

const thresholdToSave = computed(() =>
  missingCopiesThreshold.value === "" ? null : missingCopiesThreshold.value,
);

const isThresholdValid = computed(
  () =>
    thresholdToSave.value === null ||
    (Number.isInteger(thresholdToSave.value) &&
      thresholdToSave.value >= 1 &&
      thresholdToSave.value <= 99),
);

onMounted(async () => {
  isLoggedIn.value = await accountStore.checkLogin();
  missingCopiesThreshold.value = accountStore.member?.settings.missingCopiesThreshold ?? "";
  isLoading.value = false;
});

async function save() {
  if (!isThresholdValid.value) return;

  isSaving.value = true;
  try {
    await accountStore.updateSettings({ missingCopiesThreshold: thresholdToSave.value });
    toast.success("Settings saved");
  } catch {
    toast.error("Could not save your settings");
  } finally {
    isSaving.value = false;
  }
}
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <h1>{{ content.properties?.title ?? "Settings" }}</h1>
    <span
      v-if="content.properties?.description"
      v-html="content.properties?.description.markup"
    ></span>
  </div>

  <div v-if="isLoading" class="flex justify-center items-center py-12">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>

  <div v-else-if="isLoggedIn" class="container px-4 md:px-8 pb-8">
    <form class="max-w-xl space-y-6" @submit.prevent="save">
      <section class="rounded border border-gray-300 bg-surface p-4">
        <h2 class="text-lg font-bold mb-3">Collection</h2>
        <label for="missing-copies-threshold" class="block font-semibold">
          Copies needed for a full playset
        </label>
        <p class="text-sm text-gray-600 mb-2">
          The "Missing copies" filter shows cards you own fewer copies of than this.
          Leave empty to use the default ({{ defaultMissingCopiesThreshold }}).
        </p>
        <input
          id="missing-copies-threshold"
          v-model.number="missingCopiesThreshold"
          type="number"
          min="1"
          max="99"
          :placeholder="defaultMissingCopiesThreshold.toString()"
          class="w-24 rounded-md border border-gray-300 px-3 py-2"
        />
        <p v-if="!isThresholdValid" class="mt-2 text-sm text-red-600">
          Enter a whole number between 1 and 99.
        </p>
      </section>

      <Button
        :button-type="ButtonType.Success"
        :loading="isSaving"
        :disabled="!isThresholdValid"
        @click="save"
      >
        Save
      </Button>
    </form>
  </div>

  <div v-else class="container px-4 md:px-8 pb-8">
    <p class="text-lg font-bold text-center">
      <NuxtLink to="/login">Log in</NuxtLink> to change your settings.
    </p>
  </div>
</template>
