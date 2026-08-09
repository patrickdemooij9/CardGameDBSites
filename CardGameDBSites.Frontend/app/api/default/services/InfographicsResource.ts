/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FactSlideKind } from '../models/FactSlideKind';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class InfographicsResource {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInfographicsCustom({
        kind,
        heading,
        title,
        bigValue,
        bigLabel,
        caption,
        imageUrl,
        secondImageUrl,
        items,
    }: {
        kind: FactSlideKind,
        heading?: string,
        title?: string,
        bigValue?: string,
        bigLabel?: string,
        caption?: string,
        imageUrl?: string,
        secondImageUrl?: string,
        items?: Array<string>,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/infographics/custom',
            query: {
                'Kind': kind,
                'Heading': heading,
                'Title': title,
                'BigValue': bigValue,
                'BigLabel': bigLabel,
                'Caption': caption,
                'ImageUrl': imageUrl,
                'SecondImageUrl': secondImageUrl,
                'Items': items,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInfographicsFact({
        key,
        slide = 1,
        setCode,
    }: {
        key: string,
        slide?: number,
        setCode?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/infographics/fact/{key}',
            path: {
                'key': key,
            },
            query: {
                'slide': slide,
                'setCode': setCode,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInfographicsFacts({
        setCode,
    }: {
        setCode?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/infographics/facts',
            query: {
                'setCode': setCode,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInfographicsLeader({
        cardId,
        slide = 1,
        periodId,
        formatId = 1,
    }: {
        cardId: number,
        slide?: number,
        periodId?: number,
        formatId?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/infographics/leader/{cardId}',
            path: {
                'cardId': cardId,
            },
            query: {
                'slide': slide,
                'periodId': periodId,
                'formatId': formatId,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiInfographicsPriceTrends({
        slide = 1,
    }: {
        slide?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/infographics/price-trends',
            query: {
                'slide': slide,
            },
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
