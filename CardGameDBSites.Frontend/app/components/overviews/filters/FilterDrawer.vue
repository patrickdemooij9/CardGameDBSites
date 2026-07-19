<script setup lang="ts">
import { PhX } from "@phosphor-icons/vue";
import type { OverviewFilterModel } from "../OverviewFilterModel";
import FilterSection from "./FilterSection.vue";

const props = defineProps<{
  open: boolean;
  overviewState: ReturnType<typeof useOverviewState>;
  filters: OverviewFilterModel[];
}>();

const emit = defineEmits<{
  (e: "close"): void;
  (e: "loadLazyFilter", filter: OverviewFilterModel): void;
}>();

const mounted = ref(false);

function close() {
  emit("close");
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === "Escape" && props.open) close();
}

onMounted(() => {
  mounted.value = true;
  window.addEventListener("keydown", onKeydown);
});
onBeforeUnmount(() => {
  window.removeEventListener("keydown", onKeydown);
  document.body.style.removeProperty("overflow");
});

watch(
  () => props.open,
  (isOpen) => {
    if (!import.meta.client) return;
    document.body.style.overflow = isOpen ? "hidden" : "";
  },
);
</script>

<template>
  <Teleport v-if="mounted" to="#root">
    <Transition name="drawer-fade">
      <div
        v-if="open"
        class="fixed inset-0 z-40 bg-black bg-opacity-60"
        @click="close"
      />
    </Transition>
    <Transition name="drawer-slide">
      <aside
        v-if="open"
        class="fixed inset-y-0 right-0 z-50 flex flex-col w-screen md:w-[420px] bg-white shadow-xl"
        role="dialog"
        aria-modal="true"
        aria-label="Filters"
      >
        <!-- Header -->
        <div class="flex items-center justify-between px-4 py-3 border-b border-gray-200">
          <h2 class="text-lg font-bold flex items-center gap-2">
            Filters
            <span
              v-if="overviewState.activeFilterCount.value > 0"
              class="inline-flex items-center justify-center min-w-5 h-5 px-1.5 text-xs rounded-full bg-main-color text-white"
            >
              {{ overviewState.activeFilterCount.value }}
            </span>
          </h2>
          <div class="flex items-center gap-3">
            <button
              type="button"
              class="text-sm text-gray-600 hover:text-gray-900 disabled:opacity-40"
              :disabled="overviewState.activeFilterCount.value === 0"
              @click="overviewState.clearAllFilters"
            >
              Reset all
            </button>
            <button
              type="button"
              class="text-gray-400 hover:text-gray-700 transition-colors"
              aria-label="Close filters"
              @click="close"
            >
              <PhX class="h-6 w-6" />
            </button>
          </div>
        </div>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto px-4">
          <FilterSection
            v-for="filter in filters"
            :key="filter.Alias"
            :overview-state="overviewState"
            :filter="filter"
            @loadLazyFilter="(f) => emit('loadLazyFilter', f)"
          />
          <p v-if="filters.length === 0" class="py-6 text-sm text-gray-500">
            No filters available.
          </p>
        </div>

        <!-- Footer -->
        <div class="flex gap-2 px-4 py-3 border-t border-gray-200">
          <button
            type="button"
            class="flex-1 rounded-md border border-gray-300 bg-white px-3 py-2 text-sm font-semibold text-gray-900 hover:bg-gray-50 disabled:opacity-40"
            :disabled="overviewState.activeFilterCount.value === 0"
            @click="overviewState.clearAllFilters"
          >
            Reset all
          </button>
          <button
            type="button"
            class="flex-1 rounded-md bg-main-color px-3 py-2 text-sm font-semibold text-white hover:bg-main-color-hover"
            @click="close"
          >
            Done
          </button>
        </div>
      </aside>
    </Transition>
  </Teleport>
</template>

<style scoped>
.drawer-fade-enter-active,
.drawer-fade-leave-active {
  transition: opacity 0.2s ease;
}
.drawer-fade-enter-from,
.drawer-fade-leave-to {
  opacity: 0;
}

.drawer-slide-enter-active,
.drawer-slide-leave-active {
  transition: transform 0.25s ease;
}
.drawer-slide-enter-from,
.drawer-slide-leave-to {
  transform: translateX(100%);
}
</style>
