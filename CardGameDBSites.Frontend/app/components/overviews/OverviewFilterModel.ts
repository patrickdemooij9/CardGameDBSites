import type { CardsQueryFilterClauseApiModel } from "~/api/default";

export enum OverviewFilterType {
    INLINE = "INLINE",

    CHECKBOX = "CHECKBOX",
    DROPDOWN = "DROPDOWN",
    DATE = "DATE",
    TEXT_INPUT = "TEXT_INPUT",
}

export interface OverviewFilterModel {
    Alias: string;
    DisplayName: string;
    Type: OverviewFilterType;

    Items: OverviewFilterItemModel[];
    AutoFillValues: boolean;
    DefaultEnabled?: boolean;

    /** Transient UI flag: true while AutoFillValues options are being fetched. */
    Loading?: boolean;

    ToFiltersHandler?: (values: string[]) => CardsQueryFilterClauseApiModel[];
}

export interface OverviewFilterItemModel {
    DisplayName: string;
    Value: string;
    IconUrl?: string;
}