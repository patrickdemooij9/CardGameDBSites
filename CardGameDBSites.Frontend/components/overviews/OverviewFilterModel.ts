export interface OverviewFilterModel {
    Alias: string;
    DisplayName: string;
    IsInline: boolean;

    Items: OverviewFilterItemModel[];
    AutoFillValues: boolean;
}

export interface OverviewFilterItemModel {
    DisplayName: string;
    Value: string;
    IconUrl?: string;
}