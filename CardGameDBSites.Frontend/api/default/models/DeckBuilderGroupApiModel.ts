/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckBuilderSlotApiModel } from './DeckBuilderSlotApiModel';
import type { RequirementApiModel } from './RequirementApiModel';
export type DeckBuilderGroupApiModel = {
    id: number;
    name?: string | null;
    requirements?: Array<RequirementApiModel>;
    slots?: Array<DeckBuilderSlotApiModel>;
};

