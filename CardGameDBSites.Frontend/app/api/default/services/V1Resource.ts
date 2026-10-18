/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MarkdownPreviewPostModel } from '../models/MarkdownPreviewPostModel';
import type { ProxyExportRequest } from '../models/ProxyExportRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class V1Resource {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiForms({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/forms/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiFormsPagesValidate({
        id,
        pageIndex,
    }: {
        id: string,
        pageIndex: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/forms/{id}/pages/{pageIndex}/validate',
            path: {
                'id': id,
                'pageIndex': pageIndex,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getUmbracoApiExportExport({
        deckId,
        exportId,
    }: {
        deckId?: number,
        exportId?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/api/export/Export',
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
    public static getUmbracoApiExportExportForceTable({
        deckId,
    }: {
        deckId?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/umbraco/api/export/ExportForceTable',
            query: {
                'deckId': deckId,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postUmbracoApiExportProxyExport({
        requestBody,
    }: {
        requestBody?: ProxyExportRequest,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/api/export/ProxyExport',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns string OK
     * @throws ApiError
     */
    public static postUmbracoApiMarkdownPreview({
        requestBody,
    }: {
        requestBody?: MarkdownPreviewPostModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/umbraco/api/markdown/Preview',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
