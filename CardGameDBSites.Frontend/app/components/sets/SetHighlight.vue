<script setup lang="ts">
import type { SetViewModel } from "~/api/default";
import { useCollection } from "~/composables/useCollection";
import { FormatReleaseDate, FormatSetMeta, GetBannerColor, IsUpcoming } from "~/helpers/SetHelper";
import SetBanner from "./SetBanner.vue";
import SetCollectionStatus from "./SetCollectionStatus.vue";

const props = defineProps<{
  set: SetViewModel;
}>();

const emit = defineEmits<{
  collectionUpdated: [];
}>();

const { setCollectionMode } = useCollection();
const upcoming = computed(() => IsUpcoming(props.set));
const meta = computed(() => FormatSetMeta(props.set, !upcoming.value, setCollectionMode.value !== "cards"));
</script>

<template>
  <div
    class="grid overflow-hidden rounded-xl shadow-sm md:grid-cols-[2fr_3fr]"
    :style="{ backgroundColor: GetBannerColor(set) }"
  >
    <NuxtLink :to="set.urlSegment" class="no-underline">
      <SetBanner :set="set" large class="h-40 md:h-52" />
    </NuxtLink>
    <div class="flex flex-col justify-center gap-3 bg-black/40 p-6 text-white md:px-8">
      <div class="flex flex-wrap items-center gap-2">
        <span v-if="upcoming" class="rounded-full bg-white/90 px-2.5 py-0.5 text-xs font-semibold text-gray-900">
          Coming {{ FormatReleaseDate(set) }}
        </span>
        <span v-if="set.code" class="rounded bg-white/15 px-2 py-0.5 text-xs font-bold uppercase">
          {{ set.code }}
        </span>
      </div>
      <NuxtLink :to="set.urlSegment" class="no-underline text-2xl font-bold text-white hover:text-white/80">
        {{ set.displayName }}
      </NuxtLink>
      <div v-if="set.subHeading" class="max-w-2xl text-sm text-white/80" v-html="set.subHeading"></div>
      <p v-if="meta" class="text-sm text-white/70">{{ meta }}</p>
      <SetCollectionStatus
        v-if="setCollectionMode"
        :set="set"
        dark
        class="max-w-sm"
        @collection-updated="emit('collectionUpdated')"
      />
    </div>
  </div>
</template>
