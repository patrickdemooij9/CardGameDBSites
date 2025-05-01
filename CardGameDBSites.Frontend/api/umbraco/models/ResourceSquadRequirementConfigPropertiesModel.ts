/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
import type { SquadRequirementConfigPropertiesModel } from './SquadRequirementConfigPropertiesModel';
export type ResourceSquadRequirementConfigPropertiesModel = (SquadRequirementConfigPropertiesModel & {
    ability?: Array<IApiContentModel> | null;
    mainCardConditions?: ApiBlockListModel;
    requireAllResources?: boolean | null;
});

