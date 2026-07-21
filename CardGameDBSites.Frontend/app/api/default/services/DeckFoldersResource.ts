/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateDeckFolderPostModel } from '../models/CreateDeckFolderPostModel';
import type { DeckFolderApiModel } from '../models/DeckFolderApiModel';
import type { MoveDecksPostModel } from '../models/MoveDecksPostModel';
import type { UpdateDeckFolderPostModel } from '../models/UpdateDeckFolderPostModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class DeckFoldersResource {
    /**
     * @returns number OK
     * @throws ApiError
     */
    public static postApiDeckFoldersCreate({
        requestBody,
    }: {
        requestBody?: CreateDeckFolderPostModel,
    }): CancelablePromise<number> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/deckFolders/create',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiDeckFoldersDelete({
        id,
    }: {
        id?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/deckFolders/delete',
            query: {
                'id': id,
            },
        });
    }
    /**
     * @returns DeckFolderApiModel OK
     * @throws ApiError
     */
    public static getApiDeckFoldersGetByUser(): CancelablePromise<Array<DeckFolderApiModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/deckFolders/getByUser',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiDeckFoldersMoveDecks({
        requestBody,
    }: {
        requestBody?: MoveDecksPostModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/deckFolders/moveDecks',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static putApiDeckFoldersUpdate({
        requestBody,
    }: {
        requestBody?: UpdateDeckFolderPostModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/deckFolders/update',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
