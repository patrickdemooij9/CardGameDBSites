/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { PagedCommunityBlogPostsApiModel } from '../models/PagedCommunityBlogPostsApiModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class CommunityResource {
    /**
     * @returns PagedCommunityBlogPostsApiModel OK
     * @throws ApiError
     */
    public static getApiCommunityPosts({
        page = 1,
        pageSize = 30,
    }: {
        page?: number,
        pageSize?: number,
    }): CancelablePromise<PagedCommunityBlogPostsApiModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/community/posts',
            query: {
                'page': page,
                'pageSize': pageSize,
            },
        });
    }
}
