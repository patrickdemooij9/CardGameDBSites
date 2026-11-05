<script setup lang="ts">
import type { SetViewModel } from "~/api/default";
import CmsImage from "~/components/shared/CmsImage.vue";
import { GetBannerColor } from "~/helpers/SetHelper";

defineProps<{
  set: SetViewModel;
  large?: boolean;
}>();
</script>

<template>
  <div
    class="set-banner relative flex items-center justify-center overflow-hidden p-4"
    :style="{ backgroundColor: GetBannerColor(set) }"
  >
    <CmsImage
      v-if="set.imageUrl"
      :src="set.imageUrl"
      :alt="set.displayName"
      :loading="large ? 'eager' : 'lazy'"
      class="relative max-h-full max-w-full object-contain"
    />
    <span
      v-else
      class="relative text-center font-black uppercase leading-tight tracking-wide text-white"
      :class="large ? 'text-4xl md:text-5xl' : 'text-xl'"
    >
      {{ set.displayName }}
    </span>
  </div>
</template>

<style scoped>
.set-banner::before {
  content: "";
  position: absolute;
  inset: 0;
  background-image: radial-gradient(rgb(255 255 255 / 0.15) 1px, transparent 1px);
  background-size: 14px 14px;
}
</style>
