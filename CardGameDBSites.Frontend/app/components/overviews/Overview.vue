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
import type OverviewRefreshModel from "./OverviewRefreshModel";
import CardSearchInput from "~/components/shared/CardSearchInput.vue";
import type { CardDetailApiModel } from "~/api/default";
import type { LocationQueryValue } from "vue-router";

const props = defineProps<{
  hideSearch: boolean;
  hideFilters: boolean;
  filters: OverviewFilterModel[];
  sortings?: OverviewSortModel[];
  whiteBackground: boolean;
  enableQueryStringSync: boolean;
  availableViews?: string[];
}>();

defineExpose({
  setPage,
  getPage,
});

const emit = defineEmits<{
  (e: "reload", value: OverviewRefreshModel): void;
  (e: "loadLazyFilter", filter: OverviewFilterModel): void;
}>();

const route = useRoute();

const defaultView =
  props.availableViews && props.availableViews.length > 0
    ? props.availableViews[0]
    : "images";

const viewMode = ref<string>(route.query.view?.toString() ?? defaultView!);

function setViewMode(mode: string) {
  viewMode.value = mode;
  reloadData();
}

const search = ref(route.query.search?.toString() ?? "");

const selectedFilters = ref<Map<OverviewFilterModel, string[]>>(new Map());
const isLoading = ref(false);
const page = ref(1);
const pageNumberString = route.query["page"];
if (pageNumberString) {
  page.value = Number.parseInt(pageNumberString as string);
}

const selectedSort = ref(route.query.sortBy?.toString() ?? "");

const filtersOpen = ref(false);

function clickFilters() {
  filtersOpen.value = !filtersOpen.value;
}

function getPage() {
  return page.value;
}

function setPage(newPageNumber: number, forceReload = true) {
  const shouldReload = forceReload || newPageNumber !== page.value;

  page.value = newPageNumber;

  if (shouldReload) {
    reloadData();
  }
}

function loadLazyDropdownItemsIfNeeded(filter: OverviewFilterModel) {
  if (filter.Items.length > 0 || !filter.AutoFillValues) return;
  emit("loadLazyFilter", filter);
}

function setFilter(filter: OverviewFilterModel, value: string) {
  selectedFilters.value.set(filter, [value]);
  reloadData();
}

function selectFilter(filter: OverviewFilterModel, value: string) {
  const items = selectedFilters.value.get(filter) || [];
  if (items.includes(value)) {
    const index = items.indexOf(value);
    items.splice(index, 1);
  } else {
    items.push(value);
  }
  selectedFilters.value.set(filter, items);
  reloadData();
}

function isSelectedFilter(filter: OverviewFilterModel, value: string) {
  const items = selectedFilters.value.get(filter) || [];
  return items.includes(value);
}

function getFilterValue(filter: OverviewFilterModel): string | undefined {
  const items = selectedFilters.value.get(filter) || [];
  return items[0];
}

function onTextInputFilterSelect(
  filter: OverviewFilterModel,
  card: CardDetailApiModel,
) {
  if (!selectedFilters.value.get(filter)) {
    selectedFilters.value.set(filter, []);
  }
  selectedFilters.value.get(filter)!.push(card.baseId!.toString());
  reloadData();
}

function handleSubmit(event: Event) {
  event.preventDefault();
  reloadData();
}

function reloadData() {
  if (import.meta.client && props.enableQueryStringSync) {
    const url = new URL(window.location.href.split("?")[0]!);
    if (page.value !== 1) {
      url.searchParams.append("page", page.value.toString());
    }
    if (search.value) {
      url.searchParams.append("search", search.value);
    }
    if (selectedSort.value && props.sortings![0]!.Value !== selectedSort.value) {
      url.searchParams.append("sortBy", selectedSort.value);
    }
    if (viewMode.value !== defaultView) {
      url.searchParams.append("view", viewMode.value);
    }
    selectedFilters.value.forEach((values, filter) => {
      values.forEach((value) => {
        if (value) {
          url.searchParams.append(filter.Alias, value);
        }
      });
    });
    history.replaceState(history.state, "", url);
  }

  isLoading.value = true;

  const filters = new Map<OverviewFilterModel, string[]>();
  selectedFilters.value &&
    selectedFilters.value.forEach((values, filter) => {
      filters.set(props.filters.find((f) => f.Alias === filter.Alias)!, values);
    });
  emit("reload", {
    Query: search.value,
    SelectedFilters: filters,
    PageNumber: page.value,
    SortBy: selectedSort.value || undefined,
    LoadedCallback: () => {
      isLoading.value = false;
    },
  });
}

watch(
  () => props.filters,
  (newVal, oldVal) => {
    // Compare JSON stringified values for a shallow equality check
    if (JSON.stringify(newVal) !== JSON.stringify(oldVal)) {
      init();
    }
  },
  { deep: true },
);

function init() {
  selectedFilters.value = new Map();
  if (props.sortings && props.sortings.length > 0) {
    selectedSort.value = props.sortings[0]!.Value;
  }
  props.filters.forEach((filter) => {
    if (route.query[filter.Alias]) {
      const values = route.query[filter.Alias];
      if (Array.isArray(values)) {
        selectedFilters.value.set(
          filter,
          values.map((v) => v!.toString()),
        );
      } else {
        selectedFilters.value.set(filter, [values!.toString()]);
      }
    } else if (filter.DefaultEnabled) {
      selectedFilters.value.set(filter, ["true"]);
    }
  });

  reloadData();
}

init();
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
          <template v-for="filter in selectedFilters">
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
              v-model="selectedSort"
              class="h-8 p-2 bg-main-color text-white rounded hover:bg-main-color-hover"
              @change="reloadData"
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
          <!--@if (Model.Config.AvailableViews.Length > 1)
            {
                <div class="flex items-center gap-2">
                    <p>
                        Layout:
                    </p>

                    @if (Model.Config.AvailableViews.Contains(OverviewViewType.Rows))
                    {
                        <button type="button" class="flex justify-center items-center w-8 h-8 text-lg p-2 bg-main-color text-white rounded hover:bg-main-color-hover" x-on:click="listStyle = true;">
                            <i class="ph ph-list"></i>
                        </button>
                    }
                    @if (Model.Config.AvailableViews.Contains(OverviewViewType.Images))
                    {
                        <button type="button" class="flex justify-center items-center w-8 h-8 text-lg p-2 bg-main-color text-white rounded hover:bg-main-color-hover" x-on:click="listStyle = false;">
                            <i class="ph ph-squares-four"></i>
                        </button>
                    }
                </div>
            }-->
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
                viewMode === 'rows'
                  ? 'bg-main-color text-white'
                  : 'bg-gray-200 text-gray-600 hover:bg-main-color hover:text-white'
              "
              aria-label="Table view"
              :aria-pressed="viewMode === 'rows'"
              @click="setViewMode('rows')"
            >
              <PhList aria-hidden="true" />
            </button>
            <button
              v-if="availableViews.includes('collection')"
              type="button"
              class="flex justify-center items-center w-8 h-8 text-lg p-2 rounded"
              :class="
                viewMode === 'collection'
                  ? 'bg-main-color text-white'
                  : 'bg-gray-200 text-gray-600 hover:bg-main-color hover:text-white'
              "
              aria-label="Collection view"
              :aria-pressed="viewMode === 'collection'"
              @click="setViewMode('collection')"
            >
              <PhBooks aria-hidden="true" />
            </button>
            <button
              v-if="availableViews.includes('images')"
              type="button"
              class="flex justify-center items-center w-8 h-8 text-lg p-2 rounded"
              :class="
                viewMode === 'images'
                  ? 'bg-main-color text-white'
                  : 'bg-gray-200 text-gray-600 hover:bg-main-color hover:text-white'
              "
              aria-label="Image view"
              :aria-pressed="viewMode === 'images'"
              @click="setViewMode('images')"
            >
              <PhSquaresFour aria-hidden="true" />
            </button>
          </div>
        </div>
      </div>
    </form>
    <div :class="{ 'bg-white': whiteBackground }" class="py-4 relative">
      <div id="card-overview" v-show="!isLoading">
        <slot :viewMode="viewMode"></slot>
      </div>
      <div class="h-20" v-show="isLoading">
        <div
          role="status"
          class="flex flex-col gap-4 items-center absolute -translate-x-1/2 -translate-y-1/2 top-2/4 left-1/2"
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
