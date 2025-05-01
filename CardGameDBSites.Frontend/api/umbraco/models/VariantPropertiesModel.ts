/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
export type VariantPropertiesModel = {
    internalID?: number | null;
    displayName?: string | null;
    requirements?: ApiBlockListModel;
    identifier?: Array<IApiContentModel> | null;
    hasPage?: boolean | null;
    requiresPage?: boolean | null;
    childOfBase?: boolean | null;
    childOf?: Array<IApiContentModel> | null;
    manuallyAdd?: boolean | null;
    color?: string | null;
    initial?: string | null;
};

