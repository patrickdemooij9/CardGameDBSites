import { DoFetch } from '~/helpers/RequestsHelper';

export type TournamentEntrant = {
    id: string;
    tournamentEventId: string;
    playerName: string;
    placement?: number | null;
    wins?: number | null;
    losses?: number | null;
    draws?: number | null;
    deckId?: number | null;
};

export type TournamentEvent = {
    id: string;
    siteId: number;
    name: string;
    date: string;
    formatId: number;
    formatDisplayName?: string | null;
    playerCount?: number | null;
    sourceUrl?: string | null;
    createdAt: string;
    entrants: TournamentEntrant[];
};

export type DeckTournamentResult = {
    tournamentId: string;
    tournamentName: string;
    date: string;
    formatDisplayName?: string | null;
    playerCount?: number | null;
    sourceUrl?: string | null;
    placement?: number | null;
    wins?: number | null;
    losses?: number | null;
    draws?: number | null;
};

export default class TournamentService {
    async get(id: string): Promise<TournamentEvent> {
        return DoFetch<TournamentEvent>(`/api/tournaments/${id}`);
    }

    async list(formatId?: number): Promise<TournamentEvent[]> {
        const query = formatId !== undefined ? `?formatId=${formatId}` : '';
        return DoFetch<TournamentEvent[]>(`/api/tournaments${query}`);
    }

    async getForDeck(deckId: number): Promise<DeckTournamentResult[]> {
        return DoFetch<DeckTournamentResult[]>(`/api/tournaments/deck/${deckId}`);
    }
}
