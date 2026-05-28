/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CardSearchCollectionMode } from './CardSearchCollectionMode';
import type { CardsQueryFilterClauseApiModel } from './CardsQueryFilterClauseApiModel';
export type CardsQueryPostApiModel = {
    query?: string | null;
    pageNumber?: number;
    pageSize?: number;
    setId?: number | null;
    variantTypeIds?: Array<number>;
    filterClauses?: Array<CardsQueryFilterClauseApiModel>;
    collectionMode?: CardSearchCollectionMode;
    sortBy?: string | null;
    includeReprintedCards?: boolean;
    legalForDeckTypeId?: number | null;
};

