import type { CardsQueryFilterClauseApiModel } from "~/api/default";

export enum OverviewFilterType {
    INLINE = "INLINE",

    CHECKBOX = "CHECKBOX",
    DROPDOWN = "DROPDOWN",
    DATE_RANGE = "DATE_RANGE",
}

export interface OverviewFilterModel {
    Alias: string;
    DisplayName: string;
    Type: OverviewFilterType;

    Items: OverviewFilterItemModel[];
    AutoFillValues: boolean;
    DefaultEnabled?: boolean;

    ToFiltersHandler?: (values: string[]) => CardsQueryFilterClauseApiModel[];
}

export interface OverviewFilterItemModel {
    DisplayName: string;
    Value: string;
    IconUrl?: string;
}