/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckBuilderGroupApiModel } from './DeckBuilderGroupApiModel';
export type DeckBuilderApiModel = {
    id: number;
    defaultNames?: Array<string>;
    overwriteAmount?: number | null;
    groups?: Array<DeckBuilderGroupApiModel>;
};
