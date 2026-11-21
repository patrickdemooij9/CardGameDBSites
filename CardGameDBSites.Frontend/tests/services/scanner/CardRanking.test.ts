import { describe, expect, it } from "vitest";
import { EXCLUDED, rankCards } from "#card-scanner/ranking";

describe("rankCards", () => {
  it("returns the best row of each card in descending order", () => {
    const ranked = rankCards([0.2, 0.9, 0.5, 0.7], [1, 2, 3, 2], 3);

    expect(ranked).toEqual([
      { row: 1, score: 0.9 },
      { row: 2, score: 0.5 },
      { row: 0, score: 0.2 },
    ]);
  });

  it("finds a rival however many rows the winning card owns", () => {
    const scores = [0.9, 0.89, 0.88, 0.87, 0.86, 0.85, 0.84, 0.83, 0.82, 0.3];
    const baseIds = [7, 7, 7, 7, 7, 7, 7, 7, 7, 8];

    expect(rankCards(scores, baseIds, 2)).toEqual([
      { row: 0, score: 0.9 },
      { row: 9, score: 0.3 },
    ]);
  });

  it("skips excluded rows", () => {
    expect(rankCards([EXCLUDED, 0.4, EXCLUDED], [1, 2, 3], 3)).toEqual([{ row: 1, score: 0.4 }]);
  });

  it("keeps the earlier row on a tie", () => {
    expect(rankCards([0.5, 0.5], [1, 2], 1)).toEqual([{ row: 0, score: 0.5 }]);
  });

  it("returns nothing when every row is excluded", () => {
    expect(rankCards([EXCLUDED, EXCLUDED], [1, 2], 3)).toEqual([]);
  });
});
