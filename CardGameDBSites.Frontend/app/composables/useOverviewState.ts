import type { OverviewFilterModel } from "~/components/overviews/OverviewFilterModel";
import type { OverviewSortModel } from "~/components/overviews/OverviewSortModel";

export interface OverviewQueryState {
  query: string;
  selectedFilters: Map<OverviewFilterModel, string[]>;
  page: number;
  sortBy?: string;
}

export function useOverviewState(
  filters: Ref<OverviewFilterModel[]>,
  sortings?: Ref<OverviewSortModel[] | undefined>,
  views?: Ref<string[] | undefined>,
  options?: { enableQueryStringSync?: boolean },
) {
  const route = useRoute();

  const defaultSortBy = sortings?.value?.[0]?.Value ?? "";
  const defaultViewMode = views?.value?.[0] ?? 'images';
  const state = reactive({
    search: route.query.search?.toString() ?? "",
    page: Number(route.query.page) || 1,
    sortBy: route.query.sortBy?.toString() ?? defaultSortBy,
    selectedFilters: new Map<OverviewFilterModel, string[]>(),
    // Per-filter match mode. true = "all" (card must have every selected value),
    // false/absent = "any" (card must have at least one). Keyed by filter alias.
    matchAll: new Map<string, boolean>(),
    viewMode: route.query.view?.toString() ?? defaultViewMode,
  });

  function initFromRoute() {
    state.selectedFilters = new Map();
    const matchAllParam = route.query.matchAll;
    const matchAllAliases = new Set(
      (Array.isArray(matchAllParam) ? matchAllParam : [matchAllParam])
        .filter((v): v is string => !!v),
    );
    state.matchAll = new Map();
    filters.value.forEach((filter) => {
      const values = route.query[filter.Alias];
      if (values) {
        state.selectedFilters.set(
          filter,
          (Array.isArray(values) ? values : [values]).map((v) => v!.toString()),
        );
      } else if (filter.DefaultEnabled) {
        state.selectedFilters.set(filter, ["true"]);
      }
      if (matchAllAliases.has(filter.Alias)) {
        state.matchAll.set(filter.Alias, true);
      }
    });
  }
  initFromRoute();

  // Re-initialise only when the *set* of filters changes (aliases added/removed),
  // not when a filter's options are lazily loaded onto it. A deep watch here would
  // fire on lazy option loading and wipe selections that haven't been re-read from
  // the route yet.
  watch(
    () => filters.value.map((f) => f.Alias).join("|"),
    () => initFromRoute(),
  );

  function syncUrl() {
    if (!import.meta.client || !options?.enableQueryStringSync) return;
    const url = new URL(window.location.href.split("?")[0]!);
    if (state.page !== 1) url.searchParams.append("page", String(state.page));
    if (state.search) url.searchParams.append("search", state.search);
    if (state.sortBy && state.sortBy != defaultSortBy) url.searchParams.append("sortBy", state.sortBy);
    if (state.viewMode && state.viewMode != defaultViewMode) url.searchParams.append("view", state.viewMode);
    state.selectedFilters.forEach((values, filter) => {
      values.forEach((v) => v && url.searchParams.append(filter.Alias, v));
      if (state.matchAll.get(filter.Alias) && values.length > 0) {
        url.searchParams.append("matchAll", filter.Alias);
      }
    });
    history.replaceState(history.state, "", url);
  }

  function getMatchMode(filter: OverviewFilterModel): "any" | "all" {
    return state.matchAll.get(filter.Alias) ? "all" : "any";
  }

  function toggleMatchMode(filter: OverviewFilterModel) {
    state.matchAll.set(filter.Alias, !state.matchAll.get(filter.Alias));
    state.page = 1;
    syncUrl();
  }

  function selectFilter(filter: OverviewFilterModel, value: string) {
    const items = state.selectedFilters.get(filter) ?? [];
    const i = items.indexOf(value);
    i >= 0 ? items.splice(i, 1) : items.push(value);
    state.selectedFilters.set(filter, items);
    state.page = 1;
    syncUrl();
  }

  function setFilter(filter: OverviewFilterModel, value: string) {
    state.selectedFilters.set(filter, [value]);
    state.page = 1;
    syncUrl();
  }

  function isSelectedFilter(filter: OverviewFilterModel, value: string) {
    return (state.selectedFilters.get(filter) ?? []).includes(value);
  }

  function getFilterValue(filter: OverviewFilterModel) {
    return state.selectedFilters.get(filter)?.[0];
  }

  function setPage(page: number) {
    state.page = page;
    syncUrl();
  }

  function setSearch(value: string) {
    state.search = value;
    state.page = 1;
    syncUrl();
  }

  function clearFilter(filter: OverviewFilterModel) {
    state.selectedFilters.set(filter, []);
    state.matchAll.delete(filter.Alias);
    state.page = 1;
    syncUrl();
  }

  function clearAllFilters() {
    state.selectedFilters = new Map();
    state.matchAll = new Map();
    state.page = 1;
    syncUrl();
  }

  const activeFilterCount = computed(() => {
    let count = 0;
    state.selectedFilters.forEach((values, filter) => {
      // Ignore a default-enabled filter that is still in its default state.
      if (
        filter.DefaultEnabled &&
        values.length === 1 &&
        values[0] === "true"
      ) {
        return;
      }
      count += values.length;
    });
    return count;
  });

  function setSort(value: string) {
    state.sortBy = value;
    syncUrl();
  }

  function setViewMode(mode: string) {
    state.viewMode = mode;
    syncUrl();
  }

  const queryState = computed<OverviewQueryState>(() => ({
    query: state.search,
    selectedFilters: state.selectedFilters,
    page: state.page,
    sortBy: state.sortBy || undefined,
  }));

  return {
    state,
    queryState,
    activeFilterCount,
    selectFilter,
    setFilter,
    clearFilter,
    clearAllFilters,
    getMatchMode,
    toggleMatchMode,
    isSelectedFilter,
    getFilterValue,
    setPage,
    setSearch,
    setSort,
    setViewMode,
  };
}
