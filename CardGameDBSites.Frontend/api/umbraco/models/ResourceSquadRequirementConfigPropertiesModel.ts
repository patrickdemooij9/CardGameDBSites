/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
import type { SquadRequirementConfigPropertiesModel } from './SquadRequirementConfigPropertiesModel';
export type ResourceSquadRequirementConfigPropertiesModel = (SquadRequirementConfigPropertiesModel & {
    mainAbility?: Array<IApiContentModel> | null;
    mainAbilityMaxSize?: number | null;
    ability?: Array<IApiContentModel> | null;
    mainCardConditions?: ApiBlockListModel;
    singleResourceMode?: boolean | null;
    possibleValues?: Array<string> | null;
});

