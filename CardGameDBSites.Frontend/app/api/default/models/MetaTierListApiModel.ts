/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MetaTierLeaderApiModel } from './MetaTierLeaderApiModel';
export type MetaTierListApiModel = {
    periodId?: number;
    periodName?: string;
    firstEventUtc?: string | null;
    lastEventUtc?: string | null;
    lastUpdatedUtc?: string | null;
    totalDecks?: number;
    totalEvents?: number;
    totalEntrants?: number;
    entrantsWithDeck?: number;
    deltasAvailable?: boolean;
    deltasUnavailableReason?: string | null;
    deltaComparedToUtc?: string | null;
    deltaWeeks?: number;
    minDecks?: number;
    minEvents?: number;
    leaders?: Array<MetaTierLeaderApiModel>;
};

