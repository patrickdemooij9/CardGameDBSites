/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
export type SquadSlotConfigPropertiesModel = {
    label?: string | null;
    requirements?: ApiBlockListModel;
    minCards?: number | null;
    maxCards?: ApiBlockListModel;
    displaySize?: string | null;
    disableRemoval?: boolean | null;
    numberMode?: boolean | null;
    showIfTargetSlotIsFilled?: number | null;
    additionalRequirementFilters?: ApiBlockListModel;
    groupings?: ApiBlockListModel;
    defaultValue?: Array<IApiContentModel> | null;
};

