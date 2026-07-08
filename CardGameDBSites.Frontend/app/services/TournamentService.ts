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

export type PeriodApiModel = {
  id: number;
  name: string;
  startingDateUtc: string;
  endDateUtc?: string;
  isCurrent: boolean;
};

export default class TournamentService {
  async getPeriods(formatId: number): Promise<PeriodApiModel[]> {
    return DoFetch<PeriodApiModel[]>(`/api/tournaments/periods?formatId=${formatId}`);
  }

  async getRecent(periodId: number, count: number = 6): Promise<TournamentSummaryApiModel[]> {
    return DoFetch<TournamentSummaryApiModel[]>(
      `/api/tournaments/recent?periodId=${periodId}&count=${count}`
    );
  }

  async getRecentWinners(
    periodId: number,
    count: number,
    leaderGroupId: number,
    leaderSlotId: number
  ): Promise<MetaWinningDeckApiModel[]> {
    return DoFetch<MetaWinningDeckApiModel[]>(
      `/api/tournaments/meta/recent-winners?periodId=${periodId}&count=${count}&leaderGroupId=${leaderGroupId}&leaderSlotId=${leaderSlotId}`
    );
  }

  async getTopLeaders(
    periodId: number,
    take: number,
    leaderGroupId: number,
    leaderSlotId: number,
    tournamentId?: number
  ): Promise<MetaLeaderApiModel[]> {
    return DoFetch<MetaLeaderApiModel[]>(
      `/api/tournaments/meta/top-leaders?periodId=${periodId}&take=${take}&leaderGroupId=${leaderGroupId}&leaderSlotId=${leaderSlotId}${tournamentId ? `&tournamentId=${tournamentId}` : ''}`
    );
  }

  async getPopularCards(
    periodId: number,
    take: number,
    leaderGroupId: number,
    leaderSlotId: number
  ): Promise<MetaPopularCardApiModel[]> {
    return DoFetch<MetaPopularCardApiModel[]>(
      `/api/tournaments/meta/popular-cards?periodId=${periodId}&take=${take}&leaderGroupId=${leaderGroupId}&leaderSlotId=${leaderSlotId}`
    );
  }
}
