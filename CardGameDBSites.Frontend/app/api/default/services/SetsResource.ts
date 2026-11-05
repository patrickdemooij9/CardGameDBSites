/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { SetPriceHistoryItemApiModel } from '../models/SetPriceHistoryItemApiModel';
import type { SetViewModel } from '../models/SetViewModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class SetsResource {
    /**
     * @returns SetViewModel OK
     * @throws ApiError
     */
    public static getApiSetsGet({
        id,
    }: {
        id?: number,
    }): CancelablePromise<SetViewModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/sets/get',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns SetViewModel OK
     * @throws ApiError
     */
    public static getApiSetsGetAll(): CancelablePromise<Array<SetViewModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/sets/getAll',
        });
    }
    /**
     * @returns SetViewModel OK
     * @throws ApiError
     */
    public static getApiSetsGetByIds({
        requestBody,
    }: {
        requestBody?: Array<number>,
    }): CancelablePromise<Array<SetViewModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/sets/getByIds',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns SetViewModel OK
     * @throws ApiError
     */
    public static getApiSetsGetByKey({
        key,
    }: {
        key?: string,
    }): CancelablePromise<SetViewModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/sets/getByKey',
            query: {
                'key': key,
            },
        });
    }
    /**
     * @returns SetPriceHistoryItemApiModel OK
     * @throws ApiError
     */
    public static getApiSetsPriceHistory({
        setId,
    }: {
        setId?: number,
    }): CancelablePromise<Array<SetPriceHistoryItemApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/sets/priceHistory',
            query: {
                'setId': setId,
            },
        });
    }
}
