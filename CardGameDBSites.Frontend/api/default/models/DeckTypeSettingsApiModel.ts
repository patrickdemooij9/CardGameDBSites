/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckActionApiModel } from './DeckActionApiModel';
import type { DeckCardGroupApiModel } from './DeckCardGroupApiModel';
import type { DeckCardImageRuleApiModel } from './DeckCardImageRuleApiModel';
import type { RequirementApiModel } from './RequirementApiModel';
export type DeckTypeSettingsApiModel = {
    overviewUrl: string;
    displayName: string;
    amountOfSquadCards?: number;
    groupings?: Array<DeckCardGroupApiModel>;
    actions?: Array<DeckActionApiModel>;
    imageRules?: Array<DeckCardImageRuleApiModel>;
    mainCardRequirements?: Array<RequirementApiModel>;
};

