/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CardDetailApiModel } from '../models/CardDetailApiModel';
import type { CardsQueryPostApiModel } from '../models/CardsQueryPostApiModel';
import type { CreateSquadPostModel } from '../models/CreateSquadPostModel';
import type { CurrentMemberApiModel } from '../models/CurrentMemberApiModel';
import type { DeckApiModel } from '../models/DeckApiModel';
import type { DeckBuilderApiModel } from '../models/DeckBuilderApiModel';
import type { DeckQueryPostModel } from '../models/DeckQueryPostModel';
import type { DeckTypeSettingsApiModel } from '../models/DeckTypeSettingsApiModel';
import type { LoginPostModel } from '../models/LoginPostModel';
import type { MemberApiModel } from '../models/MemberApiModel';
import type { PagedResultCardDetailApiModel } from '../models/PagedResultCardDetailApiModel';
import type { PagedResultDeckApiModel } from '../models/PagedResultDeckApiModel';
import type { RegisterPostModel } from '../models/RegisterPostModel';
import type { SiteSettingsApiModel } from '../models/SiteSettingsApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class V1Resource {
    /**
     * @returns CurrentMemberApiModel OK
     * @throws ApiError
     */
    public static getApiAccountGetCurrentMember(): CancelablePromise<CurrentMemberApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/account/GetCurrentMember',
        });
    }
    /**
     * @returns CurrentMemberApiModel OK
     * @throws ApiError
     */
    public static postApiAccountLogin({
        requestBody,
    }: {
        requestBody?: LoginPostModel,
    }): CancelablePromise<CurrentMemberApiModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/account/Login',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns CurrentMemberApiModel OK
     * @throws ApiError
     */
    public static postApiAccountRegister({
        requestBody,
    }: {
        requestBody?: RegisterPostModel,
    }): CancelablePromise<CurrentMemberApiModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/account/register',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
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
     * @returns number OK
     * @throws ApiError
     */
    public static postApiDeckbuilderSubmit({
        requestBody,
    }: {
        requestBody?: CreateSquadPostModel,
    }): CancelablePromise<number> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/deckbuilder/submit',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns DeckApiModel OK
     * @throws ApiError
     */
    public static getApiDecksGet({
        id,
    }: {
        id?: number,
    }): CancelablePromise<DeckApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/decks/get',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns PagedResultDeckApiModel OK
     * @throws ApiError
     */
    public static postApiDecksQuery({
        requestBody,
    }: {
        requestBody?: DeckQueryPostModel,
    }): CancelablePromise<PagedResultDeckApiModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/decks/query',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns MemberApiModel OK
     * @throws ApiError
     */
    public static getApiMemberById({
        memberId,
    }: {
        memberId?: number,
    }): CancelablePromise<MemberApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/member/byId',
            query: {
                'memberId': memberId,
            },
        });
    }
    /**
     * @returns MemberApiModel OK
     * @throws ApiError
     */
    public static postApiMemberByIds({
        requestBody,
    }: {
        requestBody?: Array<number>,
    }): CancelablePromise<Array<MemberApiModel>> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/member/byIds',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
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
     * @returns SiteSettingsApiModel OK
     * @throws ApiError
     */
    public static getApiSettingsSite(): CancelablePromise<SiteSettingsApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/settings/site',
        });
    }
}
