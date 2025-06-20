/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateSquadSquadPostModel } from './CreateSquadSquadPostModel';
export type CreateSquadPostModel = {
    id?: number | null;
    typeId?: number;
    name?: string;
    description?: string | null;
    squads?: Array<CreateSquadSquadPostModel>;
    publish?: boolean;
};

