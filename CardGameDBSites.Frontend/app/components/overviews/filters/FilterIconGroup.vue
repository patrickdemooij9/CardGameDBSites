<script setup lang="ts">
import type { OverviewFilterModel } from "../OverviewFilterModel";

const props = defineProps<{
  overviewState: ReturnType<typeof useOverviewState>;
  filter: OverviewFilterModel;
  showLabel?: boolean;
}>();

const uid = useId();

function inputId(value: string) {
  return `${uid}-${props.filter.Alias}-${value}`;
}

function isSelected(value: string) {
  return props.overviewState.isSelectedFilter(props.filter, value);
}

function toggle(value: string) {
  props.overviewState.selectFilter(props.filter, value);
}
</script>

<template>
  <div>
    <p v-if="showLabel" class="font-bold mb-1">{{ filter.DisplayName }}</p>
    <div class="flex flex-wrap items-center gap-1 md:gap-2 bg-gray-300 rounded">
      <div v-for="item in filter.Items" :key="item.Value" class="flex">
        <input
          type="checkbox"
          class="peer invisible w-0 h-0"
          :id="inputId(item.Value)"
          :value="item.Value"
          :checked="isSelected(item.Value)"
          @change="() => toggle(item.Value)"
        />
        <label
          :for="inputId(item.Value)"
          class="p-0.5 rounded cursor-pointer overflow-hidden hover:bg-main-color peer-checked:bg-main-color"
          :title="item.DisplayName"
        >
          <img v-if="item.IconUrl" class="w-12" :src="item.IconUrl" :alt="item.DisplayName" />
          <span v-else class="inline-block px-3 py-1">{{ item.DisplayName }}</span>
        </label>
      </div>
    </div>
  </div>
</template>
