/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { SquadRequirementConfigPropertiesModel } from './SquadRequirementConfigPropertiesModel';
export type ComputedSquadRequirementPropertiesModel = (SquadRequirementConfigPropertiesModel & {
    firstAbilityRequirement?: ApiBlockListModel;
    firstAbilityCompute?: string | null;
    firstAbilityValue?: string | null;
    comparison?: string | null;
    secondAbility?: ApiBlockListModel;
    secondAbilityCompute?: string | null;
    secondAbilityValue?: string | null;
});

