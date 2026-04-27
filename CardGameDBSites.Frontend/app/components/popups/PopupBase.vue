<template>
  <Teleport to="#root">
    <div
      class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60"
    >
      <div
        class="relative bg-white rounded-lg shadow-lg w-screen mx-4 max-h-screen overflow-auto"
        :class="[getWidthClass()]"
      >
        <button
          class="absolute top-3 right-3 text-gray-400 hover:text-gray-700 transition-colors"
          @click.prevent="emit('close')"
        >
          <PhX class="h-6 w-6" />
        </button>
        <div class="p-6 md:mt-0 mt-4">
          <slot></slot>
        </div>
        <div
          class="bg-gray-50 px-4 py-3 gap-2 sm:flex sm:flex-row-reverse sm:px-6"
        >
          <slot name="actions"></slot>
          <button
            formmethod="dialog"
            type="button"
            @click="emit('close')"
            class="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:mt-0 sm:w-auto"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script lang="ts" setup>
import { PhX } from "@phosphor-icons/vue";
import { PopupSize } from "./PopupTypes";

const props = defineProps<{
  size?: PopupSize;
}>();

const emit = defineEmits<{
  (e: "close"): void;
}>();

function getWidthClass() {
  switch (props.size) {
    case PopupSize.Small:
      return "md:w-[30vw]";
    case PopupSize.Medium:
    default:
      return "md:w-[50vw]";
    case PopupSize.Large:
      return "md:w-[70vw]";
  }
}
</script>
