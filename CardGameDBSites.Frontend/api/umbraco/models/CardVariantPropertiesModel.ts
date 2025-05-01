/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
import type { IApiMediaWithCropsModel } from './IApiMediaWithCropsModel';
export type CardVariantPropertiesModel = {
    variantType?: Array<IApiContentModel> | null;
    displayName?: string | null;
    image?: Array<IApiMediaWithCropsModel> | null;
    backImage?: Array<IApiMediaWithCropsModel> | null;
    attributes?: ApiBlockListModel;
    set?: Array<IApiContentModel> | null;
};

