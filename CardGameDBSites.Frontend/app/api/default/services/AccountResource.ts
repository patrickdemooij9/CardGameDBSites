/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CurrentMemberApiModel } from '../models/CurrentMemberApiModel';
import type { ForgotPasswordPostModel } from '../models/ForgotPasswordPostModel';
import type { ForgotPasswordResetPostModel } from '../models/ForgotPasswordResetPostModel';
import type { LoginPostModel } from '../models/LoginPostModel';
import type { RegisterPostModel } from '../models/RegisterPostModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AccountResource {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiAccountForgotPassword({
        requestBody,
    }: {
        requestBody?: ForgotPasswordPostModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/account/ForgotPassword',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
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
     * @returns string OK
     * @throws ApiError
     */
    public static postApiAccountImpersonate({
        memberId,
    }: {
        memberId: number,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/account/impersonate/{memberId}',
            path: {
                'memberId': memberId,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiAccountIsLoggedIn(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/account/IsLoggedIn',
        });
    }
    /**
     * @returns string OK
     * @throws ApiError
     */
    public static postApiAccountLogin({
        requestBody,
    }: {
        requestBody?: LoginPostModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/account/Login',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns string OK
     * @throws ApiError
     */
    public static postApiAccountRegister({
        requestBody,
    }: {
        requestBody?: RegisterPostModel,
    }): CancelablePromise<string> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/account/Register',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiAccountResetPassword({
        requestBody,
    }: {
        requestBody?: ForgotPasswordResetPostModel,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/account/ResetPassword',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
