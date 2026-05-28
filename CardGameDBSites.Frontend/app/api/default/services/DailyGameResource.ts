/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DailyGameBootstrapApiModel } from '../models/DailyGameBootstrapApiModel';
import type { DailyGameGuessPostApiModel } from '../models/DailyGameGuessPostApiModel';
import type { DailyGameGuessResultApiModel } from '../models/DailyGameGuessResultApiModel';
import type { DailyGameLeaderboardEntryApiModel } from '../models/DailyGameLeaderboardEntryApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class DailyGameResource {
    /**
     * @returns DailyGameBootstrapApiModel OK
     * @throws ApiError
     */
    public static getApiDailygameBootstrap({
        guestSessionToken,
    }: {
        guestSessionToken?: string,
    }): CancelablePromise<DailyGameBootstrapApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/dailygame/bootstrap',
            query: {
                'guestSessionToken': guestSessionToken,
            },
        });
    }
    /**
     * @returns DailyGameGuessResultApiModel OK
     * @throws ApiError
     */
    public static postApiDailygameGuess({
        requestBody,
    }: {
        requestBody?: DailyGameGuessPostApiModel,
    }): CancelablePromise<DailyGameGuessResultApiModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/dailygame/guess',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns DailyGameLeaderboardEntryApiModel OK
     * @throws ApiError
     */
    public static getApiDailygameLeaderboard({
        take = 50,
    }: {
        take?: number,
    }): CancelablePromise<Array<DailyGameLeaderboardEntryApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/dailygame/leaderboard',
            query: {
                'take': take,
            },
        });
    }
}
