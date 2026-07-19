<script setup lang="ts">
import { PhX } from "@phosphor-icons/vue";
import {
  OverviewFilterType,
  type OverviewFilterModel,
} from "../OverviewFilterModel";

const props = withDefaults(
  defineProps<{
    overviewState: ReturnType<typeof useOverviewState>;
    entityName?: string;
  }>(),
  { entityName: "cards" },
);

const clauses = computed(() =>
  [...props.overviewState.state.selectedFilters]
    .filter(([, values]) => values.length > 0)
    .map(([filter, values]) => ({ filter, values })),
);

function displayValue(filter: OverviewFilterModel, value: string) {
  const item = filter.Items.find((i) => i.Value === value);
  return item?.DisplayName ?? value;
}

// A checkbox filter reads as a standalone condition (its display name), so it
// gets no "the X is …" framing — just the name as a single removable token.
function isCheckbox(filter: OverviewFilterModel) {
  return filter.Type === OverviewFilterType.CHECKBOX;
}

function isMultiSelect(filter: OverviewFilterModel) {
  return (
    filter.Type === OverviewFilterType.DROPDOWN ||
    filter.Type === OverviewFilterType.INLINE
  );
}

function verb(filter: OverviewFilterModel, count: number) {
  if (isMultiSelect(filter)) {
    if (count <= 1) return "equals";
    return props.overviewState.getMatchMode(filter) === "all"
      ? "is all of"
      : "is one of";
  }
  return "is";
}

// Only multi-select filters with 2+ values can meaningfully switch any/all.
function canToggleMatch(filter: OverviewFilterModel, count: number) {
  return isMultiSelect(filter) && count > 1;
}

function matchTitle(filter: OverviewFilterModel) {
  return props.overviewState.getMatchMode(filter) === "all"
    ? "Matching cards with all of these values — click to match any"
    : "Matching cards with any of these values — click to match all";
}

function remove(filter: OverviewFilterModel, value: string) {
  props.overviewState.selectFilter(filter, value);
}
</script>

<template>
  <p v-if="clauses.length > 0" class="text-sm leading-7">
    <span>All {{ entityName }} where </span>
    <template v-for="(clause, ci) in clauses" :key="clause.filter.Alias">
      <span v-if="ci > 0"> and </span>

      <!-- Checkbox: just the name as a removable token -->
      <template v-if="isCheckbox(clause.filter)">
        <button
          type="button"
          class="inline-flex items-center gap-1 rounded px-1.5 py-0.5 mx-0.5 text-sm font-semibold text-white bg-main-color hover:bg-main-color-hover align-middle"
          :aria-label="`Remove ${clause.filter.DisplayName} filter`"
          @click="remove(clause.filter, clause.values[0]!)"
        >
          {{ clause.filter.DisplayName }}
          <PhX class="w-3 h-3 shrink-0 opacity-80" weight="bold" />
        </button>
      </template>

      <!-- Everything else: "the {Name} {verb} {values}" -->
      <template v-else>
        <span>the <strong>{{ clause.filter.DisplayName }}</strong></span>{{ " " }}<button
          v-if="canToggleMatch(clause.filter, clause.values.length)"
          type="button"
          class="underline decoration-dotted underline-offset-2 hover:text-main-color"
          :title="matchTitle(clause.filter)"
          @click="overviewState.toggleMatchMode(clause.filter)"
        >{{ verb(clause.filter, clause.values.length) }}</button>
        <span v-else>{{ verb(clause.filter, clause.values.length) }}</span>
        <template v-for="value in clause.values" :key="value">{{ " " }}<button
            type="button"
            class="inline-flex items-center gap-1 rounded px-1.5 py-0.5 mx-0.5 text-sm font-semibold text-white bg-main-color hover:bg-main-color-hover align-middle"
            :aria-label="`Remove ${displayValue(clause.filter, value)} from ${clause.filter.DisplayName} filter`"
            @click="remove(clause.filter, value)"
          >{{ displayValue(clause.filter, value) }}<PhX class="w-3 h-3 shrink-0 opacity-80" weight="bold" /></button></template>
      </template>
    </template>
    <span>.</span>
  </p>
</template>
