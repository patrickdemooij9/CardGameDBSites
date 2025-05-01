/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { ApiLinkModel } from './ApiLinkModel';
import type { IApiMediaWithCropsModel } from './IApiMediaWithCropsModel';
import type { RichTextModel } from './RichTextModel';
export type SiteSettingsPropertiesModel = {
    siteId?: number | null;
    mainColor?: string | null;
    hoverMainColor?: string | null;
    faviconFolder?: Array<IApiMediaWithCropsModel> | null;
    faviconColor?: string | null;
    forgotPasswordEmail?: string | null;
    forgotPasswordContent?: RichTextModel;
    cardImageRoot?: Array<IApiMediaWithCropsModel> | null;
    siteName?: string | null;
    navigation?: ApiBlockListModel;
    showLogin?: boolean | null;
    navigationLogo?: Array<IApiMediaWithCropsModel> | null;
    textColorWhite?: boolean | null;
    borderColor?: string | null;
    keywordImages?: ApiBlockListModel;
    footerText?: string | null;
    footerLinks?: Array<ApiLinkModel> | null;
    defaultCreatorName?: string | null;
    defaultCreatorImage?: Array<IApiMediaWithCropsModel> | null;
    sortOptions?: ApiBlockListModel;
    allowPricingSync?: boolean | null;
    allowRedditIntegration?: boolean | null;
    redditUsername?: string | null;
    redditPassword?: string | null;
    redditClientID?: string | null;
    redditClientSecret?: string | null;
    redditSubreddit?: string | null;
};

