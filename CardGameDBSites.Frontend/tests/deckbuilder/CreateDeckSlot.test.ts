import { describe, it, expect } from "vitest";
import CreateDeckSlot from "~/components/decks/deckBuilder/models/CreateDeckSlot";
import CreateDeckCardGroup from "~/components/decks/deckBuilder/models/CreateDeckCardGroup";
import { FixedDeckAmountConfig } from "~/components/decks/deckBuilder/models/CreateDeckSlotAmount";
import type { CardDetailApiModel } from "~/api/default";

function makeCard(
  id: number,
  attributes: Record<string, Array<string>> = {},
): CardDetailApiModel {
  return { baseId: id, displayName: `Card ${id}`, attributes };
}

function makeSlotWithGroup(maxAmount: number = 4): {
  slot: CreateDeckSlot;
  group: CreateDeckCardGroup;
} {
  const slot = new CreateDeckSlot(1, "Main Deck");
  slot.maxCardAmount = new FixedDeckAmountConfig(maxAmount);

  const group = new CreateDeckCardGroup("Default");
  slot.cardGroups.push(group);

  return { slot, group };
}

describe("CreateDeckSlot", () => {
  describe("constructor", () => {
    it("initializes with the correct default values", () => {
      const slot = new CreateDeckSlot(1, "Heroes");
      expect(slot.id).toBe(1);
      expect(slot.label).toBe("Heroes");
      expect(slot.cardGroups).toEqual([]);
      expect(slot.minCards).toBe(0);
      expect(slot.disableRemoval).toBe(false);
      expect(slot.numberMode).toBe(false);
    });
  });

  describe("getAmount", () => {
    it("returns 0 when there are no cards", () => {
      const { slot } = makeSlotWithGroup();
      expect(slot.getAmount()).toBe(0);
    });

    it("returns the sum of all card amounts across all groups", () => {
      const { slot, group } = makeSlotWithGroup();
      group.cards.push({ card: makeCard(1), amount: 2, allowRemoval: true, children: [] });
      group.cards.push({ card: makeCard(2), amount: 3, allowRemoval: true, children: [] });
      expect(slot.getAmount()).toBe(5);
    });
  });

  describe("isFull", () => {
    it("returns false when the slot is empty", () => {
      const { slot } = makeSlotWithGroup(4);
      expect(slot.isFull()).toBe(false);
    });

    it("returns true when the slot is at capacity", () => {
      const { slot, group } = makeSlotWithGroup(2);
      group.cards.push({ card: makeCard(1), amount: 1, allowRemoval: true, children: [] });
      group.cards.push({ card: makeCard(2), amount: 1, allowRemoval: true, children: [] });
      expect(slot.isFull()).toBe(true);
    });

    it("returns false when the max amount is unlimited (undefined)", () => {
      const { slot } = makeSlotWithGroup(0); // FixedDeckAmountConfig(0) => undefined
      expect(slot.isFull()).toBe(false);
    });
  });

  describe("addCard", () => {
    it("adds a new card to the group", () => {
      const { slot, group } = makeSlotWithGroup(4);
      const card = makeCard(1);
      slot.addCard(card);
      expect(group.cards).toHaveLength(1);
      expect(group.cards[0].amount).toBe(1);
    });

    it("increments the amount when the same card is added again", () => {
      const { slot, group } = makeSlotWithGroup(4);
      const card = makeCard(1, { Amount: ["3"] });
      slot.addCard(card);
      slot.addCard(card);
      expect(group.cards).toHaveLength(1);
      expect(group.cards[0].amount).toBe(2);
    });
  });

  describe("removeCard", () => {
    it("decrements the amount when there are multiple copies", () => {
      const { slot, group } = makeSlotWithGroup(4);
      const card = makeCard(1);
      group.cards.push({ card, amount: 2, allowRemoval: true, children: [] });
      slot.removeCard(card);
      expect(group.cards[0].amount).toBe(1);
    });

    it("removes the card from the group when amount reaches 0", () => {
      const { slot, group } = makeSlotWithGroup(4);
      const card = makeCard(1);
      group.cards.push({ card, amount: 1, allowRemoval: true, children: [] });
      slot.removeCard(card);
      expect(group.cards).toHaveLength(0);
    });
  });

  describe("canAddCard", () => {
    it("returns false when the slot is full", () => {
      const { slot, group } = makeSlotWithGroup(1);
      const card1 = makeCard(1);
      group.cards.push({ card: card1, amount: 1, allowRemoval: true, children: [] });
      const card2 = makeCard(2);
      expect(slot.canAddCard(card2)).toBe(false);
    });

    it("returns false when the card has reached its max amount", () => {
      const { slot, group } = makeSlotWithGroup(4);
      const card = makeCard(1, { Amount: ["1"] });
      group.cards.push({ card, amount: 1, allowRemoval: true, children: [] });
      expect(slot.canAddCard(card)).toBe(false);
    });

    it("returns true when the slot is not full and the card can still be added", () => {
      const { slot } = makeSlotWithGroup(4);
      const card = makeCard(1, { Amount: ["3"] });
      expect(slot.canAddCard(card)).toBe(true);
    });
  });

  describe("isInsideSlot", () => {
    it("returns true when the card is in the slot", () => {
      const { slot, group } = makeSlotWithGroup(4);
      const card = makeCard(1);
      group.cards.push({ card, amount: 1, allowRemoval: true, children: [] });
      expect(slot.isInsideSlot(card)).toBe(true);
    });

    it("returns false when the card is not in the slot", () => {
      const { slot } = makeSlotWithGroup(4);
      const card = makeCard(1);
      expect(slot.isInsideSlot(card)).toBe(false);
    });
  });

  describe("getCards", () => {
    it("returns all cards from all groups", () => {
      const { slot, group } = makeSlotWithGroup(4);
      const card1 = makeCard(1);
      const card2 = makeCard(2);
      group.cards.push({ card: card1, amount: 1, allowRemoval: true, children: [] });
      group.cards.push({ card: card2, amount: 2, allowRemoval: true, children: [] });
      const cards = slot.getCards();
      expect(cards).toHaveLength(2);
      expect(cards).toContain(card1);
      expect(cards).toContain(card2);
    });
  });

  describe("getMaxAmount", () => {
    it("returns the configured max amount", () => {
      const { slot } = makeSlotWithGroup(5);
      expect(slot.getMaxAmount()).toBe(5);
    });

    it("returns undefined when max amount is unlimited", () => {
      const { slot } = makeSlotWithGroup(0);
      expect(slot.getMaxAmount()).toBeUndefined();
    });
  });
});
