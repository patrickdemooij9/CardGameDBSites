<script setup lang="ts">
import type { SquadSettingsOptionApiModel } from "~/api/default";
import Button from "~/components/shared/Button.vue";
import ButtonType from "~/components/shared/ButtonType";
import Modal from "~/components/shared/Modal.vue";
import type { CreateDeckModel } from "./models/CreateDeckModel";

const props = defineProps<{
  deck: CreateDeckModel;
  currentFormatName?: string;
  formats: SquadSettingsOptionApiModel[];
}>();

const emit = defineEmits<{
  close: [];
  convert: [typeId: number];
}>();

function getIllegalCardCount(typeId: number) {
  return new Set(props.deck.getIllegalCards(typeId).map((card) => card.baseId))
    .size;
}

const selectedTypeId = ref(
  props.formats.find((format) => getIllegalCardCount(format.typeId) === 0)
    ?.typeId ?? props.formats[0]?.typeId,
);
</script>

<template>
  <Modal title="Convert deck to format" @close="emit('close')">
    <div class="flex items-center gap-3 rounded border px-3 py-2 text-gray-500">
      <input type="radio" disabled />
      <div>
        <p>{{ currentFormatName }}</p>
        <p class="text-xs">Current format</p>
      </div>
    </div>
    <label
      v-for="format in formats"
      :key="format.typeId"
      class="mt-2 flex cursor-pointer items-center gap-3 rounded border px-3 py-2"
      :class="{ 'border-main-color': selectedTypeId === format.typeId }"
    >
      <input v-model="selectedTypeId" type="radio" :value="format.typeId" />
      <div>
        <p>{{ format.name }}</p>
        <p v-if="getIllegalCardCount(format.typeId) === 0" class="text-xs">
          All cards legal
        </p>
        <p v-else class="text-xs text-red-600">
          {{ getIllegalCardCount(format.typeId) }}
          {{ getIllegalCardCount(format.typeId) === 1 ? "card" : "cards" }} not
          legal
        </p>
      </div>
    </label>
    <div class="mt-4 flex justify-end gap-2">
      <Button class="border border-black" @click="emit('close')">Cancel</Button>
      <Button
        :button-type="ButtonType.Success"
        class="border border-black"
        @click="emit('convert', selectedTypeId!)"
      >
        Convert
      </Button>
    </div>
  </Modal>
</template>
