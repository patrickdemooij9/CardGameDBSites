import type { OverviewFilterModel } from "~/components/overviews/OverviewFilterModel";
import type { OverviewSortModel } from "~/components/overviews/OverviewSortModel";

export interface OverviewQueryState {
  query: string;
  selectedFilters: Map<OverviewFilterModel, string[]>;
  page: number;
  sortBy?: string;
}

export interface OverviewUrlOptions {
  basePath: string;
  initialFilters?: Record<string, string[]>;
  resolveLandingPath?: (alias: string, value: string) => string | undefined;
  onLandingChange?: (landingPath: string | undefined) => void;
}

export function useOverviewState(
  filters: Ref<OverviewFilterModel[]>,
  sortings?: Ref<OverviewSortModel[] | undefined>,
  views?: Ref<string[] | undefined>,
  options?: { enableQueryStringSync?: boolean; urlOptions?: OverviewUrlOptions },
) {
  const route = useRoute();
  const basePath = options?.urlOptions?.basePath ?? route.path;

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
      const values = route.query[filter.Alias] ?? options?.urlOptions?.initialFilters?.[filter.Alias];
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

  watch(
    () => filters.value,
    () => initFromRoute(),
  );

  function isDefaultFilterState(filter: OverviewFilterModel, values: string[]) {
    return filter.DefaultEnabled && values.length === 1 && values[0] === "true";
  }

  const activeLanding = computed(() => {
    const resolve = options?.urlOptions?.resolveLandingPath;
    if (!resolve) return undefined;
    const active = [...state.selectedFilters].filter(
      ([filter, values]) => values.length > 0 && !isDefaultFilterState(filter, values),
    );
    if (active.length !== 1) return undefined;
    const [filter, values] = active[0]!;
    if (values.length !== 1) return undefined;
    const path = resolve(filter.Alias, values[0]!);
    return path ? { path, filter } : undefined;
  });

  function buildUrl(page = state.page) {
    const landing = activeLanding.value;
    const params = new URLSearchParams();
    if (page !== 1) params.append("page", String(page));
    if (state.search) params.append("search", state.search);
    if (state.sortBy && state.sortBy != defaultSortBy) params.append("sortBy", state.sortBy);
    if (state.viewMode && state.viewMode != defaultViewMode) params.append("view", state.viewMode);
    state.selectedFilters.forEach((values, filter) => {
      if (filter === landing?.filter) return;
      values.forEach((v) => v && params.append(filter.Alias, v));
      if (state.matchAll.get(filter.Alias) && values.length > 0) {
        params.append("matchAll", filter.Alias);
      }
    });
    const query = params.toString();
    return `${landing?.path ?? basePath}${query ? `?${query}` : ""}`;
  }

  function syncUrl() {
    if (!import.meta.client || !options?.enableQueryStringSync) return;
    history.replaceState(history.state, "", buildUrl());
    options.urlOptions?.onLandingChange?.(activeLanding.value?.path);
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
      if (isDefaultFilterState(filter, values)) {
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
    activeLanding,
    buildUrl,
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
