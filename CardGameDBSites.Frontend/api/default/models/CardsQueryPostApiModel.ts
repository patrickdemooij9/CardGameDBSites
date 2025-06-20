/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CardsQueryFilterClauseApiModel } from './CardsQueryFilterClauseApiModel';
export type CardsQueryPostApiModel = {
    query?: string | null;
    pageNumber?: number;
    pageSize?: number;
    setId?: number | null;
    variantTypeId?: number | null;
    filterClauses?: Array<CardsQueryFilterClauseApiModel>;
};

