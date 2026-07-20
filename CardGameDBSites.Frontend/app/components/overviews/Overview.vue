<script setup lang="ts">
import {
  PhBooks,
  PhFaders,
  PhList,
  PhMagnifyingGlass,
  PhSquaresFour,
} from "@phosphor-icons/vue";
import {
  OverviewFilterType,
  type OverviewFilterModel,
} from "./OverviewFilterModel";
import type { OverviewSortModel } from "./OverviewSortModel";
import FilterDrawer from "./filters/FilterDrawer.vue";
import FilterIconGroup from "./filters/FilterIconGroup.vue";
import FilterSentence from "./filters/FilterSentence.vue";

const props = defineProps<{
  overviewState: ReturnType<typeof useOverviewState>,
  hideSearch: boolean;
  hideFilters: boolean;
  hideInlineFilters: boolean;
  filters: OverviewFilterModel[];
  sortings?: OverviewSortModel[];
  whiteBackground: boolean;
  availableViews?: string[];
  isLoading: boolean;
  entityName?: string;
}>();

const emit = defineEmits<{
  (e: "loadLazyFilter", filter: OverviewFilterModel): void;
}>();

const drawerOpen = ref(false);
const search = ref(props.overviewState.state.search);

const inlineFilters = computed(() =>
  props.filters.filter((filter) => filter.Type === OverviewFilterType.INLINE),
);

function handleSubmit(event: Event) {
  event.preventDefault();
  props.overviewState.setSearch(search.value);
}
</script>

<template>
  <div>
    <form class="container px-4 md:px-8 mb-4" @submit="handleSubmit">
      <div :class="{ 'justify-end': hideSearch }" class="flex gap-4">
        <div
          v-if="!hideSearch"
          class="flex grow h-10 overflow-hidden rounded border border-solid border-gray-300"
        >
          <input
            class="pl-4 py-4 grow"
            name="search"
            type="text"
            placeholder="Search..."
            v-model="search"
          />

          <button
            class="flex justify-center items-center w-8 text-lg px-2 bg-white"
            type="button"
            @click="handleSubmit"
          >
            <PhMagnifyingGlass />
          </button>
        </div>
        <button
          v-if="!hideFilters"
          class="relative flex justify-center items-center gap-2 px-3 h-10 text-lg bg-main-color text-white rounded hover:bg-main-color-hover"
          type="button"
          @click="drawerOpen = true"
        >
          <PhFaders />
          <span class="hidden sm:inline text-base">Filters</span>
          <span
            v-if="overviewState.activeFilterCount.value > 0"
            class="inline-flex items-center justify-center min-w-5 h-5 px-1.5 text-xs rounded-full bg-white text-main-color"
          >
            {{ overviewState.activeFilterCount.value }}
          </span>
        </button>
      </div>

      <!-- Desktop-only inline quick filters (icon groups) -->
      <div
        v-if="!hideFilters && !hideInlineFilters && inlineFilters.length > 0"
        class="hidden md:flex flex-wrap gap-4 mt-4"
      >
        <FilterIconGroup
          v-for="filter in inlineFilters"
          :key="filter.Alias"
          :overview-state="overviewState"
          :filter="filter"
          :show-label="true"
        />
      </div>

      <div class="flex flex-col-reverse gap-8 justify-between pt-4 md:flex-row">
        <FilterSentence :overview-state="overviewState" :entity-name="entityName" />
        <div class="flex self-end gap-4">
          <div
            v-if="sortings && sortings.length > 0"
            class="flex items-center gap-2"
          >
            <p>Sort by:</p>
            <select
              :value="overviewState.state.sortBy"
              class="h-8 p-2 bg-main-color text-white rounded hover:bg-main-color-hover"
              @change="overviewState.setSort(($event.target as HTMLOptionElement).value)"
            >
              <option
                v-for="sort in sortings"
                :key="sort.Value"
                :value="sort.Value"
              >
                {{ sort.Name }}
              </option>
            </select>
          </div>
          <div
            v-if="availableViews && availableViews.length > 1"
            class="flex items-center gap-2"
            role="group"
            aria-label="Layout"
          >
            <p aria-hidden="true">Layout:</p>
            <button
              v-if="availableViews.includes('rows')"
              type="button"
              class="flex justify-center items-center w-8 h-8 text-lg p-2 rounded"
              :class="
                overviewState.state.viewMode === 'rows'
                  ? 'bg-main-color text-white'
                  : 'bg-gray-200 text-gray-600 hover:bg-main-color hover:text-white'
              "
              aria-label="Table view"
              :aria-pressed="overviewState.state.viewMode === 'rows'"
              @click="overviewState.setViewMode('rows')"
            >
              <PhList aria-hidden="true" />
            </button>
            <button
              v-if="availableViews.includes('collection')"
              type="button"
              class="flex justify-center items-center w-8 h-8 text-lg p-2 rounded"
              :class="
                overviewState.state.viewMode === 'collection'
                  ? 'bg-main-color text-white'
                  : 'bg-gray-200 text-gray-600 hover:bg-main-color hover:text-white'
              "
              aria-label="Collection view"
              :aria-pressed="overviewState.state.viewMode === 'collection'"
              @click="overviewState.setViewMode('collection')"
            >
              <PhBooks aria-hidden="true" />
            </button>
            <button
              v-if="availableViews.includes('images')"
              type="button"
              class="flex justify-center items-center w-8 h-8 text-lg p-2 rounded"
              :class="
                overviewState.state.viewMode === 'images'
                  ? 'bg-main-color text-white'
                  : 'bg-gray-200 text-gray-600 hover:bg-main-color hover:text-white'
              "
              aria-label="Image view"
              :aria-pressed="overviewState.state.viewMode === 'images'"
              @click="overviewState.setViewMode('images')"
            >
              <PhSquaresFour aria-hidden="true" />
            </button>
          </div>
        </div>
      </div>
    </form>

    <FilterDrawer
      v-if="!hideFilters"
      :open="drawerOpen"
      :overview-state="overviewState"
      :filters="filters"
      @close="drawerOpen = false"
      @loadLazyFilter="(filter) => emit('loadLazyFilter', filter)"
    />

    <div :class="{ 'bg-white': whiteBackground }" class="py-4 relative min-h-20">
      <div id="card-overview" :aria-busy="isLoading" :class="{ 'opacity-40': isLoading }">
        <slot :viewMode="overviewState.state.viewMode"></slot>
      </div>
      <div
        v-if="isLoading"
        class="absolute inset-0 flex items-center justify-center"
      >
        <div
          role="status"
          class="flex flex-col gap-4 items-center bg-white/80 rounded-lg px-6 py-4"
        >
          <svg
            aria-hidden="true"
            class="w-8 h-8 text-gray-200 animate-spin dark:text-gray-600 fill-blue-600"
            viewBox="0 0 100 101"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M100 50.5908C100 78.2051 77.6142 100.591 50 100.591C22.3858 100.591 0 78.2051 0 50.5908C0 22.9766 22.3858 0.59082 50 0.59082C77.6142 0.59082 100 22.9766 100 50.5908ZM9.08144 50.5908C9.08144 73.1895 27.4013 91.5094 50 91.5094C72.5987 91.5094 90.9186 73.1895 90.9186 50.5908C90.9186 27.9921 72.5987 9.67226 50 9.67226C27.4013 9.67226 9.08144 27.9921 9.08144 50.5908Z"
              fill="currentColor"
            />
            <path
              d="M93.9676 39.0409C96.393 38.4038 97.8624 35.9116 97.0079 33.5539C95.2932 28.8227 92.871 24.3692 89.8167 20.348C85.8452 15.1192 80.8826 10.7238 75.2124 7.41289C69.5422 4.10194 63.2754 1.94025 56.7698 1.05124C51.7666 0.367541 46.6976 0.446843 41.7345 1.27873C39.2613 1.69328 37.813 4.19778 38.4501 6.62326C39.0873 9.04874 41.5694 10.4717 44.0505 10.1071C47.8511 9.54855 51.7191 9.52689 55.5402 10.0491C60.8642 10.7766 65.9928 12.5457 70.6331 15.2552C75.2735 17.9648 79.3347 21.5619 82.5849 25.841C84.9175 28.9121 86.7997 32.2913 88.1811 35.8758C89.083 38.2158 91.5421 39.6781 93.9676 39.0409Z"
              fill="currentFill"
            />
          </svg>
          <span>Loading your favorite cards...</span>
        </div>
      </div>
    </div>
  </div>
</template>
