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
    viewMode: route.query.view?.toString() ?? defaultViewMode,
  });

  function initFromRoute() {
    state.selectedFilters = new Map();
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
    });
  }
  initFromRoute();

  watch(
    filters,
    (n, o) => {
      if (JSON.stringify(n) !== JSON.stringify(o)) initFromRoute();
    },
    { deep: true },
  );

  function syncUrl() {
    if (!import.meta.client || !options?.enableQueryStringSync) return;
    const url = new URL(window.location.href.split("?")[0]!);
    if (state.page !== 1) url.searchParams.append("page", String(state.page));
    if (state.search) url.searchParams.append("search", state.search);
    if (state.sortBy && state.sortBy != defaultSortBy) url.searchParams.append("sortBy", state.sortBy);
    if (state.viewMode && state.viewMode != defaultViewMode) url.searchParams.append("view", state.viewMode);
    state.selectedFilters.forEach((values, filter) =>
      values.forEach((v) => v && url.searchParams.append(filter.Alias, v)),
    );
    history.replaceState(history.state, "", url);
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
    selectFilter,
    setFilter,
    isSelectedFilter,
    getFilterValue,
    setPage,
    setSearch,
    setSort,
    setViewMode,
  };
}
