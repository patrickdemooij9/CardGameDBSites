<script setup lang="ts">
import type { SetViewModel } from "~/api/default";
import { useCollection } from "~/composables/useCollection";
import { FormatSetMeta } from "~/helpers/SetHelper";
import SetBanner from "./SetBanner.vue";
import SetCollectionStatus from "./SetCollectionStatus.vue";

const props = defineProps<{
  set: SetViewModel;
}>();

const emit = defineEmits<{
  collectionUpdated: [];
}>();

const { setCollectionMode } = useCollection();
const meta = computed(() => FormatSetMeta(props.set, true, setCollectionMode.value !== "cards"));
</script>

<template>
  <div class="flex flex-col overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md">
    <NuxtLink :to="set.urlSegment" class="no-underline">
      <SetBanner :set="set" class="h-24" />
    </NuxtLink>
    <div class="flex grow flex-col gap-1 p-4">
      <div class="flex items-start justify-between gap-2">
        <NuxtLink :to="set.urlSegment" class="no-underline font-semibold text-gray-900 hover:text-gray-600">
          {{ set.displayName }}
        </NuxtLink>
        <span v-if="set.code" class="shrink-0 rounded bg-gray-100 px-1.5 py-0.5 text-xs font-bold uppercase text-gray-600">
          {{ set.code }}
        </span>
      </div>
      <p v-if="meta" class="text-sm text-gray-500">{{ meta }}</p>
    </div>
    <div v-if="setCollectionMode" class="border-t border-gray-100 px-4 py-3">
      <SetCollectionStatus :set="set" @collection-updated="emit('collectionUpdated')" />
    </div>
  </div>
</template>
