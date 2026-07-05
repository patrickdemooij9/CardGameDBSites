import type { TournamentEntrantApiModel, TournamentSummaryApiModel } from '~/api/default';
import { DoFetch } from '~/helpers/RequestsHelper';

export type MetaWinningDeckApiModel = {
  tournamentId: number;
  tournamentName: string;
  tournamentDateUtc: string;
  externalUrl?: string;
  playerName?: string;
  deckId: number;
  deckName?: string;
  leaderName?: string;
};

export type MetaLeaderApiModel = {
  leaderName: string;
  wins: number;
  top8Count: number;
};

export type MetaPopularCardApiModel = {
  cardName: string;
  percentage: number;
};

export default class TournamentService {
  async getRecent(count: number = 6): Promise<TournamentSummaryApiModel[]> {
    return DoFetch<TournamentSummaryApiModel[]>(`/api/tournaments/recent?count=${count}`);
  }

  async getRecentWinners(
    count: number,
    leaderGroupId: number,
    leaderSlotId: number
  ): Promise<MetaWinningDeckApiModel[]> {
    return DoFetch<MetaWinningDeckApiModel[]>(
      `/api/tournaments/meta/recent-winners?count=${count}&leaderGroupId=${leaderGroupId}&leaderSlotId=${leaderSlotId}`
    );
  }

  async getTopLeaders(
    days: number,
    take: number,
    leaderGroupId: number,
    leaderSlotId: number,
    tournamentId?: number
  ): Promise<MetaLeaderApiModel[]> {
    return DoFetch<MetaLeaderApiModel[]>(
      `/api/tournaments/meta/top-leaders?days=${days}&take=${take}&leaderGroupId=${leaderGroupId}&leaderSlotId=${leaderSlotId}${tournamentId ? `&tournamentId=${tournamentId}` : ''}`
    );
  }

  async getPopularCards(
    days: number,
    take: number,
    leaderGroupId: number,
    leaderSlotId: number
  ): Promise<MetaPopularCardApiModel[]> {
    return DoFetch<MetaPopularCardApiModel[]>(
      `/api/tournaments/meta/popular-cards?days=${days}&take=${take}&leaderGroupId=${leaderGroupId}&leaderSlotId=${leaderSlotId}`
    );
  }
}
