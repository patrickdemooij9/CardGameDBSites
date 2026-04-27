/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
import type { RichTextModel } from './RichTextModel';
export type CardOverviewPropertiesModel = {
    title?: string | null;
    description?: RichTextModel;
    showAdBanner?: boolean | null;
    attributesToShow?: Array<IApiContentModel> | null;
    filters?: ApiBlockListModel;
    hideFilters?: boolean | null;
    internalFilters?: ApiBlockListModel;
    sortings?: ApiBlockListModel;
    pageSize?: number | null;
};

