<script setup lang="ts">
import ButtonType from './ButtonType';

const props = defineProps<{
  disabled?: boolean;
  buttonType?: ButtonType
}>();

function getClasses(){
    const buttonType = props.buttonType ?? ButtonType.Primary;
    if (buttonType === ButtonType.Primary) {
        return "bg-theme-surface text-theme";
    } else if (buttonType === ButtonType.Success) {
        return "bg-green-500 text-white hover:bg-green-600";
    } else if (buttonType === ButtonType.Danger){
      return "bg-red-500 text-white hover:bg-red-300";
    } else if (buttonType === ButtonType.Outline){
      return "bg-transparent border border-theme text-theme hover:bg-theme-surface-alt";
    }
}
</script>

<template>
  <button
    type="button"
    :disabled="disabled"
    :class="getClasses()"
    class="pointer theme-radius w-fit px-4 py-2 disabled:cursor-not-allowed disabled:opacity-50 hover:bg-theme-surface-alt"
  >
    <slot></slot>
  </button>
</template>
