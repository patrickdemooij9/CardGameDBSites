export default interface OverviewRefreshModel {
    Query: string;
    SelectedFilters: Record<string, string[]>;
    PageNumber: number;

    LoadedCallback?: () => void;
}