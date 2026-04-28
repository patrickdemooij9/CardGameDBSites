import { describe, it, expect } from "vitest";
import CreateDeckGroup from "~/app/components/decks/deckBuilder/models/CreateDeckGroup";
import CreateDeckSlot from "~/app/components/decks/deckBuilder/models/CreateDeckSlot";
import CreateDeckCardGroup from "~/app/components/decks/deckBuilder/models/CreateDeckCardGroup";
import { FixedDeckAmountConfig } from "~/app/components/decks/deckBuilder/models/CreateDeckSlotAmount";
import type { CardDetailApiModel } from "~/app/api/default";

function makeCard(
  id: number,
  attributes: Record<string, Array<string>> = {},
): CardDetailApiModel {
  return { baseId: id, displayName: `Card ${id}`, attributes };
}

function makePopulatedSlot(slotId: number, cards: { card: CardDetailApiModel; amount: number }[], maxAmount: number = 10): CreateDeckSlot {
  const slot = new CreateDeckSlot(slotId, `Slot ${slotId}`);
  slot.maxCardAmount = new FixedDeckAmountConfig(maxAmount);
  const cardGroup = new CreateDeckCardGroup("Default");
  cards.forEach(({ card, amount }) => {
    cardGroup.cards.push({ card, amount, allowRemoval: true, children: [] });
  });
  slot.cardGroups.push(cardGroup);
  return slot;
}

describe("CreateDeckGroup", () => {
  describe("getAmount", () => {
    it("returns 0 when there are no slots", () => {
      const group = new CreateDeckGroup();
      expect(group.getAmount()).toBe(0);
    });

    it("returns the sum of all slot amounts", () => {
      const group = new CreateDeckGroup();
      group.slots.push(makePopulatedSlot(1, [{ card: makeCard(1), amount: 2 }]));
      group.slots.push(makePopulatedSlot(2, [{ card: makeCard(2), amount: 3 }]));
      expect(group.getAmount()).toBe(5);
    });
  });

  describe("getMaxAmount", () => {
    it("returns 0 when there are no slots", () => {
      const group = new CreateDeckGroup();
      expect(group.getMaxAmount()).toBe(0);
    });

    it("returns the sum of slot max amounts", () => {
      const group = new CreateDeckGroup();
      group.slots.push(makePopulatedSlot(1, [], 5));
      group.slots.push(makePopulatedSlot(2, [], 3));
      expect(group.getMaxAmount()).toBe(8);
    });
  });

  describe("getCards", () => {
    it("returns all cards from all slots", () => {
      const group = new CreateDeckGroup();
      const card1 = makeCard(1);
      const card2 = makeCard(2);
      group.slots.push(makePopulatedSlot(1, [{ card: card1, amount: 1 }]));
      group.slots.push(makePopulatedSlot(2, [{ card: card2, amount: 1 }]));
      const cards = group.getCards();
      expect(cards).toHaveLength(2);
      expect(cards).toContain(card1);
      expect(cards).toContain(card2);
    });

    it("returns an empty array when there are no cards", () => {
      const group = new CreateDeckGroup();
      group.slots.push(makePopulatedSlot(1, []));
      expect(group.getCards()).toEqual([]);
    });
  });

  describe("validateGroup", () => {
    it("returns a valid result when all slots are satisfied", () => {
      const group = new CreateDeckGroup();
      const slot = makePopulatedSlot(1, [{ card: makeCard(1), amount: 1 }], 4);
      slot.minCards = 0;
      group.slots.push(slot);
      const result = group.validateGroup();
      expect(result.isValid()).toBe(true);
    });

    it("returns invalid result when a slot has too few cards", () => {
      const group = new CreateDeckGroup();
      const slot = makePopulatedSlot(1, [], 4);
      slot.minCards = 2;
      group.slots.push(slot);
      const result = group.validateGroup();
      expect(result.isValid()).toBe(false);
    });

    it("returns invalid result with error messages from group-level requirements", () => {
      const group = new CreateDeckGroup();
      const card = makeCard(1, { Faction: ["Water"] });
      const slot = makePopulatedSlot(1, [{ card, amount: 1 }], 4);
      slot.minCards = 0;
      group.slots.push(slot);
      group.requirements.push({
        alias: "EqualValue",
        config: { ability: "Faction", values: ["Fire"] },
        restrictionType: "Hard" as any,
        errorMessage: "Cards must be Fire faction",
      });
      const result = group.validateGroup();
      expect(result.isValid()).toBe(false);
      const showableErrors = result.items.filter((item) => item.showMessage);
      expect(showableErrors.some((e) => e.errorMessage === "Cards must be Fire faction")).toBe(true);
    });
  });

  describe("getAvailableSlots", () => {
    it("returns all slots in the group", () => {
      const group = new CreateDeckGroup();
      group.slots.push(makePopulatedSlot(1, []));
      group.slots.push(makePopulatedSlot(2, []));
      expect(group.getAvailableSlots()).toHaveLength(2);
    });
  });
});
