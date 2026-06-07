import type { TournamentEntrantApiModel } from './TournamentEntrantApiModel';

export type TournamentSummaryApiModel = {
    id?: number;
    name?: string;
    dateUtc?: string;
    type?: string;
    externalUrl?: string;
    playerCount?: number;
    winner?: TournamentEntrantApiModel;
};
