import type NavigationItem from "./NavigationItemModel";

export default interface NavigationModel {
    textColorIsWhite: boolean;
    createDeckMode: boolean;

    loginPageUrl?: string;
    navigationLogoUrl: string;

    accountItems: NavigationItem[];
    items: NavigationItem[];
}