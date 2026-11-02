/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MemberSettingsApiModel } from './MemberSettingsApiModel';
export type CurrentMemberApiModel = {
    id: number;
    displayName: string;
    likedDecks: Array<number>;
    isAdmin: boolean;
    impersonatedBy?: number | null;
    settings: MemberSettingsApiModel;
};

