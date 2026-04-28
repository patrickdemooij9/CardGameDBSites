/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IApiContentModelBase } from './IApiContentModelBase';
import type { IApiContentRouteModel } from './IApiContentRouteModel';
export type IApiContentResponseModelBase = (IApiContentModelBase & {
    readonly id?: string;
    readonly contentType: string;
    readonly name?: string | null;
    readonly createDate?: string;
    readonly updateDate?: string;
    route?: IApiContentRouteModel;
    readonly cultures?: Record<string, IApiContentRouteModel>;
});

