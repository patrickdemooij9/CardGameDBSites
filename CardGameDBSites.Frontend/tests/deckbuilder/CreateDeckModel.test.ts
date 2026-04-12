import { describe, it, expect } from "vitest";
import { CreateDeckModel } from "~/components/decks/deckBuilder/models/CreateDeckModel";
import CreateDeckGroup from "~/components/decks/deckBuilder/models/CreateDeckGroup";
import CreateDeckSlot from "~/components/decks/deckBuilder/models/CreateDeckSlot";
import CreateDeckCardGroup from "~/components/decks/deckBuilder/models/CreateDeckCardGroup";
import { FixedDeckAmountConfig } from "~/components/decks/deckBuilder/models/CreateDeckSlotAmount";
import type { CardDetailApiModel } from "~/api/default";

function makeCard(
  id: number,
  attributes: Record<string, Array<string>> = {},
  nonLegalDeckTypes: number[] = [],
): CardDetailApiModel {
  return { baseId: id, displayName: `Card ${id}`, attributes, nonLegalDeckTypes };
}

function makeGroupWithSlot(
  slotId: number,
  cards: { card: CardDetailApiModel; amount: number }[] = [],
  maxAmount = 10,
  minCards = 0,
): CreateDeckGroup {
  const group = new CreateDeckGroup();
  const slot = new CreateDeckSlot(slotId, `Slot ${slotId}`);
  slot.maxCardAmount = new FixedDeckAmountConfig(maxAmount);
  slot.minCards = minCards;
  const cardGroup = new CreateDeckCardGroup("Default");
  cards.forEach(({ card, amount }) => {
    cardGroup.cards.push({ card, amount, allowRemoval: true, children: [] });
  });
  slot.cardGroups.push(cardGroup);
  group.slots.push(slot);
  return group;
}

describe("CreateDeckModel", () => {
  describe("getDeckAmount", () => {
    it("returns 0 for an empty deck", () => {
      const deck = new CreateDeckModel();
      expect(deck.getDeckAmount()).toBe(0);
    });

    it("returns total amount across all groups and slots", () => {
      const deck = new CreateDeckModel();
      deck.groups.push(makeGroupWithSlot(1, [{ card: makeCard(1), amount: 2 }]));
      deck.groups.push(makeGroupWithSlot(2, [{ card: makeCard(2), amount: 3 }]));
      expect(deck.getDeckAmount()).toBe(5);
    });
  });

  describe("getDeckMaxAmount", () => {
    it("returns 0 for an empty deck", () => {
      const deck = new CreateDeckModel();
      expect(deck.getDeckMaxAmount()).toBe(0);
    });

    it("returns the sum of all group max amounts", () => {
      const deck = new CreateDeckModel();
      deck.groups.push(makeGroupWithSlot(1, [], 5));
      deck.groups.push(makeGroupWithSlot(2, [], 3));
      expect(deck.getDeckMaxAmount()).toBe(8);
    });
  });

  describe("isLegalCard", () => {
    it("returns true when typeId is not set", () => {
      const deck = new CreateDeckModel();
      const card = makeCard(1, {}, [1, 2]);
      expect(deck.isLegalCard(card)).toBe(true);
    });

    it("returns true when typeId is set and the card is not in the nonLegalDeckTypes list", () => {
      const deck = new CreateDeckModel();
      deck.typeId = 3;
      const card = makeCard(1, {}, [1, 2]);
      expect(deck.isLegalCard(card)).toBe(true);
    });

    it("returns false when typeId is set and the card is in the nonLegalDeckTypes list", () => {
      const deck = new CreateDeckModel();
      deck.typeId = 1;
      const card = makeCard(1, {}, [1, 2]);
      expect(deck.isLegalCard(card)).toBe(false);
    });
  });

  describe("getCards", () => {
    it("returns an empty array for a deck with no cards", () => {
      const deck = new CreateDeckModel();
      deck.groups.push(makeGroupWithSlot(1, []));
      expect(deck.getCards()).toEqual([]);
    });

    it("returns all DeckCard entries from all groups", () => {
      const deck = new CreateDeckModel();
      const card1 = makeCard(1);
      const card2 = makeCard(2);
      deck.groups.push(makeGroupWithSlot(1, [{ card: card1, amount: 1 }]));
      deck.groups.push(makeGroupWithSlot(2, [{ card: card2, amount: 2 }]));
      const deckCards = deck.getCards();
      expect(deckCards).toHaveLength(2);
      expect(deckCards[0].card).toBe(card1);
      expect(deckCards[1].card).toBe(card2);
    });
  });

  describe("getIllegalCards", () => {
    it("returns an empty array when all cards are legal", () => {
      const deck = new CreateDeckModel();
      deck.typeId = 1;
      const card = makeCard(1, {}, [2]);
      deck.groups.push(makeGroupWithSlot(1, [{ card, amount: 1 }]));
      expect(deck.getIllegalCards()).toEqual([]);
    });

    it("returns the illegal cards when some cards are not legal for the deck type", () => {
      const deck = new CreateDeckModel();
      deck.typeId = 1;
      const legalCard = makeCard(1, {}, [2]);
      const illegalCard = makeCard(2, {}, [1]);
      deck.groups.push(makeGroupWithSlot(1, [
        { card: legalCard, amount: 1 },
        { card: illegalCard, amount: 1 },
      ]));
      const illegal = deck.getIllegalCards();
      expect(illegal).toHaveLength(1);
      expect(illegal[0]).toBe(illegalCard);
    });
  });

  describe("isLegalDeck", () => {
    it("returns true when there are no illegal cards", () => {
      const deck = new CreateDeckModel();
      deck.typeId = 1;
      const card = makeCard(1, {}, [2]);
      deck.groups.push(makeGroupWithSlot(1, [{ card, amount: 1 }]));
      expect(deck.isLegalDeck()).toBe(true);
    });

    it("returns false when there is at least one illegal card", () => {
      const deck = new CreateDeckModel();
      deck.typeId = 1;
      const illegalCard = makeCard(1, {}, [1]);
      deck.groups.push(makeGroupWithSlot(1, [{ card: illegalCard, amount: 1 }]));
      expect(deck.isLegalDeck()).toBe(false);
    });
  });

  describe("validate", () => {
    it("returns a valid result when the deck satisfies all requirements", () => {
      const deck = new CreateDeckModel();
      deck.groups.push(makeGroupWithSlot(1, [{ card: makeCard(1), amount: 1 }], 4, 0));
      expect(deck.validate().isValid()).toBe(true);
    });

    it("returns an invalid result when a slot does not meet minimum card requirement", () => {
      const deck = new CreateDeckModel();
      deck.groups.push(makeGroupWithSlot(1, [], 4, 2)); // minCards = 2, actual = 0
      expect(deck.validate().isValid()).toBe(false);
    });

    it("returns a combined validation across multiple groups", () => {
      const deck = new CreateDeckModel();
      deck.groups.push(makeGroupWithSlot(1, [], 4, 1)); // invalid
      deck.groups.push(makeGroupWithSlot(2, [], 4, 1)); // invalid
      const validation = deck.validate();
      expect(validation.isValid()).toBe(false);
      expect(validation.items.length).toBeGreaterThanOrEqual(2);
    });
  });

  describe("getSlotsForCard", () => {
    it("returns slots that satisfy the slot's requirements for the given card", () => {
      const deck = new CreateDeckModel();
      const group = new CreateDeckGroup();
      const slot = new CreateDeckSlot(1, "Heroes");
      slot.maxCardAmount = new FixedDeckAmountConfig(4);
      slot.requirements = [
        {
          alias: "EqualValue",
          config: { ability: "Faction", values: ["Fire"] },
          restrictionType: "Hard" as any,
        },
      ];
      slot.cardGroups.push(new CreateDeckCardGroup("Default"));
      group.slots.push(slot);
      deck.groups.push(group);

      const fireCard = makeCard(1, { Faction: ["Fire"] });
      const waterCard = makeCard(2, { Faction: ["Water"] });

      expect(deck.getSlotsForCard(fireCard)).toContain(slot);
      expect(deck.getSlotsForCard(waterCard)).not.toContain(slot);
    });

    it("includes the sideboard slot for cards that pass sideboard requirements", () => {
      const deck = new CreateDeckModel();
      deck.hasSideboard = true;
      const sideboardSlot = new CreateDeckSlot(99, "Sideboard");
      sideboardSlot.maxCardAmount = new FixedDeckAmountConfig(15);
      sideboardSlot.requirements = [];
      const sideboardCardGroup = new CreateDeckCardGroup("Default");
      sideboardSlot.cardGroups.push(sideboardCardGroup);
      deck.sideboardSlot = sideboardSlot;

      const card = makeCard(1);
      const slots = deck.getSlotsForCard(card);
      expect(slots).toContain(sideboardSlot);
    });

    it("does not include the sideboard slot when hasSideboard is false", () => {
      const deck = new CreateDeckModel();
      deck.hasSideboard = false;
      const sideboardSlot = new CreateDeckSlot(99, "Sideboard");
      sideboardSlot.maxCardAmount = new FixedDeckAmountConfig(15);
      const sideboardCardGroup = new CreateDeckCardGroup("Default");
      sideboardSlot.cardGroups.push(sideboardCardGroup);
      deck.sideboardSlot = sideboardSlot;

      const card = makeCard(1);
      expect(deck.getSlotsForCard(card)).not.toContain(sideboardSlot);
    });
  });

  describe("getTotalCardAmount", () => {
    it("returns 0 when the card is in no slots", () => {
      const deck = new CreateDeckModel();
      deck.groups.push(makeGroupWithSlot(1, []));
      const card = makeCard(1);
      expect(deck.getTotalCardAmount(card)).toBe(0);
    });

    it("sums amounts across all groups and slots", () => {
      const deck = new CreateDeckModel();
      const card = makeCard(1);
      deck.groups.push(makeGroupWithSlot(1, [{ card, amount: 2 }]));
      deck.groups.push(makeGroupWithSlot(2, [{ card, amount: 1 }]));
      expect(deck.getTotalCardAmount(card)).toBe(3);
    });

    it("includes sideboard card amounts", () => {
      const deck = new CreateDeckModel();
      const card = makeCard(1);
      deck.groups.push(makeGroupWithSlot(1, [{ card, amount: 2 }]));

      const sideboardSlot = new CreateDeckSlot(99, "Sideboard");
      sideboardSlot.maxCardAmount = new FixedDeckAmountConfig(15);
      const sideboardCardGroup = new CreateDeckCardGroup("Default");
      sideboardCardGroup.cards.push({ card, amount: 1, allowRemoval: true, children: [] });
      sideboardSlot.cardGroups.push(sideboardCardGroup);
      deck.sideboardSlot = sideboardSlot;

      expect(deck.getTotalCardAmount(card)).toBe(3);
    });
  });

  describe("getCardAmountInOtherSlots", () => {
    it("returns the count in all slots except the given one", () => {
      const deck = new CreateDeckModel();
      const card = makeCard(1);
      const group = makeGroupWithSlot(1, [{ card, amount: 2 }]);
      deck.groups.push(group);
      const mainSlot = group.slots[0];

      const sideboardSlot = new CreateDeckSlot(99, "Sideboard");
      sideboardSlot.maxCardAmount = new FixedDeckAmountConfig(15);
      const sideboardCardGroup = new CreateDeckCardGroup("Default");
      sideboardCardGroup.cards.push({ card, amount: 1, allowRemoval: true, children: [] });
      sideboardSlot.cardGroups.push(sideboardCardGroup);
      deck.sideboardSlot = sideboardSlot;

      // Excluding mainSlot: only sideboard has 1
      expect(deck.getCardAmountInOtherSlots(mainSlot, card)).toBe(1);
      // Excluding sideboard: only mainSlot has 2
      expect(deck.getCardAmountInOtherSlots(sideboardSlot, card)).toBe(2);
    });
  });

  describe("getSideboardAmount", () => {
    it("returns 0 when there is no sideboard slot", () => {
      const deck = new CreateDeckModel();
      expect(deck.getSideboardAmount()).toBe(0);
    });

    it("returns the total amount in the sideboard slot", () => {
      const deck = new CreateDeckModel();
      const sideboardSlot = new CreateDeckSlot(99, "Sideboard");
      sideboardSlot.maxCardAmount = new FixedDeckAmountConfig(15);
      const sideboardCardGroup = new CreateDeckCardGroup("Default");
      sideboardCardGroup.cards.push({ card: makeCard(1), amount: 3, allowRemoval: true, children: [] });
      sideboardSlot.cardGroups.push(sideboardCardGroup);
      deck.sideboardSlot = sideboardSlot;
      expect(deck.getSideboardAmount()).toBe(3);
    });
  });

  describe("getCards with sideboard", () => {
    it("includes sideboard cards in getCards result", () => {
      const deck = new CreateDeckModel();
      const card1 = makeCard(1);
      const card2 = makeCard(2);
      deck.groups.push(makeGroupWithSlot(1, [{ card: card1, amount: 1 }]));

      const sideboardSlot = new CreateDeckSlot(99, "Sideboard");
      sideboardSlot.maxCardAmount = new FixedDeckAmountConfig(15);
      const sideboardCardGroup = new CreateDeckCardGroup("Default");
      sideboardCardGroup.cards.push({ card: card2, amount: 2, allowRemoval: true, children: [] });
      sideboardSlot.cardGroups.push(sideboardCardGroup);
      deck.sideboardSlot = sideboardSlot;

      const allCards = deck.getCards();
      expect(allCards).toHaveLength(2);
      expect(allCards.map(c => c.card)).toContain(card1);
      expect(allCards.map(c => c.card)).toContain(card2);
    });
  });
});
