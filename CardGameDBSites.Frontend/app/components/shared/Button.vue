<script setup lang="ts">
import ButtonType from './ButtonType';

const props = defineProps<{
  disabled?: boolean;
  buttonType?: ButtonType;
  loading?: boolean;
}>();

function getClasses(){
    const buttonType = props.buttonType ?? ButtonType.Primary;
    if (buttonType === ButtonType.Primary) {
        return "bg-white text-black";
    } else if (buttonType === ButtonType.Success) {
        return "bg-green-500 text-white hover:bg-green-600";
    } else if (buttonType === ButtonType.Danger){
      return "bg-red-500 text-white hover:bg-red-300";
    } else if (buttonType === ButtonType.Outline){
      return "bg-transparent border border-black text-black hover:bg-gray-200";
    }
}
</script>

<template>
  <button
    type="button"
    :disabled="disabled || loading"
    :class="getClasses()"
    class="pointer rounded w-fit px-4 py-2 disabled:cursor-not-allowed disabled:bg-gray-200 hover:bg-gray-200 flex items-center justify-center gap-2"
  >
    <div v-if="loading" class="animate-spin rounded-full h-4 w-4 border-b-2 border-current"></div>
    <slot v-if="!loading"></slot>
  </button>
</template>
