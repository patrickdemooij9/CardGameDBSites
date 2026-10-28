import type { CardsQueryFilterClauseApiModel, CardsQueryPostApiModel } from "~/api/default";

export enum OverviewFilterType {
    INLINE = "INLINE",

    CHECKBOX = "CHECKBOX",
    DROPDOWN = "DROPDOWN",
    DATE = "DATE",
    TEXT_INPUT = "TEXT_INPUT",
    RANGE = "RANGE",
    SINGLE_SELECT = "SINGLE_SELECT",
    KEYWORD = "KEYWORD",
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

    ToFiltersHandler?: (values: string[], matchMode: "any" | "all") => CardsQueryFilterClauseApiModel[];
    FormatValue?: (value: string) => string;
    ApplyToQuery?: (values: string[], query: CardsQueryPostApiModel) => void;
}

export interface OverviewFilterItemModel {
    DisplayName: string;
    Value: string;
    IconUrl?: string;
    Group?: string;
}