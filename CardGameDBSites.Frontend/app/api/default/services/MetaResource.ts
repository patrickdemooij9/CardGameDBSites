/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckApiModel } from '../models/DeckApiModel';
import type { MetaCardApiModel } from '../models/MetaCardApiModel';
import type { MetaCardLinkApiModel } from '../models/MetaCardLinkApiModel';
import type { MetaCardStatApiModel } from '../models/MetaCardStatApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class MetaResource {
    /**
     * @returns MetaCardLinkApiModel OK
     * @throws ApiError
     */
    public static getApiMetaCardPageUrl({
        cardId,
    }: {
        cardId?: number,
    }): CancelablePromise<MetaCardLinkApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/meta/card-page-url',
            query: {
                'cardId': cardId,
            },
        });
    }
    /**
     * @returns MetaCardStatApiModel OK
     * @throws ApiError
     */
    public static getApiMetaCardStats({
        periodId,
        cardIds,
    }: {
        periodId?: number,
        cardIds?: Array<number>,
    }): CancelablePromise<Array<MetaCardStatApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/meta/card-stats',
            query: {
                'periodId': periodId,
                'cardIds': cardIds,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns MetaCardApiModel OK
     * @throws ApiError
     */
    public static getApiMetaResolveCard({
        path,
    }: {
        path?: string,
    }): CancelablePromise<MetaCardApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/meta/resolve-card',
            query: {
                'path': path,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns DeckApiModel OK
     * @throws ApiError
     */
    public static getApiMetaTopDecks({
        periodId,
        cardId,
        count = 6,
        leaderGroupId = 1,
        leaderSlotId,
    }: {
        periodId?: number,
        cardId?: number,
        count?: number,
        leaderGroupId?: number,
        leaderSlotId?: number,
    }): CancelablePromise<Array<DeckApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/meta/top-decks',
            query: {
                'periodId': periodId,
                'cardId': cardId,
                'count': count,
                'leaderGroupId': leaderGroupId,
                'leaderSlotId': leaderSlotId,
            },
        });
    }
}
