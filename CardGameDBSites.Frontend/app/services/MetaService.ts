import { DoFetch } from '~/helpers/RequestsHelper';

export type Archetype = {
    id: string;
    siteId: number;
    formatId?: number | null;
    name: string;
    description?: string | null;
};

export type ArchetypeMetaResult = {
    archetype: Archetype;
    deckCount: number;
    previousDeckCount: number;
    trend: number;
};

export default class MetaService {
    async getTopDecks(formatId?: number): Promise<ArchetypeMetaResult[]> {
        const query = formatId !== undefined ? `?formatId=${formatId}` : '';
        return DoFetch<ArchetypeMetaResult[]>(`/api/meta/top-decks${query}`);
    }

    async getTrending(
        formatId?: number,
        currentStart?: Date,
        currentEnd?: Date,
        previousStart?: Date,
        previousEnd?: Date
    ): Promise<ArchetypeMetaResult[]> {
        const params = new URLSearchParams();
        if (formatId !== undefined) params.set('formatId', formatId.toString());
        if (currentStart) params.set('currentStart', currentStart.toISOString());
        if (currentEnd) params.set('currentEnd', currentEnd.toISOString());
        if (previousStart) params.set('previousStart', previousStart.toISOString());
        if (previousEnd) params.set('previousEnd', previousEnd.toISOString());
        const query = params.toString() ? `?${params.toString()}` : '';
        return DoFetch<ArchetypeMetaResult[]>(`/api/meta/trending${query}`);
    }

    async getArchetypes(formatId?: number): Promise<Archetype[]> {
        const query = formatId !== undefined ? `?formatId=${formatId}` : '';
        return DoFetch<Archetype[]>(`/api/meta/archetypes${query}`);
    }

    async getDeckArchetypes(deckId: number): Promise<Archetype[]> {
        return DoFetch<Archetype[]>(`/api/meta/deck/${deckId}/archetypes`);
    }
}
