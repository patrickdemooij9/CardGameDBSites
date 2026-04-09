import { describe, it, expect } from "vitest";
import CreateDeckCardGroup from "~/components/decks/deckBuilder/models/CreateDeckCardGroup";
import type { CardDetailApiModel } from "~/api/default";

function makeCard(id: number, attributes: Record<string, Array<string>> = {}): CardDetailApiModel {
  return { baseId: id, displayName: `Card ${id}`, attributes };
}

describe("CreateDeckCardGroup", () => {
  describe("constructor", () => {
    it("initializes with the given display name and empty cards array", () => {
      const group = new CreateDeckCardGroup("Heroes");
      expect(group.displayName).toBe("Heroes");
      expect(group.cards).toEqual([]);
      expect(group.requirements).toEqual([]);
    });
  });

  describe("getOrderedCards", () => {
    it("returns cards in insertion order when no sortBy is set", () => {
      const group = new CreateDeckCardGroup("Heroes");
      const card1 = makeCard(1);
      const card2 = makeCard(2);
      group.cards.push({ card: card1, amount: 1, allowRemoval: true, children: [] });
      group.cards.push({ card: card2, amount: 1, allowRemoval: true, children: [] });
      const ordered = group.getOrderedCards();
      expect(ordered[0].card.baseId).toBe(1);
      expect(ordered[1].card.baseId).toBe(2);
    });

    it("does not mutate the original cards array", () => {
      const group = new CreateDeckCardGroup("Heroes");
      group.cards.push({ card: makeCard(1), amount: 1, allowRemoval: true, children: [] });
      group.getOrderedCards();
      expect(group.cards).toHaveLength(1);
    });

    it("returns cards sorted by numeric attribute when sortBy is set", () => {
      const group = new CreateDeckCardGroup("Heroes");
      group.sortBy = "Cost";
      const card1 = makeCard(1, { Cost: ["3"] });
      const card2 = makeCard(2, { Cost: ["1"] });
      const card3 = makeCard(3, { Cost: ["2"] });
      group.cards.push({ card: card1, amount: 1, allowRemoval: true, children: [] });
      group.cards.push({ card: card2, amount: 1, allowRemoval: true, children: [] });
      group.cards.push({ card: card3, amount: 1, allowRemoval: true, children: [] });
      const ordered = group.getOrderedCards();
      expect(ordered[0].card.baseId).toBe(2); // Cost 1
      expect(ordered[1].card.baseId).toBe(3); // Cost 2
      expect(ordered[2].card.baseId).toBe(1); // Cost 3
    });

    it("returns an empty array when there are no cards", () => {
      const group = new CreateDeckCardGroup("Heroes");
      expect(group.getOrderedCards()).toEqual([]);
    });
  });
});
