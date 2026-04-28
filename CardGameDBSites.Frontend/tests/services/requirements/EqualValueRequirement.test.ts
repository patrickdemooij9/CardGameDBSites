import { describe, it, expect } from "vitest";
import EqualValueRequirement from "~/app/services/requirements/EqualValueRequirement";
import type { CardDetailApiModel } from "~/app/api/default";

function makeCard(attributes: Record<string, Array<string>>): CardDetailApiModel {
  return { baseId: 1, displayName: "Test Card", attributes };
}

describe("EqualValueRequirement", () => {
  const requirement = new EqualValueRequirement();

  describe("IsValid", () => {
    it("returns true when a card's attribute contains one of the required values", () => {
      const card = makeCard({ Faction: ["Fire"] });
      expect(requirement.IsValid([card], { ability: "Faction", values: ["Fire", "Water"] })).toBe(true);
    });

    it("returns false when a card's attribute does not contain any required values", () => {
      const card = makeCard({ Faction: ["Earth"] });
      expect(requirement.IsValid([card], { ability: "Faction", values: ["Fire", "Water"] })).toBe(false);
    });

    it("returns false when a card is missing the required attribute", () => {
      const card = makeCard({});
      expect(requirement.IsValid([card], { ability: "Faction", values: ["Fire"] })).toBe(false);
    });

    it("returns false when a card's attribute is an empty array", () => {
      const card = makeCard({ Faction: [] });
      expect(requirement.IsValid([card], { ability: "Faction", values: ["Fire"] })).toBe(false);
    });

    it("returns true for an empty card list", () => {
      expect(requirement.IsValid([], { ability: "Faction", values: ["Fire"] })).toBe(true);
    });

    it("returns false when any card in the list fails the requirement", () => {
      const card1 = makeCard({ Faction: ["Fire"] });
      const card2 = makeCard({ Faction: ["Earth"] });
      expect(requirement.IsValid([card1, card2], { ability: "Faction", values: ["Fire"] })).toBe(false);
    });

    it("returns true when all cards in the list satisfy the requirement", () => {
      const card1 = makeCard({ Faction: ["Fire"] });
      const card2 = makeCard({ Faction: ["Fire"] });
      expect(requirement.IsValid([card1, card2], { ability: "Faction", values: ["Fire"] })).toBe(true);
    });
  });

  describe("ToFilters", () => {
    it("returns an AND clause with the correct filter", () => {
      const card = makeCard({ Faction: ["Fire"] });
      const filters = requirement.ToFilters([card], { ability: "Faction", values: ["Fire"] });
      expect(filters).toHaveLength(1);
      expect(filters[0].clauseType).toBe("AND");
      expect(filters[0].filters).toHaveLength(1);
      expect(filters[0].filters![0].alias).toBe("Faction");
      expect(filters[0].filters![0].values).toEqual(["Fire"]);
    });
  });
});
