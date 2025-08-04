<script setup lang="ts">
import {
  PhCaretDown,
  PhFaders,
  PhMagnifyingGlass,
  PhX,
} from "@phosphor-icons/vue";
import type { OverviewFilterModel } from "./OverviewFilterModel";
import Dropdown from "../shared/Dropdown.vue";
import type OverviewRefreshModel from "./OverviewRefreshModel";

defineProps<{
  hideSearch: boolean;
  hideFilters: boolean;
  filters: OverviewFilterModel[];
  whiteBackground: boolean;
}>();

defineExpose({
  setPage,
  getPage
});

const emit = defineEmits<{
  (e: "reload", value: OverviewRefreshModel): void;
  (e: "loadLazyFilter", filter: OverviewFilterModel): void;
}>();

const route = useRoute();

const search = ref(route.query.search?.toString() ?? "");

const selectedFilters = ref<Record<string, string[]>>({});
const isLoading = ref(false);
const page = ref(1);
const pageNumberString = route.query["page"];
if (pageNumberString) {
  page.value = Number.parseInt(pageNumberString as string);
}

const filtersOpen = ref(false);

function clickFilters() {
  filtersOpen.value = !filtersOpen.value;
}

function getPage(){
  return page.value;
}

function setPage(newPageNumber: number) {
  const shouldReload = newPageNumber !== page.value;

  page.value = newPageNumber;

  if (shouldReload){
    reloadData();
  }
}

function loadLazyDropdownItemsIfNeeded(filter: OverviewFilterModel) {
  if (filter.Items.length > 0 || !filter.AutoFillValues) return;
  emit("loadLazyFilter", filter);
}

function selectFilter(alias: string, value: string) {
  if (selectedFilters.value[alias]) {
    const items = selectedFilters.value[alias];
    if (items.includes(value)) {
      const index = items.indexOf(value);
      items.splice(index, 1);
    } else {
      items.push(value);
    }
  } else {
    selectedFilters.value[alias] = [value];
  }
  reloadData();
}

function isSelectedFilter(alias: string, value: string) {
  return (
    selectedFilters.value[alias] && selectedFilters.value[alias].includes(value)
  );
}

function handleSubmit(event: Event) {
  event.preventDefault();
  reloadData();
}

function reloadData() {
  const url = new URL(window.location.href.split("?")[0]);
  if (page.value !== 1) {
    url.searchParams.append("page", page.value.toString());
  }
  if (search.value) {
    url.searchParams.append("search", search.value);
  }
  /*Object.entries(selectedFilters.value).forEach((entry) => {
    entry[1].forEach((value) => {
      url.searchParams.append(entry[0], value);
    });
  });*/
  history.replaceState("", "", url);

  isLoading.value = true;
  emit("reload", {
    Query: search.value,
    SelectedFilters: selectedFilters.value,
    PageNumber: page.value,
    LoadedCallback: () => {
      isLoading.value = false;
    },
  });
}

reloadData();
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
          <div v-for="filter in filters.filter((filter) => filter.IsInline)">
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
                  :checked="isSelectedFilter(filter.Alias, item.Value)"
                  @change="() => selectFilter(filter.Alias, item.Value)"
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
          <!--<div class="rounded border-b border-main-color" x-data="{results: null, term: null, open: false}" x-on:click.outside="open = false">
                        <input type="text" class="py-2 px-4" placeholder="Deck includes cards..." x-model="term" x-on:click="open = !open" x-on:input.debounce="results = await $fetch('/umbraco/api/dataapi/searchcards?term=' + term)" />

                        <div class="absolute text-white mt-2 z-10 max-h-72 bg-main-color rounded overflow-auto scrollbar:!w-1.5 scrollbar-thumb:!rounded scrollbar-thumb:!bg-slate-300 md:shadow-xl" :class="open ? '' : 'invisible'">
                            <template x-for="item in JSON.parse(results)">
                                <option class="px-3 py-2 cursor-pointer" :value="item.value" x-text="item.label" x-on:click="addFilterValue('@filter.Alias', item.value, item.label)"></option>
                            </template>
                        </div>
                    </div>-->
          <Dropdown
            v-for="filter in filters.filter((filter) => !filter.IsInline)"
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
                  :checked="isSelectedFilter(filter.Alias, item.Value)"
                  @change="() => selectFilter(filter.Alias, item.Value)"
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
        </div>
      </div>

      <div class="flex flex-col-reverse gap-8 justify-between pt-4 md:flex-row">
        <div class="flex flex-wrap items-center gap-2">
          <template v-for="filter in Object.entries(selectedFilters)">
            <div
              v-for="filterItem in filter[1]"
              class="flex gap-2 rounded-md border-2 border-main-color p-1 cursor-pointer"
              x-on:click="removeFilter(filterItem)"
            >
              <p class="text-xs">
                <span>{{ filter[0] }}</span>
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
          <!--@if (Model.Config.Sortings.Length > 0)
            {
                <div class="flex items-center gap-2">
                    <p>Sort by:</p>
                    <select name="sortBy" class="h-8 p-2 bg-main-color text-white rounded hover:bg-main-color-hover" x-on:change="updateOverview()">
                        @foreach (var sort in Model.Config.Sortings)
                        {
                            <!option value="@sort.Value" @(sort.Value.Equals(Model.SortBy) ? "selected" : "")>@sort.Name</!option>
                        }
                    </select>
                </div>
            }-->
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
        </div>
      </div>
    </form>
    <div :class="{ 'bg-white': whiteBackground }" class="py-4 relative">
      <div id="card-overview" v-show="!isLoading">
        <slot></slot>
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
