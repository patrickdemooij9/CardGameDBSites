import { DoFetch, DoServerFetch } from '~/helpers/RequestsHelper';

export type TournamentEntrant = {
    id: string;
    tournamentEventId: string;
    playerName: string;
    placement?: number | null;
    wins?: number | null;
    losses?: number | null;
    draws?: number | null;
    deckId?: number | null;
    matches?: TournamentMatch[];
};

export type TournamentMatch = {
    id: string;
    tournamentEventId: string;
    tournamentEntrantId: string;
    roundNumber?: number | null;
    opponentName?: string | null;
    wins: number;
    losses: number;
    draws: number;
    createdAt: string;
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
    sourceType?: string | null;
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
    sourceType?: string | null;
    placement?: number | null;
    wins?: number | null;
    losses?: number | null;
    draws?: number | null;
};

export type CreateTournamentDto = {
    name: string;
    date: string;
    formatId: number;
    playerCount?: number | null;
    sourceUrl?: string | null;
    sourceType?: string | null;
};

export type AddEntrantDto = {
    playerName: string;
    placement?: number | null;
    deckId?: number | null;
    matches?: AddTournamentMatchDto[];
};

export type UpdateEntrantDto = {
    playerName?: string | null;
    placement?: number | null;
    deckId?: number | null;
};

export type AddTournamentMatchDto = {
    roundNumber?: number | null;
    opponentName?: string | null;
    wins: number;
    losses: number;
    draws: number;
};

export type UpdateTournamentMatchDto = {
    roundNumber?: number | null;
    opponentName?: string | null;
    wins?: number | null;
    losses?: number | null;
    draws?: number | null;
};

export type ParsedDeckCard = {
    cardId: number;
    cardName: string;
    amount: number;
    section: string;
};

export type UnmatchedDeckCard = {
    cardName: string;
    amount: number;
    section: string;
};

export type ParseDeckResult = {
    matched: ParsedDeckCard[];
    unmatched: UnmatchedDeckCard[];
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

    async create(dto: CreateTournamentDto): Promise<{ id: string }> {
        return DoServerFetch<{ id: string }>('/api/tournaments', true, {
            method: 'POST',
            body: dto,
        });
    }

    async deleteTournament(id: string): Promise<void> {
        await DoServerFetch('/api/tournaments/' + id, true, {
            method: 'DELETE',
        });
    }

    async addEntrant(tournamentId: string, dto: AddEntrantDto): Promise<{ id: string }> {
        return DoServerFetch<{ id: string }>(`/api/tournaments/${tournamentId}/entrants`, true, {
            method: 'POST',
            body: dto,
        });
    }

    async updateEntrant(entrantId: string, dto: UpdateEntrantDto): Promise<void> {
        await DoServerFetch(`/api/tournaments/entrants/${entrantId}`, true, {
            method: 'PATCH',
            body: dto,
        });
    }

    async deleteEntrant(entrantId: string): Promise<void> {
        await DoServerFetch('/api/tournaments/entrants/' + entrantId, true, {
            method: 'DELETE',
        });
    }

    async addMatch(entrantId: string, dto: AddTournamentMatchDto): Promise<{ id: string }> {
        return DoServerFetch<{ id: string }>(`/api/tournaments/entrants/${entrantId}/matches`, true, {
            method: 'POST',
            body: dto,
        });
    }

    async updateMatch(matchId: string, dto: UpdateTournamentMatchDto): Promise<void> {
        await DoServerFetch(`/api/tournaments/matches/${matchId}`, true, {
            method: 'PATCH',
            body: dto,
        });
    }

    async deleteMatch(matchId: string): Promise<void> {
        await DoServerFetch(`/api/tournaments/matches/${matchId}`, true, {
            method: 'DELETE',
        });
    }

    async parseDeck(text: string): Promise<ParseDeckResult> {
        return DoFetch<ParseDeckResult>('/api/tournaments/parse-deck', {
            method: 'POST',
            body: { text },
        });
    }
}
