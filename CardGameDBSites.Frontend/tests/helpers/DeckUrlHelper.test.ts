import { describe, expect, test } from "vitest";
import { getDeckDetailUrl, getEditDeckUrl } from "~/helpers/DeckUrlHelper";

describe("DeckUrlHelper", () => {
  test("builds deck detail url from overview url", () => {
    expect(getDeckDetailUrl("/strike-teams", 42)).toBe("/strike-teams/42");
  });

  test("builds deck detail url with fallback", () => {
    expect(getDeckDetailUrl(undefined, 42)).toBe("/decks/42");
  });

  test("builds edit deck url from create deck url", () => {
    expect(getEditDeckUrl("/create-strike-team", 12)).toBe(
      "/create-strike-team?id=12",
    );
  });

  test("builds edit deck url with fallback", () => {
    expect(getEditDeckUrl(undefined, 12)).toBe("/create-deck?id=12");
  });
});
