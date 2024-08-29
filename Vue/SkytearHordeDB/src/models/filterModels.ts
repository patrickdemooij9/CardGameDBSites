export interface Config{
    changeUrl: boolean;
}

export interface FilterItem{
    value: string;
    option: ItemOption;
    element: HTMLInputElement;
}

export interface Filter{
    key: string;
    filterItems: FilterItem[];
}

export interface CardValue{
    type: string,
    value: string,
    values: string[]
}

export enum ItemOption{
    None,
    Include,
    Exclude
}