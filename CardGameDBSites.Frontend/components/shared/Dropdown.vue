<script setup lang="ts">
const emit = defineEmits<{
  (e: "open"): void;
}>();

const isOpen = ref(false);

function clickDropdownButton(newValue: boolean) {
  isOpen.value = newValue;
  emit("open");
}
</script>
<template>
  <div
    class="rounded bg-main-color text-white"
    v-click-outside="() => clickDropdownButton(false)"
  >
    <button
      type="button"
      class="flex gap-2 py-2 px-4 items-center"
      @click="() => clickDropdownButton(!isOpen)"
    >
      <slot name="button"></slot>
    </button>
    <div
      v-if="isOpen"
      class="absolute mt-2 z-10 max-h-72 bg-main-color rounded overflow-auto scrollbar scrollbar:!w-1.5 scrollbar-thumb:!rounded scrollbar-thumb:!bg-slate-300 md:shadow-xl"
    >
      <slot name="content"></slot>
    </div>
  </div>
</template>
