/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
import type { IApiMediaWithCropsModel } from './IApiMediaWithCropsModel';
import type { RichTextModel } from './RichTextModel';
export type SetPropertiesModel = {
    setCode?: string | null;
    displayName?: string | null;
    displayImage?: Array<IApiMediaWithCropsModel> | null;
    categoryName?: string | null;
    extraInformation?: Array<string> | null;
    hasBeenReleased?: boolean | null;
    mainVariantType?: Array<IApiContentModel> | null;
    tcgPlayerCategory?: number | null;
    nonLegalDeckTypes?: Array<IApiContentModel> | null;
    releaseDate?: string | null;
    cards?: Array<IApiContentModel> | null;
    subheading?: RichTextModel;
    description?: RichTextModel;
    faqBlocks?: ApiBlockListModel;
};

