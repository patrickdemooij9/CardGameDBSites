/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
export type PriceSyncStatusApiModel = {
    isRunning: boolean;
    startedUtc?: string | null;
    finishedUtc?: string | null;
    syncedCards: number;
    error?: string | null;
    latestPriceDate?: string | null;
};

