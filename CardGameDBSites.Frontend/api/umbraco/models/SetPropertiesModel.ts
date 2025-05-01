/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IApiContentModel } from './IApiContentModel';
import type { IApiMediaWithCropsModel } from './IApiMediaWithCropsModel';
export type SetPropertiesModel = {
    setCode?: string | null;
    displayName?: string | null;
    displayImage?: Array<IApiMediaWithCropsModel> | null;
    extraInformation?: Array<string> | null;
    hasBeenReleased?: boolean | null;
    mainVariantType?: Array<IApiContentModel> | null;
    tcgPlayerCategory?: number | null;
    cards?: Array<IApiContentModel> | null;
};

