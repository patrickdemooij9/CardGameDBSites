import type { ImageCropsApiModel, TournamentEntrantApiModel, TournamentSummaryApiModel } from '~/api/default';
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

export type MetaLeaderUsageApiModel = {
  leaderName: string;
  count: number;
};

export type MetaPopularCardApiModel = {
  cardId: number;
  cardName: string;
  percentage: number;
  imageUrl?: ImageCropsApiModel;
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

  async getLeaderUsage(
    tournamentId: number,
    take: number,
    leaderGroupId: number,
    leaderSlotId: number
  ): Promise<MetaLeaderUsageApiModel[]> {
    return DoFetch<MetaLeaderUsageApiModel[]>(
      `/api/tournaments/meta/leader-usage?tournamentId=${tournamentId}&take=${take}&leaderGroupId=${leaderGroupId}&leaderSlotId=${leaderSlotId}`
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
