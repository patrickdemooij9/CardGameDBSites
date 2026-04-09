import { describe, it, expect } from "vitest";
import NotEqualValueRequirement from "~/services/requirements/NotEqualValueRequirement";
import type { CardDetailApiModel } from "~/api/default";

function makeCard(attributes: Record<string, Array<string>>): CardDetailApiModel {
  return { baseId: 1, displayName: "Test Card", attributes };
}

describe("NotEqualValueRequirement", () => {
  const requirement = new NotEqualValueRequirement();

  describe("IsValid", () => {
    it("returns false when a card's attribute contains one of the excluded values", () => {
      const card = makeCard({ Type: ["Leader"] });
      expect(requirement.IsValid([card], { ability: "Type", values: ["Leader"] })).toBe(false);
    });

    it("returns true when a card's attribute does not contain any excluded values", () => {
      const card = makeCard({ Type: ["Unit"] });
      expect(requirement.IsValid([card], { ability: "Type", values: ["Leader"] })).toBe(true);
    });

    it("returns true when a card is missing the attribute entirely", () => {
      const card = makeCard({});
      expect(requirement.IsValid([card], { ability: "Type", values: ["Leader"] })).toBe(true);
    });

    it("returns true for an empty card list", () => {
      expect(requirement.IsValid([], { ability: "Type", values: ["Leader"] })).toBe(true);
    });

    it("returns false when any card in the list fails the requirement", () => {
      const card1 = makeCard({ Type: ["Unit"] });
      const card2 = makeCard({ Type: ["Leader"] });
      expect(requirement.IsValid([card1, card2], { ability: "Type", values: ["Leader"] })).toBe(false);
    });

    it("returns true when no card in the list has the excluded value", () => {
      const card1 = makeCard({ Type: ["Unit"] });
      const card2 = makeCard({ Type: ["Spell"] });
      expect(requirement.IsValid([card1, card2], { ability: "Type", values: ["Leader"] })).toBe(true);
    });
  });

  describe("ToFilters", () => {
    it("returns a NOT clause with the correct filter", () => {
      const card = makeCard({ Type: ["Unit"] });
      const filters = requirement.ToFilters([card], { ability: "Type", values: ["Leader"] });
      expect(filters).toHaveLength(1);
      expect(filters[0].clauseType).toBe("NOT");
      expect(filters[0].filters).toHaveLength(1);
      expect(filters[0].filters![0].alias).toBe("Type");
      expect(filters[0].filters![0].values).toEqual(["Leader"]);
    });
  });
});
