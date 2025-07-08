/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CardSectionApiModel } from './CardSectionApiModel';
import type { KeywordImageApiModel } from './KeywordImageApiModel';
import type { LinkApiModel } from './LinkApiModel';
import type { NavigationItemApiModel } from './NavigationItemApiModel';
export type SiteSettingsApiModel = {
    mainColor: string;
    hoverMainColor: string;
    borderColor: string;
    siteName: string;
    showLogin: boolean;
    loginPageUrl?: string | null;
    accountNavigation?: Array<NavigationItemApiModel>;
    navigation?: Array<NavigationItemApiModel>;
    navigationLogoUrl: string;
    textColorWhite: boolean;
    footerText: string;
    footerLinks?: Array<LinkApiModel>;
    cardSections?: Array<CardSectionApiModel>;
    keywordImages?: Array<KeywordImageApiModel>;
};

