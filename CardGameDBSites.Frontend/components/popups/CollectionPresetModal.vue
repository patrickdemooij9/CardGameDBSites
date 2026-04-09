<template>
  <PopupBase :size="PopupSize.Medium" @close="emit('close')">
    <h3 class="text-lg font-bold mb-4">Add preset to collection</h3>

    <div class="space-y-3">
      <div
        v-for="preset in presets"
        :key="preset.id"
        class="border rounded-lg p-4 cursor-pointer transition-colors"
        :class="
          selectedPresetId === preset.id
            ? 'border-blue-500 bg-blue-50'
            : 'hover:border-blue-300 hover:bg-gray-50'
        "
        @click="selectedPresetId = preset.id"
      >
        <div class="font-semibold">{{ preset.name }}</div>
        <div class="text-sm text-gray-500">{{ preset.cardCount }} cards</div>
      </div>
    </div>

    <template #actions>
      <Button
        @click="handleApply"
        :button-type="ButtonType.Success"
        :disabled="!selectedPresetId || isApplying"
        class="btn disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {{ isApplying ? "Adding..." : "Add to collection" }}
      </Button>
    </template>
  </PopupBase>
</template>

<script setup lang="ts">
import PopupBase from "./PopupBase.vue";
import { PopupSize } from "./PopupTypes";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";
import type { PresetApiModel } from "~/models/PresetApiModel";
import { useCollection } from "~/composables/useCollection";
import { useAppToast } from "~/composables/useAppToast";

const props = defineProps<{
  presets: PresetApiModel[];
}>();

const emit = defineEmits<{
  (e: "close"): void;
  (e: "applied"): void;
}>();

const collectionStore = useCollection();
const appToast = useAppToast();

const selectedPresetId = ref<string | null>(null);
const isApplying = ref(false);

async function handleApply() {
  if (!selectedPresetId.value) return;

  isApplying.value = true;
  try {
    await collectionStore.applyPreset(selectedPresetId.value);
    appToast.success("Preset added to your collection!");
    emit("applied");
    emit("close");
  } catch {
    appToast.error("An error occurred while applying the preset.");
  } finally {
    isApplying.value = false;
  }
}
</script>
