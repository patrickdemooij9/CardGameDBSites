/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
import type { IntroductionCompositionPropertiesModel } from './IntroductionCompositionPropertiesModel';
export type SetOverviewPropertiesModel = (IntroductionCompositionPropertiesModel & {
    setFilters?: ApiBlockListModel;
    setSortings?: ApiBlockListModel;
    setPropertiesToShow?: Array<IApiContentModel> | null;
});

