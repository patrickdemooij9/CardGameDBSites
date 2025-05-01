export default interface OverviewRefreshModel {
    Query: string;
    SelectedFilters: Record<string, string[]>;

    LoadedCallback?: () => void;
}