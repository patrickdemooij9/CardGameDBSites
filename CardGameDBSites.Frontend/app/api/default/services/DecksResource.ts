/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckApiModel } from '../models/DeckApiModel';
import type { DeckJsonFile } from '../models/DeckJsonFile';
import type { DeckQueryPostModel } from '../models/DeckQueryPostModel';
import type { PagedResultDeckApiModel } from '../models/PagedResultDeckApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class DecksResource {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiDecksDeleteDeck({
        deckId,
    }: {
        deckId?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/decks/deleteDeck',
            query: {
                'deckId': deckId,
            },
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
     * @returns DeckJsonFile OK
     * @throws ApiError
     */
    public static getApiDecksGetAsJsonModel({
        id,
    }: {
        id?: number,
    }): CancelablePromise<DeckJsonFile> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/decks/getAsJsonModel',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiDecksLikeDeck({
        requestBody,
    }: {
        requestBody?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/decks/likeDeck',
            body: requestBody,
            mediaType: 'application/json',
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
     * @returns any OK
     * @throws ApiError
     */
    public static postApiDecksViewDeck({
        requestBody,
    }: {
        requestBody?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/decks/viewDeck',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
