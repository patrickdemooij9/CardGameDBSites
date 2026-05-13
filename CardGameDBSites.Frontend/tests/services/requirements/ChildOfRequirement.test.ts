import { describe, it, expect } from "vitest";
import ChildOfRequirement from "~/services/requirements/ChildOfRequirement";
import type { CardDetailApiModel } from "~/api/default";

function makeCard(id: number): CardDetailApiModel {
  return { baseId: id, displayName: `Card ${id}`, attributes: {} };
}

describe("ChildOfRequirement", () => {
  const requirement = new ChildOfRequirement();

  describe("IsValid", () => {
    it("returns true when all cards are in the allowedChildIds list", () => {
      const card = makeCard(10);
      expect(requirement.IsValid([card], { allowedChildIds: [10, 20] })).toBe(true);
    });

    it("returns false when a card's baseId is not in the allowedChildIds list", () => {
      const card = makeCard(99);
      expect(requirement.IsValid([card], { allowedChildIds: [10, 20] })).toBe(false);
    });

    it("returns true for an empty card list", () => {
      expect(requirement.IsValid([], { allowedChildIds: [10, 20] })).toBe(true);
    });

    it("returns false when allowedChildIds is empty and cards are provided", () => {
      const card = makeCard(10);
      expect(requirement.IsValid([card], { allowedChildIds: [] })).toBe(false);
    });

    it("returns true when allowedChildIds is missing (defaults to empty) and no cards", () => {
      expect(requirement.IsValid([], {})).toBe(true);
    });

    it("returns false when allowedChildIds is missing and cards are provided", () => {
      const card = makeCard(10);
      expect(requirement.IsValid([card], {})).toBe(false);
    });

    it("returns false when any card is not in the allowedChildIds", () => {
      const card1 = makeCard(10);
      const card2 = makeCard(99);
      expect(requirement.IsValid([card1, card2], { allowedChildIds: [10, 20] })).toBe(false);
    });

    it("returns true when all multiple cards are in the allowedChildIds", () => {
      const card1 = makeCard(10);
      const card2 = makeCard(20);
      expect(requirement.IsValid([card1, card2], { allowedChildIds: [10, 20] })).toBe(true);
    });
  });

  describe("ToFilters", () => {
    it("returns an AND clause filtering by baseId for each allowedChildId", () => {
      const card = makeCard(10);
      const filters = requirement.ToFilters([card], { allowedChildIds: [10, 20] });
      expect(filters).toHaveLength(1);
      expect(filters![0].clauseType).toBe("AND");
      expect(filters![0].filters).toHaveLength(1);
      expect(filters![0].filters![0].alias).toBe("baseId");
      expect(filters![0].filters![0].values).toEqual(["10", "20"]);
      expect(filters![0].filters![0].mode).toBe("Contains");
    });

    it("returns undefined when allowedChildIds is empty", () => {
      expect(requirement.ToFilters([], { allowedChildIds: [] })).toBeUndefined();
    });

    it("returns undefined when allowedChildIds is missing", () => {
      expect(requirement.ToFilters([], {})).toBeUndefined();
    });
  });
});
