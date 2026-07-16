/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TournamentEntrantApiModel } from './TournamentEntrantApiModel';
export type TournamentSummaryApiModel = {
    id?: number;
    name?: string;
    dateUtc?: string;
    type?: string;
    externalUrl?: string | null;
    playerCount?: number;
    winner?: TournamentEntrantApiModel;
};

