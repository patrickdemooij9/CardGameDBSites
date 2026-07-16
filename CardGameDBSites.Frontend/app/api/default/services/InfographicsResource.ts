/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class InfographicsResource {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInfographicsFact({
        key,
        slide = 1,
    }: {
        key: string,
        slide?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/infographics/fact/{key}',
            path: {
                'key': key,
            },
            query: {
                'slide': slide,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInfographicsFacts(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/infographics/facts',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInfographicsTournament({
        id,
        slide = 1,
    }: {
        id: number,
        slide?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/infographics/tournament/{id}',
            path: {
                'id': id,
            },
            query: {
                'slide': slide,
            },
        });
    }
}
