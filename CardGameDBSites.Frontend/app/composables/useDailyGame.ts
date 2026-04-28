import type {
  DailyGameBootstrap,
  DailyGameGuessPost,
  DailyGameGuessResult,
  DailyGameLeaderboardEntry,
} from "~/models/dailygame/DailyGameModels";
import { DoServerFetch } from "~/helpers/RequestsHelper";

export function useDailyGame() {
  const bootstrap = async (guestSessionToken?: string) => {
    const query = guestSessionToken
      ? `?guestSessionToken=${encodeURIComponent(guestSessionToken)}`
      : "";
    return DoServerFetch<DailyGameBootstrap>(`/api/dailygame/bootstrap${query}`, true, {
      method: "GET",
    });
  };

  const submitGuess = async (model: DailyGameGuessPost) => {
    return DoServerFetch<DailyGameGuessResult>("/api/dailygame/guess", true, {
      method: "POST",
      body: model,
    });
  };

  const loadLeaderboard = async (take = 50) => {
    return DoServerFetch<DailyGameLeaderboardEntry[]>(
      `/api/dailygame/leaderboard?take=${take}`,
      true,
      { method: "GET" },
    );
  };

  return {
    bootstrap,
    submitGuess,
    loadLeaderboard,
  };
}
