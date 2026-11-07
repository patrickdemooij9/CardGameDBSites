import type NavigationItem from "./NavigationItemModel";

export default interface NavigationModel {
    textColorIsWhite: boolean;
    createDeckMode: boolean;

    siteName: string;
    loginPageUrl?: string;
    navigationLogoUrl: string;

    accountItems: NavigationItem[];
    items: NavigationItem[];
}