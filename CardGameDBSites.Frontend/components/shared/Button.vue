<script setup lang="ts">
import ButtonType from './ButtonType';

const props = defineProps<{
  disabled?: boolean;
  buttonType?: ButtonType
}>();

function getClasses(){
    const buttonType = props.buttonType ?? ButtonType.Primary;
    if (buttonType === ButtonType.Primary) {
        return "bg-white text-black";
    } else if (buttonType === ButtonType.Success) {
        return "bg-green-500 text-white hover:bg-green-600";
    } else if (buttonType === ButtonType.Danger){
      return "bg-red-500 text-white hover:bg-red-300";
    }
}
</script>

<template>
  <button
    type="button"
    :disabled="disabled"
    :class="getClasses()"
    class="pointer rounded w-fit px-4 py-2 disabled:cursor-not-allowed disabled:bg-gray-200 hover:bg-gray-200"
  >
    <slot></slot>
  </button>
</template>
