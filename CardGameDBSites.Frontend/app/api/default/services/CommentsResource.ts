/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CommentViewModel } from '../models/CommentViewModel';
import type { CreateCommentPostModel } from '../models/CreateCommentPostModel';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class CommentsResource {
    /**
     * @returns CommentViewModel OK
     * @throws ApiError
     */
    public static postApiCommentsAddCardComment({
        requestBody,
    }: {
        requestBody?: CreateCommentPostModel,
    }): CancelablePromise<CommentViewModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/comments/addCardComment',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns CommentViewModel OK
     * @throws ApiError
     */
    public static postApiCommentsAddDeckComment({
        requestBody,
    }: {
        requestBody?: CreateCommentPostModel,
    }): CancelablePromise<CommentViewModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/comments/addDeckComment',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiCommentsDeleteCardComment({
        commentId,
    }: {
        commentId?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/comments/deleteCardComment',
            query: {
                'commentId': commentId,
            },
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiCommentsDeleteDeckComment({
        commentId,
    }: {
        commentId?: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/comments/deleteDeckComment',
            query: {
                'commentId': commentId,
            },
        });
    }
    /**
     * @returns CommentViewModel OK
     * @throws ApiError
     */
    public static getApiCommentsGetByCard({
        cardId,
    }: {
        cardId?: number,
    }): CancelablePromise<Array<CommentViewModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/comments/getByCard',
            query: {
                'cardId': cardId,
            },
        });
    }
    /**
     * @returns CommentViewModel OK
     * @throws ApiError
     */
    public static getApiCommentsGetByDeck({
        deckId,
    }: {
        deckId?: number,
    }): CancelablePromise<Array<CommentViewModel>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/comments/getByDeck',
            query: {
                'deckId': deckId,
            },
        });
    }
}
