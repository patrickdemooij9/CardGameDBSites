<script setup lang="ts">
import type { SetViewModel } from "~/api/default";
import { useAllSets } from "~/composables/useAllSets";
import { useCollection } from "~/composables/useCollection";
import { GetLatestSet } from "~/helpers/SetHelper";
import SetCard from "~/components/sets/SetCard.vue";
import SetHighlight from "~/components/sets/SetHighlight.vue";

const props = withDefaults(
  defineProps<{
    highlightLatest?: boolean;
  }>(),
  {
    highlightLatest: true,
  },
);

const emit = defineEmits<{
  collectionUpdated: [];
}>();

const { data: sets } = await useAllSets();
const { loadSetCollection } = useCollection();

const highlightedSet = computed(() => (props.highlightLatest ? GetLatestSet(sets.value) : undefined));

const groupedSets = computed(() => {
  const groups = new Map<string, SetViewModel[]>();
  sets.value
    .filter((set) => set.id !== highlightedSet.value?.id)
    .forEach((set) => {
      const category = set.category?.trim() ?? "";
      groups.set(category, [...(groups.get(category) ?? []), set]);
    });

  return [...groups.entries()].map(([category, sets]) => ({ category, sets }));
});

onMounted(loadSetCollection);
</script>

<template>
  <div class="flex flex-col gap-10">
    <SetHighlight
      v-if="highlightedSet"
      :set="highlightedSet"
      @collection-updated="emit('collectionUpdated')"
    />
    <section v-for="group in groupedSets" :key="group.category" class="flex flex-col gap-4">
      <h2 v-if="group.category" class="border-b border-gray-200 pb-2 text-xl font-bold">
        {{ group.category }}
      </h2>
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <SetCard
          v-for="set in group.sets"
          :key="set.id"
          :set="set"
          @collection-updated="emit('collectionUpdated')"
        />
      </div>
    </section>
  </div>
</template>
