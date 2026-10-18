<script setup lang="ts">
import type { FormPropertiesModel } from '~/api/umbraco';

defineProps<{
  content: FormPropertiesModel;
}>();
</script>

<template>
  <div v-if="content.formAlias" class="max-w-2xl">
    <SproutForm :alias="content.formAlias" />
  </div>
</template>
