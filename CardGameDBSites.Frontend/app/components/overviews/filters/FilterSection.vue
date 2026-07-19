<script setup lang="ts">
import { PhCaretDown } from "@phosphor-icons/vue";
import {
  OverviewFilterType,
  type OverviewFilterModel,
} from "../OverviewFilterModel";
import type { CardDetailApiModel } from "~/api/default";
import CardSearchInput from "~/components/shared/CardSearchInput.vue";
import FilterIconGroup from "./FilterIconGroup.vue";

const props = defineProps<{
  overviewState: ReturnType<typeof useOverviewState>;
  filter: OverviewFilterModel;
}>();

const emit = defineEmits<{
  (e: "loadLazyFilter", filter: OverviewFilterModel): void;
}>();

const selectedCount = computed(
  () => (props.overviewState.state.selectedFilters.get(props.filter) ?? []).length,
);

// Auto-open sections that already have active selections.
const open = ref(selectedCount.value > 0);

// Auto-opened sections still need their lazy options fetched.
onMounted(() => {
  if (open.value) loadLazyIfNeeded();
});

function toggleOpen() {
  open.value = !open.value;
  if (open.value) loadLazyIfNeeded();
}

function loadLazyIfNeeded() {
  if (props.filter.Items.length > 0 || !props.filter.AutoFillValues) return;
  emit("loadLazyFilter", props.filter);
}

// Show a per-section search once a list gets long enough to be unwieldy.
const SEARCH_THRESHOLD = 8;
const query = ref("");
const showSearch = computed(() => props.filter.Items.length >= SEARCH_THRESHOLD);
const visibleItems = computed(() => {
  const q = query.value.trim().toLowerCase();
  if (!q) return props.filter.Items;
  return props.filter.Items.filter((item) =>
    item.DisplayName.toLowerCase().includes(q),
  );
});

function isSelected(value: string) {
  return props.overviewState.isSelectedFilter(props.filter, value);
}

function toggle(value: string) {
  props.overviewState.selectFilter(props.filter, value);
}

function getValue() {
  return props.overviewState.getFilterValue(props.filter);
}

function setValue(value: string) {
  props.overviewState.setFilter(props.filter, value);
}

function onCardSelect(card: CardDetailApiModel) {
  props.overviewState.selectFilter(props.filter, card.baseId!.toString());
}
</script>

<template>
  <div class="border-b border-gray-200">
    <button
      type="button"
      class="flex w-full items-center justify-between py-3 text-left font-bold"
      :aria-expanded="open"
      @click="toggleOpen"
    >
      <span class="flex items-center gap-2">
        {{ filter.DisplayName }}
        <span
          v-if="selectedCount > 0"
          class="inline-flex items-center justify-center min-w-5 h-5 px-1.5 text-xs rounded-full bg-main-color text-white"
        >
          {{ selectedCount }}
        </span>
      </span>
      <PhCaretDown
        class="transition-transform"
        :class="{ 'rotate-180': open }"
      />
    </button>

    <div v-if="open" class="pb-4">
      <!-- INLINE icon group -->
      <FilterIconGroup
        v-if="filter.Type === OverviewFilterType.INLINE"
        :overview-state="overviewState"
        :filter="filter"
      />

      <!-- DROPDOWN → checkbox list -->
      <div
        v-else-if="filter.Type === OverviewFilterType.DROPDOWN"
        class="flex flex-col gap-1"
      >
        <input
          v-if="showSearch"
          v-model="query"
          type="text"
          :placeholder="`Search ${filter.DisplayName.toLowerCase()}...`"
          class="mb-1 px-2 py-1.5 text-sm rounded border border-gray-300 w-full"
        />
        <div class="flex flex-col gap-1 max-h-64 overflow-auto">
          <label
            v-for="item in visibleItems"
            :key="item.Value"
            :for="`section-${filter.Alias}-${item.Value}`"
            class="flex items-center gap-2 px-1 py-1.5 cursor-pointer rounded hover:bg-gray-100"
          >
            <input
              type="checkbox"
              class="h-4 w-4 bg-white rounded appearance-none border border-gray-300 checked:bg-checked checked:bg-black"
              :id="`section-${filter.Alias}-${item.Value}`"
              :value="item.Value"
              :checked="isSelected(item.Value)"
              @change="() => toggle(item.Value)"
            />
            <span class="flex items-center gap-2">
              {{ item.DisplayName }}
              <img
                v-if="item.IconUrl"
                class="class-image"
                :src="item.IconUrl"
                :alt="item.DisplayName"
              />
            </span>
          </label>
          <p v-if="filter.Items.length === 0" class="text-sm text-gray-500 px-1 py-2">
            No options available.
          </p>
          <p
            v-else-if="visibleItems.length === 0"
            class="text-sm text-gray-500 px-1 py-2"
          >
            No matches for "{{ query }}".
          </p>
        </div>
      </div>

      <!-- DATE -->
      <input
        v-else-if="filter.Type === OverviewFilterType.DATE"
        type="date"
        class="border border-gray-300 rounded px-3 py-2 text-sm w-full"
        :value="getValue()"
        @change="setValue(($event.target as HTMLInputElement).value)"
      />

      <!-- CHECKBOX toggle -->
      <button
        v-else-if="filter.Type === OverviewFilterType.CHECKBOX"
        type="button"
        class="px-3 py-1 rounded border text-sm"
        :class="
          isSelected('true')
            ? 'bg-main-color text-white border-main-color'
            : 'bg-white text-gray-600 border-gray-300 hover:border-gray-500'
        "
        @click="() => toggle('true')"
      >
        {{ filter.DisplayName }}
      </button>

      <!-- TEXT_INPUT (card autocomplete) -->
      <div v-else-if="filter.Type === OverviewFilterType.TEXT_INPUT" class="flex flex-col gap-2">
        <CardSearchInput @select="onCardSelect" />
        <div
          v-if="(overviewState.state.selectedFilters.get(filter) ?? []).length > 0"
          class="flex flex-wrap gap-2"
        >
          <button
            v-for="value in overviewState.state.selectedFilters.get(filter)"
            :key="value"
            type="button"
            class="text-xs rounded-md border-2 border-main-color px-2 py-1"
            @click="toggle(value)"
          >
            {{ value }} ✕
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
