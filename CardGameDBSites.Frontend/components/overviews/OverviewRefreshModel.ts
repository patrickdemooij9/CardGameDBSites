import type { OverviewFilterModel } from "./OverviewFilterModel";

export default interface OverviewRefreshModel {
    Query: string;
    SelectedFilters: Map<OverviewFilterModel, string[]>;
    PageNumber: number;

    LoadedCallback?: () => void;
}