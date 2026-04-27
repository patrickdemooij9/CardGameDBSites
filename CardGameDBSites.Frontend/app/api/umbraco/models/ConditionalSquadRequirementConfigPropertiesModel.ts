/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { SquadRequirementConfigPropertiesModel } from './SquadRequirementConfigPropertiesModel';
export type ConditionalSquadRequirementConfigPropertiesModel = (SquadRequirementConfigPropertiesModel & {
    condition?: ApiBlockListModel;
    requirements?: ApiBlockListModel;
});

