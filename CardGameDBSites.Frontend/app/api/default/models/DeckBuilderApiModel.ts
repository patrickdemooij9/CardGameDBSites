/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckBuilderGroupApiModel } from './DeckBuilderGroupApiModel';
import type { RequirementApiModel } from './RequirementApiModel';
export type DeckBuilderApiModel = {
    id: number;
    defaultNames?: Array<string>;
    overwriteAmount?: number | null;
    requirements?: Array<RequirementApiModel>;
    groups?: Array<DeckBuilderGroupApiModel>;
    sideboardGroup?: DeckBuilderGroupApiModel;
};

