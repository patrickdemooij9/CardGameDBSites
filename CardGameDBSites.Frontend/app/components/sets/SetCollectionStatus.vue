<script setup lang="ts">
import { PhCheckCircle, PhPackage } from "@phosphor-icons/vue";
import type { SetViewModel } from "~/api/default";
import { useCollection } from "~/composables/useCollection";
import { GetProgressPercent } from "~/helpers/SetHelper";
import ProgressBar from "~/components/shared/ProgressBar.vue";

const props = defineProps<{
  set: SetViewModel;
  dark?: boolean;
}>();

const emit = defineEmits<{
  collectionUpdated: [];
}>();

const { setCollectionMode, getSetProgress, isSetOwned, toggleSet } = useCollection();
const isProcessing = ref(false);

const owned = computed(() => getSetProgress(props.set.id)?.ownedCards ?? 0);
const total = computed(() => getSetProgress(props.set.id)?.totalCards ?? 0);

async function toggle() {
  isProcessing.value = true;
  try {
    await toggleSet(props.set.id);
    emit("collectionUpdated");
  } finally {
    isProcessing.value = false;
  }
}
</script>

<template>
  <div v-if="setCollectionMode === 'cards'">
    <p v-if="total === 0" class="text-sm" :class="dark ? 'text-white/70' : 'text-gray-500'">
      No cards listed yet
    </p>
    <ProgressBar
      v-else
      :percent-filled="GetProgressPercent(owned, total)"
      :description="`${owned} / ${total}`"
      :dark="dark"
    />
  </div>
  <div v-else-if="setCollectionMode === 'sets'" class="flex items-center justify-between gap-2 text-sm">
    <span
      v-if="isSetOwned(set.id)"
      class="flex items-center gap-1 font-semibold"
      :class="dark ? 'text-white' : 'text-green-600'"
    >
      <PhCheckCircle weight="fill" /> Owned
    </span>
    <button
      type="button"
      class="ml-auto flex items-center gap-1 font-semibold disabled:opacity-50"
      :class="dark ? 'text-white hover:text-white/80' : 'text-blue-600 hover:text-blue-800'"
      :disabled="isProcessing"
      @click="toggle"
    >
      <template v-if="isSetOwned(set.id)">Remove</template>
      <template v-else><PhPackage /> Add whole box</template>
    </button>
  </div>
</template>
