/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MemberApiModel } from '../models/MemberApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class MembersResource {
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
}
