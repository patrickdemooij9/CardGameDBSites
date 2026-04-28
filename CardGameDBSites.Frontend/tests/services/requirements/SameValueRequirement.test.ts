import { describe, it, expect } from "vitest";
import SameValueRequirement from "~/app/services/requirements/SameValueRequirement";
import type { CardDetailApiModel } from "~/app/api/default";

function makeCard(attributes: Record<string, Array<string>>): CardDetailApiModel {
  return { baseId: 1, displayName: "Test Card", attributes };
}

describe("SameValueRequirement", () => {
  const requirement = new SameValueRequirement();

  describe("IsValid", () => {
    it("returns true for an empty card list", () => {
      expect(requirement.IsValid([], { ability: "Faction" })).toBe(true);
    });

    it("returns true when a single card has a value for the attribute", () => {
      const card = makeCard({ Faction: ["Fire"] });
      expect(requirement.IsValid([card], { ability: "Faction" })).toBe(true);
    });

    it("returns false when a single card is missing the attribute", () => {
      const card = makeCard({});
      expect(requirement.IsValid([card], { ability: "Faction" })).toBe(false);
    });

    it("returns true when all cards share at least one common value", () => {
      const card1 = makeCard({ Faction: ["Fire", "Water"] });
      const card2 = makeCard({ Faction: ["Fire", "Earth"] });
      expect(requirement.IsValid([card1, card2], { ability: "Faction" })).toBe(true);
    });

    it("returns false when cards have no common value", () => {
      const card1 = makeCard({ Faction: ["Fire"] });
      const card2 = makeCard({ Faction: ["Water"] });
      expect(requirement.IsValid([card1, card2], { ability: "Faction" })).toBe(false);
    });

    it("returns false when one card is missing the attribute", () => {
      const card1 = makeCard({ Faction: ["Fire"] });
      const card2 = makeCard({});
      expect(requirement.IsValid([card1, card2], { ability: "Faction" })).toBe(false);
    });

    it("returns false when one card has an empty attribute array", () => {
      const card1 = makeCard({ Faction: ["Fire"] });
      const card2 = makeCard({ Faction: [] });
      expect(requirement.IsValid([card1, card2], { ability: "Faction" })).toBe(false);
    });
  });

  describe("ToFilters", () => {
    it("returns undefined for an empty card list", () => {
      expect(requirement.ToFilters([], { ability: "Faction" })).toBeUndefined();
    });

    it("returns a filter based on the single card's values", () => {
      const card = makeCard({ Faction: ["Fire", "Water"] });
      const filters = requirement.ToFilters([card], { ability: "Faction" });
      expect(filters).toHaveLength(1);
      expect(filters![0].clauseType).toBe("AND");
      expect(filters![0].filters![0].alias).toBe("Faction");
      expect(filters![0].filters![0].values).toEqual(["Fire", "Water"]);
    });

    it("returns a filter based on the intersection of multiple cards' values", () => {
      const card1 = makeCard({ Faction: ["Fire", "Water"] });
      const card2 = makeCard({ Faction: ["Fire", "Earth"] });
      const filters = requirement.ToFilters([card1, card2], { ability: "Faction" });
      expect(filters).toHaveLength(1);
      expect(filters![0].filters![0].values).toEqual(["Fire"]);
    });

    it("returns undefined when cards have no common value", () => {
      const card1 = makeCard({ Faction: ["Fire"] });
      const card2 = makeCard({ Faction: ["Water"] });
      expect(requirement.ToFilters([card1, card2], { ability: "Faction" })).toBeUndefined();
    });

    it("returns undefined when a card is missing the attribute", () => {
      const card1 = makeCard({ Faction: ["Fire"] });
      const card2 = makeCard({});
      expect(requirement.ToFilters([card1, card2], { ability: "Faction" })).toBeUndefined();
    });
  });
});
