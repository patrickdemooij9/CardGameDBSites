import type NavigationItem from "./NavigationItemModel";

export default interface NavigationModel {
    textColorIsWhite: boolean;
    createDeckMode: boolean;

    navigationLogoUrl: string;

    items: NavigationItem[];
}