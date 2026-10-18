/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AddCollectionCardsApiModel } from '../models/AddCollectionCardsApiModel';
import type { CollectionCardApiModel } from '../models/CollectionCardApiModel';
import type { CollectionExportType } from '../models/CollectionExportType';
import type { CollectionSettingsApiModel } from '../models/CollectionSettingsApiModel';
import type { CollectionSummaryApiModel } from '../models/CollectionSummaryApiModel';
import type { DeckProgressApiModel } from '../models/DeckProgressApiModel';
import type { EmptyCreatedAtActionResultModel } from '../models/EmptyCreatedAtActionResultModel';
import type { MaintenanceResultModel } from '../models/MaintenanceResultModel';
import type { PackPostApiModel } from '../models/PackPostApiModel';
import type { PackVerifySuccessApiModel } from '../models/PackVerifySuccessApiModel';
import type { PresetApiModel } from '../models/PresetApiModel';
import type { PublishedContentNotFoundResultModel } from '../models/PublishedContentNotFoundResultModel';
import type { RedirectToUmbracoPageResultModel } from '../models/RedirectToUmbracoPageResultModel';
import type { RedirectToUmbracoUrlResultModel } from '../models/RedirectToUmbracoUrlResultModel';
import type { SetProgressApiModel } from '../models/SetProgressApiModel';
import type { UmbracoPageResultModel } from '../models/UmbracoPageResultModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class CollectionResource {
    /**
     * @returns CollectionCardApiModel OK
     * @throws ApiError
     */
    public static postApiCollectionAddCards({
        cardId,
        requestBody,
    }: {
        cardId?: number,
        requestBody?: Record<string, number>,
    }): CancelablePromise<Array<CollectionCardApiModel>> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/addCards',
            query: {
                'cardId': cardId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns CollectionCardApiModel OK
     * @throws ApiError
     */
    public static postApiCollectionAddCardsBatch({
        requestBody,
    }: {
        requestBody?: Array<AddCollectionCardsApiModel>,
    }): CancelablePromise<Array<CollectionCardApiModel>> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/addCardsBatch',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCollectionAddPack({
        requestBody,
    }: {
        requestBody?: PackPostApiModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/addPack',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCollectionAddPreset({
        presetId,
    }: {
        presetId?: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/addPreset',
            query: {
                'presetId': presetId,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCollectionAddSet({
        setId,
    }: {
        setId?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/addSet',
            query: {
                'setId': setId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }
    /**
     * @returns CollectionCardApiModel OK
     * @throws ApiError
     */
    public static postApiCollectionCards({
        requestBody,
    }: {
        requestBody?: Array<number>,
    }): CancelablePromise<Array<CollectionCardApiModel>> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/cards',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns DeckProgressApiModel OK
     * @throws ApiError
     */
    public static postApiCollectionDecksProgress({
        requestBody,
    }: {
        requestBody?: Array<number>,
    }): CancelablePromise<Array<DeckProgressApiModel>> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/decksProgress',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns binary OK
     * @throws ApiError
     */
    public static getApiCollectionExport({
        exportType,
    }: {
        exportType?: CollectionExportType,
    }): CancelablePromise<Blob> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/collection/export',
            query: {
                'exportType': exportType,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCollectionImport({
        overwrite,
    }: {
        overwrite?: boolean,
    }): CancelablePromise<(EmptyCreatedAtActionResultModel | MaintenanceResultModel | PublishedContentNotFoundResultModel | RedirectToUmbracoPageResultModel | RedirectToUmbracoUrlResultModel | UmbracoPageResultModel)> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/import',
            query: {
                'overwrite': overwrite,
            },
        });
    }
    /**
     * @returns PresetApiModel OK
     * @throws ApiError
     */
    public static getApiCollectionPresets(): CancelablePromise<Array<PresetApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/collection/presets',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiCollectionRemoveSet({
        setId,
    }: {
        setId?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/removeSet',
            query: {
                'setId': setId,
            },
        });
    }
    /**
     * @returns number OK
     * @throws ApiError
     */
    public static getApiCollectionSets(): CancelablePromise<Array<number>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/collection/sets',
        });
    }
    /**
     * @returns SetProgressApiModel OK
     * @throws ApiError
     */
    public static getApiCollectionSetsProgress(): CancelablePromise<Array<SetProgressApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/collection/setsProgress',
        });
    }
    /**
     * @returns CollectionSettingsApiModel OK
     * @throws ApiError
     */
    public static getApiCollectionSettings(): CancelablePromise<CollectionSettingsApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/collection/settings',
        });
    }
    /**
     * @returns CollectionSummaryApiModel OK
     * @throws ApiError
     */
    public static getApiCollectionSummary(): CancelablePromise<CollectionSummaryApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/collection/summary',
        });
    }
    /**
     * @returns PackVerifySuccessApiModel OK
     * @throws ApiError
     */
    public static postApiCollectionVerifyPack({
        requestBody,
    }: {
        requestBody?: PackPostApiModel,
    }): CancelablePromise<PackVerifySuccessApiModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/collection/verifyPack',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }
}
