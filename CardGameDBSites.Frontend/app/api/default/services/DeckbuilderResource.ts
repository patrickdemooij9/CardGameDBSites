/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateSquadPostModel } from '../models/CreateSquadPostModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class DeckbuilderResource {
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
     * @returns number OK
     * @throws ApiError
     */
    public static postApiDeckbuilderSubmitLoggedIn({
        requestBody,
    }: {
        requestBody?: CreateSquadPostModel,
    }): CancelablePromise<number> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/deckbuilder/submitLoggedIn',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
