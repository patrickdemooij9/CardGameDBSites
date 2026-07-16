/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MetaLeaderApiModel } from '../models/MetaLeaderApiModel';
import type { MetaPopularCardApiModel } from '../models/MetaPopularCardApiModel';
import type { MetaWinningDeckApiModel } from '../models/MetaWinningDeckApiModel';
import type { PeriodApiModel } from '../models/PeriodApiModel';
import type { TournamentSummaryApiModel } from '../models/TournamentSummaryApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TournamentsResource {
    /**
     * @returns MetaPopularCardApiModel OK
     * @throws ApiError
     */
    public static getApiTournamentsMetaPopularCards({
        periodId,
        take = 8,
        leaderGroupId = 1,
        leaderSlotId,
    }: {
        periodId?: number,
        take?: number,
        leaderGroupId?: number,
        leaderSlotId?: number,
    }): CancelablePromise<Array<MetaPopularCardApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tournaments/meta/popular-cards',
            query: {
                'periodId': periodId,
                'take': take,
                'leaderGroupId': leaderGroupId,
                'leaderSlotId': leaderSlotId,
            },
        });
    }
    /**
     * @returns MetaWinningDeckApiModel OK
     * @throws ApiError
     */
    public static getApiTournamentsMetaRecentWinners({
        periodId,
        count = 6,
        leaderGroupId = 1,
        leaderSlotId,
    }: {
        periodId?: number,
        count?: number,
        leaderGroupId?: number,
        leaderSlotId?: number,
    }): CancelablePromise<Array<MetaWinningDeckApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tournaments/meta/recent-winners',
            query: {
                'periodId': periodId,
                'count': count,
                'leaderGroupId': leaderGroupId,
                'leaderSlotId': leaderSlotId,
            },
        });
    }
    /**
     * @returns MetaLeaderApiModel OK
     * @throws ApiError
     */
    public static getApiTournamentsMetaTopLeaders({
        periodId,
        take = 5,
        leaderGroupId = 1,
        leaderSlotId,
        tournamentId,
    }: {
        periodId?: number,
        take?: number,
        leaderGroupId?: number,
        leaderSlotId?: number,
        tournamentId?: number,
    }): CancelablePromise<Array<MetaLeaderApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tournaments/meta/top-leaders',
            query: {
                'periodId': periodId,
                'take': take,
                'leaderGroupId': leaderGroupId,
                'leaderSlotId': leaderSlotId,
                'tournamentId': tournamentId,
            },
        });
    }
    /**
     * @returns PeriodApiModel OK
     * @throws ApiError
     */
    public static getApiTournamentsPeriods({
        formatId,
    }: {
        formatId?: number,
    }): CancelablePromise<Array<PeriodApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tournaments/periods',
            query: {
                'formatId': formatId,
            },
        });
    }
    /**
     * @returns TournamentSummaryApiModel OK
     * @throws ApiError
     */
    public static getApiTournamentsRecent({
        periodId,
        count = 6,
    }: {
        periodId?: number,
        count?: number,
    }): CancelablePromise<Array<TournamentSummaryApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tournaments/recent',
            query: {
                'periodId': periodId,
                'count': count,
            },
        });
    }
}
