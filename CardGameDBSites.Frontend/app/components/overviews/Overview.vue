<script setup lang="ts">
import {
  PhBooks,
  PhCaretDown,
  PhFaders,
  PhList,
  PhMagnifyingGlass,
  PhSquaresFour,
  PhX,
} from "@phosphor-icons/vue";
import {
  OverviewFilterType,
  type OverviewFilterModel,
} from "./OverviewFilterModel";
import type { OverviewSortModel } from "./OverviewSortModel";
import Dropdown from "../shared/Dropdown.vue";
import CardSearchInput from "~/components/shared/CardSearchInput.vue";
import type { CardDetailApiModel } from "~/api/default";

const props = defineProps<{
  overviewState: ReturnType<typeof useOverviewState>,
  hideSearch: boolean;
  hideFilters: boolean;
  filters: OverviewFilterModel[];
  sortings?: OverviewSortModel[];
  whiteBackground: boolean;
  availableViews?: string[];
  isLoading: boolean;
}>();

const emit = defineEmits<{
  (e: "loadLazyFilter", filter: OverviewFilterModel): void;
}>();

const filtersOpen = ref(false);
const search = ref(props.overviewState.state.search);

function clickFilters() {
  filtersOpen.value = !filtersOpen.value;
}

function loadLazyDropdownItemsIfNeeded(filter: OverviewFilterModel) {
  if (filter.Items.length > 0 || !filter.AutoFillValues) return;
  emit("loadLazyFilter", filter);
}

function setFilter(filter: OverviewFilterModel, value: string) {
  props.overviewState.setFilter(filter, value);
}

function selectFilter(filter: OverviewFilterModel, value: string) {
  props.overviewState.selectFilter(filter, value);
}

function isSelectedFilter(filter: OverviewFilterModel, value: string) {
  return props.overviewState.isSelectedFilter(filter, value);
}

function getFilterValue(filter: OverviewFilterModel): string | undefined {
  return props.overviewState.getFilterValue(filter);
}

function onTextInputFilterSelect(
  filter: OverviewFilterModel,
  card: CardDetailApiModel,
) {
  selectFilter(filter, card.baseId!.toString());
}

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
          class="flex justify-center items-center w-10 text-lg p-2 bg-main-color text-white rounded hover:bg-main-color-hover"
          type="button"
          @click="clickFilters"
        >
          <PhFaders />
        </button>
      </div>
      <div v-if="filtersOpen" class="py-4 border-b-2 border-gray-300">
        <div class="flex gap-4">
          <div
            v-for="filter in filters.filter(
              (filter) => filter.Type === OverviewFilterType.INLINE,
            )"
          >
            <p class="font-bold">{{ filter.DisplayName }}</p>
            <div
              class="flex flex-wrap items-center md:gap-2 bg-gray-300 rounded"
            >
              <div v-for="item in filter.Items" class="flex">
                <input
                  type="checkbox"
                  class="peer invisible w-0 h-0"
                  :id="`${filter.Alias}-${item.Value}`"
                  :value="item.Value"
                  :checked="isSelectedFilter(filter, item.Value)"
                  @change="() => selectFilter(filter, item.Value)"
                />
                <label
                  :for="`${filter.Alias}-${item.Value}`"
                  class="p-0.5 rounded cursor-pointer overflow-hidden hover:bg-main-color peer-checked:bg-main-color"
                >
                  <img class="w-12" :src="item.IconUrl" />
                </label>
              </div>
            </div>
          </div>
        </div>

        <div class="flex gap-4 mt-4">
          <Dropdown
            v-for="filter in filters.filter(
              (filter) => filter.Type === OverviewFilterType.DROPDOWN,
            )"
            @open="() => loadLazyDropdownItemsIfNeeded(filter)"
          >
            <template #button>
              <span>{{ filter.DisplayName }}</span>
              <PhCaretDown />
            </template>
            <template #content>
              <label
                v-for="item in filter.Items"
                :for="`${filter.Alias}-${item.Value}`"
                class="flex items-center gap-2 px-3 py-2 cursor-pointer hover:bg-main-color-hover"
              >
                <input
                  type="checkbox"
                  class="h-4 w-4 bg-white rounded appearance-none checked:bg-checked checked:bg-black"
                  :id="`${filter.Alias}-${item.Value}`"
                  :value="item.Value"
                  :checked="isSelectedFilter(filter, item.Value)"
                  @change="() => selectFilter(filter, item.Value)"
                />
                <p>
                  {{ item.DisplayName }}
                  <img
                    v-if="item.IconUrl"
                    class="class-image"
                    :src="item.IconUrl"
                  />
                </p>
              </label>
            </template>
          </Dropdown>
          <div
            v-if="filters.some((f) => f.Type === OverviewFilterType.DATE)"
            class="flex flex-wrap gap-4"
          >
            <div
              v-for="filter in filters.filter(
                (f) => f.Type === OverviewFilterType.DATE,
              )"
              :key="filter.Alias"
              class="flex gap-2 items-center"
            >
              <p class="font-bold text-sm">{{ filter.DisplayName }}</p>
              <input
                type="date"
                class="border border-gray-300 rounded px-3 py-2 text-sm"
                :value="getFilterValue(filter)"
                @change="
                  ($event) =>
                    setFilter(
                      filter,
                      ($event.target as HTMLInputElement).value,
                    )
                "
              />
            </div>
          </div>
          <button
            v-for="filter in filters.filter(
              (filter) => filter.Type === OverviewFilterType.CHECKBOX,
            )"
            :key="filter.Alias"
            type="button"
            class="px-3 py-1 rounded border text-sm"
            :class="
              isSelectedFilter(filter, 'true')
                ? 'bg-main-color text-white border-main-color'
                : 'bg-white text-gray-600 border-gray-300 hover:border-gray-500'
            "
            @click="() => selectFilter(filter, 'true')"
          >
            {{ filter.DisplayName }}
          </button>
          <div
            v-for="filter in filters.filter(
              (filter) => filter.Type === OverviewFilterType.TEXT_INPUT,
            )"
            :key="filter.Alias"
          >
            <CardSearchInput
              @select="(card) => onTextInputFilterSelect(filter, card)"
            />
          </div>
        </div>
      </div>

      <div class="flex flex-col-reverse gap-8 justify-between pt-4 md:flex-row">
        <div class="flex flex-wrap items-center gap-2">
          <template v-for="filter in overviewState.state.selectedFilters">
            <div
              v-for="filterItem in filter[1]"
              class="flex gap-2 rounded-md border-2 border-main-color p-1 cursor-pointer"
              x-on:click="removeFilter(filterItem)"
            >
              <p class="text-xs">
                <span>{{ filter[0].DisplayName }}</span>
                :
                <span>{{ filterItem }}</span>
              </p>
              <button
                type="button"
                @click="selectFilter(filter[0], filterItem)"
              >
                <PhX class="items-center"></PhX>
              </button>
            </div>
          </template>
        </div>
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
