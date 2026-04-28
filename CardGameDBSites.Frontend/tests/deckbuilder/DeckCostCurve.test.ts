import { describe, it, expect } from "vitest";
import { CreateDeckModel } from "~/components/decks/deckBuilder/models/CreateDeckModel";
import CreateDeckGroup from "~/components/decks/deckBuilder/models/CreateDeckGroup";
import CreateDeckSlot from "~/components/decks/deckBuilder/models/CreateDeckSlot";
import CreateDeckCardGroup from "~/components/decks/deckBuilder/models/CreateDeckCardGroup";
import { FixedDeckAmountConfig } from "~/components/decks/deckBuilder/models/CreateDeckSlotAmount";
import type { CardDetailApiModel } from "~/api/default";
import { computeCostCurve, computeCostCurveChart } from "~/helpers/CostCurveHelper";

function makeCard(
  id: number,
  attributes: Record<string, Array<string>> = {},
): CardDetailApiModel {
  return { baseId: id, displayName: `Card ${id}`, attributes };
}

function buildDeck(
  cards: { card: CardDetailApiModel; amount: number }[],
): CreateDeckModel {
  const deck = new CreateDeckModel();
  const group = new CreateDeckGroup();
  const slot = new CreateDeckSlot(1, "Main");
  slot.maxCardAmount = new FixedDeckAmountConfig(60);
  const cardGroup = new CreateDeckCardGroup("Default");
  cards.forEach(({ card, amount }) => {
    cardGroup.cards.push({ card, amount, allowRemoval: true, children: [] });
  });
  slot.cardGroups.push(cardGroup);
  group.slots.push(slot);
  deck.groups.push(group);
  return deck;
}


describe("DeckCostCurve", () => {
  describe("computeCostCurve", () => {
    it("returns an empty array when the deck has no cards", () => {
      const deck = new CreateDeckModel();
      expect(computeCostCurve(deck)).toEqual([]);
    });

    it("returns an empty array when no cards have a Cost attribute", () => {
      const deck = buildDeck([{ card: makeCard(1), amount: 2 }]);
      expect(computeCostCurve(deck)).toEqual([]);
    });

    it("counts a single card correctly", () => {
      const deck = buildDeck([{ card: makeCard(1, { Cost: ["3"] }), amount: 1 }]);
      const result = computeCostCurve(deck);
      expect(result).toHaveLength(1);
      expect(result[0]).toEqual({ label: "3", count: 1, heightPercent: 100 });
    });

    it("multiplies count by card amount", () => {
      const deck = buildDeck([{ card: makeCard(1, { Cost: ["2"] }), amount: 3 }]);
      const result = computeCostCurve(deck);
      expect(result[0].count).toBe(3);
    });

    it("groups multiple cards with the same cost", () => {
      const deck = buildDeck([
        { card: makeCard(1, { Cost: ["2"] }), amount: 2 },
        { card: makeCard(2, { Cost: ["2"] }), amount: 1 },
      ]);
      const result = computeCostCurve(deck);
      expect(result).toHaveLength(1);
      expect(result[0].count).toBe(3);
    });

    it("sorts entries by cost numerically", () => {
      const deck = buildDeck([
        { card: makeCard(1, { Cost: ["5"] }), amount: 1 },
        { card: makeCard(2, { Cost: ["1"] }), amount: 1 },
        { card: makeCard(3, { Cost: ["3"] }), amount: 1 },
      ]);
      const result = computeCostCurve(deck);
      expect(result.map((e) => e.label)).toEqual(["1", "3", "5"]);
    });

    it("sets heightPercent to 100 for the most common cost", () => {
      const deck = buildDeck([
        { card: makeCard(1, { Cost: ["1"] }), amount: 4 },
        { card: makeCard(2, { Cost: ["2"] }), amount: 2 },
      ]);
      const result = computeCostCurve(deck);
      const cost1 = result.find((e) => e.label === "1")!;
      const cost2 = result.find((e) => e.label === "2")!;
      expect(cost1.heightPercent).toBe(100);
      expect(cost2.heightPercent).toBe(50);
    });

    it("respects a custom cost attribute name", () => {
      const deck = buildDeck([
        { card: makeCard(1, { ManaCost: ["4"] }), amount: 1 },
      ]);
      expect(computeCostCurve(deck, "ManaCost")).toHaveLength(1);
      expect(computeCostCurve(deck, "Cost")).toHaveLength(0);
    });

    it("ignores cards without the cost attribute", () => {
      const deck = buildDeck([
        { card: makeCard(1, { Cost: ["1"] }), amount: 2 },
        { card: makeCard(2), amount: 1 },
      ]);
      const result = computeCostCurve(deck);
      expect(result).toHaveLength(1);
      expect(result[0].count).toBe(2);
    });
  });
});

describe("computeCostCurveChart", () => {
  const ALL_LABELS = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9+"];

  it("always returns all cost labels (0-9+) even when deck has no cards", () => {
    const deck = new CreateDeckModel();
    const result = computeCostCurveChart(deck);
    expect(result.labels).toEqual(ALL_LABELS);
    expect(result.datasets).toHaveLength(1);
    expect(result.datasets[0].label).toBe("Cards");
    expect(result.datasets[0].data).toEqual(new Array(10).fill(0));
  });

  it("returns a single dataset labeled 'Cards' when no typeAttr is given", () => {
    const deck = buildDeck([{ card: makeCard(1, { Cost: ["2"] }), amount: 3 }]);
    const result = computeCostCurveChart(deck, "Cost");
    expect(result.labels).toEqual(ALL_LABELS);
    expect(result.datasets).toHaveLength(1);
    expect(result.datasets[0].label).toBe("Cards");
    // index 2 corresponds to cost "2"
    expect(result.datasets[0].data[2]).toBe(3);
    // all other slots are 0
    expect(result.datasets[0].data.filter((_, i) => i !== 2)).toEqual(new Array(9).fill(0));
  });

  it("groups counts by card type when typeAttr is provided", () => {
    const deck = buildDeck([
      { card: makeCard(1, { Cost: ["2"], Type: ["Unit"] }), amount: 2 },
      { card: makeCard(2, { Cost: ["2"], Type: ["Spell"] }), amount: 1 },
    ]);
    const result = computeCostCurveChart(deck, "Cost", "Type");
    expect(result.labels).toEqual(ALL_LABELS);
    const unitDs = result.datasets.find((d) => d.label === "Unit")!;
    const spellDs = result.datasets.find((d) => d.label === "Spell")!;
    expect(unitDs.data[2]).toBe(2);
    expect(spellDs.data[2]).toBe(1);
  });

  it("fills zero for cost buckets a type does not have", () => {
    const deck = buildDeck([
      { card: makeCard(1, { Cost: ["1"], Type: ["Unit"] }), amount: 1 },
      { card: makeCard(2, { Cost: ["3"], Type: ["Spell"] }), amount: 1 },
    ]);
    const result = computeCostCurveChart(deck, "Cost", "Type");
    expect(result.labels).toEqual(ALL_LABELS);
    const unitDs = result.datasets.find((d) => d.label === "Unit")!;
    const spellDs = result.datasets.find((d) => d.label === "Spell")!;
    // "1" is at index 1, "3" is at index 3
    expect(unitDs.data[1]).toBe(1);
    expect(unitDs.data[3]).toBe(0);
    expect(spellDs.data[1]).toBe(0);
    expect(spellDs.data[3]).toBe(1);
  });

  it("buckets costs >= 9 into '9+'", () => {
    const deck = buildDeck([
      { card: makeCard(1, { Cost: ["9"] }), amount: 1 },
      { card: makeCard(2, { Cost: ["10"] }), amount: 2 },
    ]);
    const result = computeCostCurveChart(deck, "Cost");
    expect(result.labels).toEqual(ALL_LABELS);
    // "9+" is the last label (index 9)
    expect(result.datasets[0].data[9]).toBe(3);
  });

  it("always uses the fixed label set regardless of which costs are present", () => {
    const deck = buildDeck([
      { card: makeCard(1, { Cost: ["3"] }), amount: 1 },
      { card: makeCard(2, { Cost: ["1"] }), amount: 1 },
      { card: makeCard(3, { Cost: ["9"] }), amount: 1 },
    ]);
    const result = computeCostCurveChart(deck, "Cost");
    expect(result.labels).toEqual(ALL_LABELS);
  });
});
