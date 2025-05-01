/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckCardApiModel } from './DeckCardApiModel';
export type DeckApiModel = {
    id?: number;
    name?: string;
    description?: string | null;
    createdBy?: number | null;
    createdDate?: string;
    updatedDate?: string;
    isPublished?: boolean;
    typeId?: number;
    amountOfLikes?: number;
    score?: number;
    cards?: Array<DeckCardApiModel>;
};

