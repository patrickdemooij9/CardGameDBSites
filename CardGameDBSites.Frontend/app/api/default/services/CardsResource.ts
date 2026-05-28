/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CardDetailApiModel } from '../models/CardDetailApiModel';
import type { CardPriceChangeApiModel } from '../models/CardPriceChangeApiModel';
import type { CardPriceHistoryItemApiModel } from '../models/CardPriceHistoryItemApiModel';
import type { CardsQueryPostApiModel } from '../models/CardsQueryPostApiModel';
import type { CardVariantTypeApiModel } from '../models/CardVariantTypeApiModel';
import type { PagedResultCardDetailApiModel } from '../models/PagedResultCardDetailApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class CardsResource {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiCardsAll({
        skip,
        take,
    }: {
        skip?: number,
        take?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cards/all',
            query: {
                'skip': skip,
                'take': take,
            },
        });
    }
    /**
     * @returns CardDetailApiModel OK
     * @throws ApiError
     */
    public static getApiCardsById({
        id,
    }: {
        id?: string,
    }): CancelablePromise<CardDetailApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cards/byId',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns CardDetailApiModel OK
     * @throws ApiError
     */
    public static postApiCardsByIds({
        requestBody,
    }: {
        requestBody?: Array<number>,
    }): CancelablePromise<Array<CardDetailApiModel>> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/cards/byIds',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns string OK
     * @throws ApiError
     */
    public static getApiCardsGetAllValues({
        abilityName,
    }: {
        abilityName?: string,
    }): CancelablePromise<Array<string>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cards/getAllValues',
            query: {
                'abilityName': abilityName,
            },
        });
    }
    /**
     * @returns CardPriceHistoryItemApiModel OK
     * @throws ApiError
     */
    public static getApiCardsPriceHistory({
        cardId,
        variantId,
    }: {
        cardId?: number,
        variantId?: number,
    }): CancelablePromise<Array<CardPriceHistoryItemApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cards/priceHistory',
            query: {
                'cardId': cardId,
                'variantId': variantId,
            },
        });
    }
    /**
     * @returns PagedResultCardDetailApiModel OK
     * @throws ApiError
     */
    public static postApiCardsQuery({
        requestBody,
    }: {
        requestBody?: CardsQueryPostApiModel,
    }): CancelablePromise<PagedResultCardDetailApiModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/cards/query',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns CardPriceChangeApiModel OK
     * @throws ApiError
     */
    public static getApiCardsTopPriceChanges({
        count,
        descending,
        variantTypeId,
    }: {
        count?: number,
        descending?: boolean,
        variantTypeId?: number,
    }): CancelablePromise<Array<CardPriceChangeApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cards/topPriceChanges',
            query: {
                'count': count,
                'descending': descending,
                'variantTypeId': variantTypeId,
            },
        });
    }
    /**
     * @returns CardVariantTypeApiModel OK
     * @throws ApiError
     */
    public static getApiCardsVariantTypes(): CancelablePromise<Array<CardVariantTypeApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cards/variantTypes',
        });
    }
}
