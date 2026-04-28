import { describe, expect, it } from "vitest";
import { applyGuessResult, canGuess, shouldShowLeaderboard } from "~/app/services/dailygame/DailyGameStateService";
import type { DailyGameBootstrap } from "~/app/models/dailygame/DailyGameModels";

function makeState(overrides: Partial<DailyGameBootstrap> = {}): DailyGameBootstrap {
  return {
    guestSessionToken: "token",
    maxAttempts: 5,
    attemptsUsed: 0,
    attemptsLeft: 5,
    elapsedSeconds: 0,
    blurLevel: 24,
    isFinished: false,
    isSolved: false,
    attempts: [],
    leaderboard: [],
    ...overrides,
  };
}

describe("DailyGameStateService", () => {
  it("canGuess is true while game is active", () => {
    expect(canGuess(makeState())).toBe(true);
  });

  it("canGuess is false when finished", () => {
    expect(canGuess(makeState({ isFinished: true }))).toBe(false);
  });

  it("leaderboard is shown after finish", () => {
    expect(shouldShowLeaderboard(makeState({ isFinished: true }))).toBe(true);
  });

  it("applyGuessResult merges returned state", () => {
    const current = makeState();
    const next = makeState({ attemptsUsed: 1, attemptsLeft: 4, blurLevel: 18 });
    const result = applyGuessResult(current, { state: next });

    expect(result.attemptsUsed).toBe(1);
    expect(result.attemptsLeft).toBe(4);
    expect(result.blurLevel).toBe(18);
  });
});
