/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ProxyExportRequest } from '../models/ProxyExportRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ExportResource {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiExport({
        requestBody,
    }: {
        requestBody?: ProxyExportRequest,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/export',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiExportExport({
        deckId,
        exportId,
    }: {
        deckId?: number,
        exportId?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/export/export',
            query: {
                'deckId': deckId,
                'exportId': exportId,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiExportExportForceTable({
        deckId,
    }: {
        deckId?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/export/exportForceTable',
            query: {
                'deckId': deckId,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiExportProxyExport({
        requestBody,
    }: {
        requestBody?: ProxyExportRequest,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/export/proxyExport',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
