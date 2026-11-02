/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApproveRequest } from '../models/ApproveRequest';
import type { KeywordBackfillResult } from '../models/KeywordBackfillResult';
import type { ManualSubmitRequest } from '../models/ManualSubmitRequest';
import type { PriceSyncStatusApiModel } from '../models/PriceSyncStatusApiModel';
import type { PricingCardApiModel } from '../models/PricingCardApiModel';
import type { PricingOverviewApiModel } from '../models/PricingOverviewApiModel';
import type { SetTcgPlayerIdRequest } from '../models/SetTcgPlayerIdRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AdminResource {
    /**
     * @returns KeywordBackfillResult OK
     * @throws ApiError
     */
    public static postApiAdminKeywordsBackfill({
        dryRun = true,
    }: {
        dryRun?: boolean,
    }): CancelablePromise<KeywordBackfillResult> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/admin/keywords/backfill',
            query: {
                'dryRun': dryRun,
            },
        });
    }
    /**
     * @returns PricingOverviewApiModel OK
     * @throws ApiError
     */
    public static getApiAdminPricingOverview(): CancelablePromise<PricingOverviewApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/admin/pricing/overview',
        });
    }
    /**
     * @returns PriceSyncStatusApiModel OK
     * @throws ApiError
     */
    public static getApiAdminPricingSync(): CancelablePromise<PriceSyncStatusApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/admin/pricing/sync',
        });
    }
    /**
     * @returns PriceSyncStatusApiModel OK
     * @throws ApiError
     */
    public static postApiAdminPricingSync(): CancelablePromise<PriceSyncStatusApiModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/admin/pricing/sync',
            errors: {
                409: `Conflict`,
            },
        });
    }
    /**
     * @returns PricingCardApiModel OK
     * @throws ApiError
     */
    public static postApiAdminPricingTcgplayerid({
        requestBody,
    }: {
        requestBody?: SetTcgPlayerIdRequest,
    }): CancelablePromise<PricingCardApiModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/admin/pricing/tcgplayerid',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
                409: `Conflict`,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCardimportqueueApprove({
        id,
        setId,
        requestBody,
    }: {
        id?: number,
        setId?: number,
        requestBody?: ApproveRequest,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/cardimportqueue/approve',
            query: {
                'id': id,
                'setId': setId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiCardimportqueueGetbackimage({
        id,
    }: {
        id?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cardimportqueue/getbackimage',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiCardimportqueueGetimage({
        id,
    }: {
        id?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cardimportqueue/getimage',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiCardimportqueuePending(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cardimportqueue/pending',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiCardimportqueuePresets(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/cardimportqueue/presets',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCardimportqueueReject({
        id,
    }: {
        id?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/cardimportqueue/reject',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCardimportqueueRematch({
        id,
    }: {
        id?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/cardimportqueue/rematch',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCardimportqueueSubmitmanual({
        requestBody,
    }: {
        requestBody?: ManualSubmitRequest,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/cardimportqueue/submitmanual',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCardimportqueueUpdatedata({
        id,
        requestBody,
    }: {
        id?: number,
        requestBody?: Record<string, string>,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/cardimportqueue/updatedata',
            query: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
