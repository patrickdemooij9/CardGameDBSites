import type { DailyGameBootstrap, DailyGameGuessResult } from "~/models/dailygame/DailyGameModels";

export function shouldShowLeaderboard(state: DailyGameBootstrap): boolean {
  return state.isFinished;
}

export function canGuess(state: DailyGameBootstrap): boolean {
  return !state.isFinished && state.attemptsLeft > 0;
}

export function applyGuessResult(current: DailyGameBootstrap, result: DailyGameGuessResult): DailyGameBootstrap {
  return {
    ...current,
    ...result.state,
  };
}
