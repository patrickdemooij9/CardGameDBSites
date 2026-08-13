import { describe, it, expect } from "vitest";
import ResourceRequirement from "~/services/requirements/ResourceRequirement";
import type { CardDetailApiModel } from "~/api/default";

let nextId = 1;
function makeCard(attributes: Record<string, Array<string>>): CardDetailApiModel {
  return { baseId: nextId++, displayName: "Test Card", attributes };
}

// Characters carry Provides; cards carry Requires. Main cards are identified by Type.
const characterCondition = {
  type: "EqualValue",
  config: { ability: "Type", values: ["Character"] },
};

function config(overrides: Record<string, any> = {}) {
  return {
    mainAbility: "Provides",
    ability: "Requires",
    resourceMode: "ContainsAny",
    possibleValues: ["A", "B", "C", "D"],
    mainCardsCondition: [characterCondition],
    ...overrides,
  };
}

describe("ResourceRequirement", () => {
  const requirement = new ResourceRequirement();

  describe("ToFilters without main cards", () => {
    it("filters the main side on the union of what the other cards require", () => {
      const cards = [
        makeCard({ Type: ["Card"], Requires: ["A"] }),
        makeCard({ Type: ["Card"], Requires: ["B"] }),
      ];

      const filters = requirement.ToFilters(cards, config())!;

      expect(filters).toHaveLength(1);
      const filter = filters[0]!.filters!;

      // Negated main-cards condition first, so non-characters pass the OR group untouched.
      expect(filter[0]!.alias).toBe("Type");
      expect(filter[0]!.values).toEqual(["Character"]);
      expect(filter[0]!.negate).toBe(true);

      // Then the union {A, B} - C and D are not required by anything, so they are absent.
      expect(filter.slice(1).map((it) => it.alias)).toEqual([
        "Provides.A.Amount",
        "Provides.B.Amount",
      ]);
      expect(filter.slice(1).every((it) => it.mode === "Higher")).toBe(true);
      expect(filter.slice(1).every((it) => it.values![0] === "1")).toBe(true);
    });

    it("uses the union rather than requiring overlap with every card", () => {
      // A character providing only A is still valid once a second character covers B,
      // so the permissive reading must not exclude it.
      const cards = [
        makeCard({ Type: ["Card"], Requires: ["A"] }),
        makeCard({ Type: ["Card"], Requires: ["B"] }),
      ];

      const filters = requirement.ToFilters(cards, config())!;

      // One clause, so the two Provides filters OR together rather than AND.
      expect(filters).toHaveLength(1);
      expect(filters[0]!.filters).toHaveLength(3);
    });

    it("returns undefined when the other cards require nothing in possibleValues", () => {
      const cards = [makeCard({ Type: ["Card"], Requires: ["Z"] })];

      expect(requirement.ToFilters(cards, config())).toBeUndefined();
    });

    it("returns undefined when the main-cards condition cannot be inverted", () => {
      const cards = [makeCard({ Type: ["Card"], Requires: ["A"] })];

      const filters = requirement.ToFilters(
        cards,
        config({ mainCardsCondition: [{ type: "SameValue", config: { ability: "Type" } }] }),
      );

      expect(filters).toBeUndefined();
    });

    it("still returns undefined for Subset, which needs pool counts", () => {
      const cards = [makeCard({ Type: ["Card"], Requires: ["A"] })];

      expect(requirement.ToFilters(cards, config({ resourceMode: "Subset" }))).toBeUndefined();
    });

    it("still returns undefined for Budget, which needs pool counts", () => {
      const cards = [makeCard({ Type: ["Card"], Requires: ["A"] })];

      expect(requirement.ToFilters(cards, config({ resourceMode: "Budget" }))).toBeUndefined();
    });
  });

  describe("ToFilters with main cards", () => {
    it("falls back to filtering the other side once a character is picked", () => {
      const cards = [
        makeCard({ Type: ["Character"], Provides: ["A"] }),
        makeCard({ Type: ["Card"], Requires: ["A"] }),
      ];

      const filters = requirement.ToFilters(cards, config())!;

      expect(filters).toHaveLength(1);
      expect(filters[0]!.filters!.map((it) => it.alias)).toEqual(["Requires.A.Amount"]);
    });
  });
});
