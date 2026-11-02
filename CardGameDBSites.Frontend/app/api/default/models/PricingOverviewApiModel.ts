/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MissingPriceApiModel } from './MissingPriceApiModel';
import type { PriceSyncStatusApiModel } from './PriceSyncStatusApiModel';
import type { TcgPlayerIdConflictApiModel } from './TcgPlayerIdConflictApiModel';
export type PricingOverviewApiModel = {
    priceableCardCount: number;
    missingPrices: Array<MissingPriceApiModel>;
    conflicts: Array<TcgPlayerIdConflictApiModel>;
    syncStatus: PriceSyncStatusApiModel;
};

