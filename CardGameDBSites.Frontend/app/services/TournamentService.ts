import type { TournamentEntrantApiModel, TournamentSummaryApiModel } from '~/api/default';
import { DoFetch } from '~/helpers/RequestsHelper';

export default class TournamentService {
  async getRecent(count: number = 6): Promise<TournamentSummaryApiModel[]> {
    return DoFetch<TournamentSummaryApiModel[]>(`/api/tournaments/recent?count=${count}`);
  }

  async getTop8(tournamentId: number): Promise<TournamentEntrantApiModel[]> {
    return DoFetch<TournamentEntrantApiModel[]>(`/api/tournaments/${tournamentId}/top8`);
  }
}
