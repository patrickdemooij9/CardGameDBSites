/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ImportTournament } from '../models/ImportTournament';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ManagementResource {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiManagement({
        requestBody,
    }: {
        requestBody?: ImportTournament,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/management',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiManagementDecksCreatePreset({
        deckId,
    }: {
        deckId: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/management/decks/{deckId}/createPreset',
            path: {
                'deckId': deckId,
            },
            errors: {
                400: `Bad Request`,
                403: `Forbidden`,
                404: `Not Found`,
            },
        });
    }
}
