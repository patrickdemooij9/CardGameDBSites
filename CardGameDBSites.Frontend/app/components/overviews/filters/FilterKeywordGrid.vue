<script setup lang="ts">
import { PhArrowSquareOut, PhCircle, PhCircleHalf, PhCircleNotch, PhLink } from "@phosphor-icons/vue";
import type { OverviewFilterItemModel, OverviewFilterModel } from "../OverviewFilterModel";
import { GetKeywordAttributes } from "~/helpers/OverviewHelper";
import { KeywordRelationLabels, ToKeywordValue } from "~/helpers/KeywordFilterHelper";

const props = defineProps<{
  overviewState: ReturnType<typeof useOverviewState>;
  filter: OverviewFilterModel;
}>();

const relationIcons: Record<string, { icon: Component; weight: "fill" | "regular" }> = {
  Has: { icon: PhCircle, weight: "fill" },
  Conditional: { icon: PhCircleHalf, weight: "fill" },
  Grants: { icon: PhArrowSquareOut, weight: "regular" },
  Synergy: { icon: PhLink, weight: "regular" },
};

const columns = computed(() =>
  GetKeywordAttributes()
    .filter((it) => relationIcons[it.relation])
    .map((it) => ({
      ...it,
      ...relationIcons[it.relation]!,
      label: KeywordRelationLabels[it.relation] ?? it.displayName,
    })),
);

const groups = computed(() => {
  const result: { name?: string; items: OverviewFilterItemModel[] }[] = [];
  props.filter.Items.forEach((item) => {
    const last = result[result.length - 1];
    if (last && last.name === item.Group) last.items.push(item);
    else result.push({ name: item.Group, items: [item] });
  });
  return result;
});

function isSelected(item: OverviewFilterItemModel, relation: string) {
  return props.overviewState.isSelectedFilter(props.filter, ToKeywordValue(item.Value, relation));
}

function hasSelection(item: OverviewFilterItemModel) {
  return columns.value.some((column) => isSelected(item, column.relation));
}

function toggle(item: OverviewFilterItemModel, relation: string) {
  props.overviewState.selectFilter(props.filter, ToKeywordValue(item.Value, relation));
}
</script>

<template>
  <div class="flex flex-col gap-4">
    <ul class="grid grid-cols-2 gap-x-4 gap-y-2 rounded-lg border border-gray-200 bg-gray-50 p-3 text-sm text-gray-700">
      <li v-for="column in columns" :key="column.relation" class="flex items-center gap-2">
        <component :is="column.icon" :weight="column.weight" class="shrink-0" />
        {{ column.label }}
      </li>
    </ul>

    <p v-if="filter.Loading" class="flex items-center gap-2 text-sm text-gray-500">
      <PhCircleNotch class="animate-spin" />
      Loading options…
    </p>

    <div v-for="(group, gi) in groups" :key="group.name ?? gi">
      <p v-if="group.name" class="mb-1 text-xs font-semibold uppercase tracking-wide text-gray-500">
        {{ group.name }}
      </p>
      <div
        v-for="item in group.items"
        :key="item.Value"
        class="-mx-2 flex items-center justify-between gap-2 rounded-lg px-2 py-1 hover:bg-gray-100"
      >
        <span :class="{ 'font-bold': hasSelection(item) }">{{ item.DisplayName }}</span>
        <div class="flex gap-1.5">
          <button
            v-for="column in columns"
            :key="column.relation"
            type="button"
            class="flex h-9 w-9 items-center justify-center rounded-lg border"
            :class="
              isSelected(item, column.relation)
                ? 'border-black bg-black text-white'
                : 'border-gray-200 text-gray-500 hover:border-gray-400'
            "
            :title="`${item.DisplayName}: ${column.label}`"
            :aria-label="`${item.DisplayName}: ${column.label}`"
            :aria-pressed="isSelected(item, column.relation)"
            @click="toggle(item, column.relation)"
          >
            <component :is="column.icon" :weight="column.weight" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
