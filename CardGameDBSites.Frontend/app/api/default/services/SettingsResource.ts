/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckBuilderApiModel } from '../models/DeckBuilderApiModel';
import type { DeckTypeSettingsApiModel } from '../models/DeckTypeSettingsApiModel';
import type { SetOverviewSettingsApiModel } from '../models/SetOverviewSettingsApiModel';
import type { SiteSettingsApiModel } from '../models/SiteSettingsApiModel';
import type { SquadSettingsOptionApiModel } from '../models/SquadSettingsOptionApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class SettingsResource {
    /**
     * @returns DeckBuilderApiModel OK
     * @throws ApiError
     */
    public static getApiSettingsDeckBuilder({
        typeId,
    }: {
        typeId?: number,
    }): CancelablePromise<DeckBuilderApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/settings/deckBuilder',
            query: {
                'typeId': typeId,
            },
        });
    }
    /**
     * @returns DeckTypeSettingsApiModel OK
     * @throws ApiError
     */
    public static getApiSettingsDeckType({
        typeId,
    }: {
        typeId?: number,
    }): CancelablePromise<DeckTypeSettingsApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/settings/deckType',
            query: {
                'typeId': typeId,
            },
        });
    }
    /**
     * @returns SetOverviewSettingsApiModel OK
     * @throws ApiError
     */
    public static getApiSettingsSetOverview(): CancelablePromise<SetOverviewSettingsApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/settings/setOverview',
        });
    }
    /**
     * @returns SiteSettingsApiModel OK
     * @throws ApiError
     */
    public static getApiSettingsSite(): CancelablePromise<SiteSettingsApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/settings/site',
        });
    }
    /**
     * @returns SquadSettingsOptionApiModel OK
     * @throws ApiError
     */
    public static getApiSettingsSquadSettingsOptions(): CancelablePromise<Array<SquadSettingsOptionApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/settings/squadSettingsOptions',
        });
    }
}
