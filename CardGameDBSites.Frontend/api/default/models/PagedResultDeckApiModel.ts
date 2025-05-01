/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckApiModel } from './DeckApiModel';
export type PagedResultDeckApiModel = {
    pageNumber?: number;
    pageSize?: number;
    readonly totalPages?: number;
    totalItems?: number;
    items?: Array<DeckApiModel> | null;
};

