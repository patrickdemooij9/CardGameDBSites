/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ContentCompositionPropertiesModel } from './ContentCompositionPropertiesModel';
import type { IApiMediaWithCropsModel } from './IApiMediaWithCropsModel';
export type BlogDetailPropertiesModel = (ContentCompositionPropertiesModel & {
    image?: Array<IApiMediaWithCropsModel> | null;
    title?: string | null;
    description?: string | null;
    author?: string | null;
    publishDate?: string | null;
});

