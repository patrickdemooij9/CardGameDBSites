/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckStatus } from './DeckStatus';
export type DeckQueryPostModel = {
    typeId?: number | null;
    status?: DeckStatus;
    cards?: Array<number>;
    take?: number;
    page?: number;
    userId?: number | null;
    orderBy?: string | null;
};

