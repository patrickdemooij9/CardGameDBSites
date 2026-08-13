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
    it("requires a main card to cover every other card", () => {
      // One card needs A, another needs B, so the character has to provide both. Separate
      // clauses AND together, unlike filters inside a clause.
      const cards = [
        makeCard({ Type: ["Card"], Requires: ["A"] }),
        makeCard({ Type: ["Card"], Requires: ["B"] }),
      ];

      const filters = requirement.ToFilters(cards, config())!;

      expect(filters).toHaveLength(2);

      // Negated main-cards condition leads each clause, so non-characters pass untouched.
      for (const clause of filters) {
        expect(clause.filters![0]!.alias).toBe("Type");
        expect(clause.filters![0]!.values).toEqual(["Character"]);
        expect(clause.filters![0]!.negate).toBe(true);
      }

      expect(filters[0]!.filters!.slice(1).map((it) => it.alias)).toEqual([
        "Provides.A.Amount",
      ]);
      expect(filters[1]!.filters!.slice(1).map((it) => it.alias)).toEqual([
        "Provides.B.Amount",
      ]);
      expect(filters[0]!.filters![1]!.mode).toBe("Higher");
      expect(filters[0]!.filters![1]!.values).toEqual(["1"]);
    });

    it("ORs a single card's own resources, since it only needs one overlap", () => {
      // One card listing A and B is satisfied by a character providing either.
      const cards = [makeCard({ Type: ["Card"], Requires: ["A", "B"] })];

      const filters = requirement.ToFilters(cards, config())!;

      expect(filters).toHaveLength(1);
      expect(filters[0]!.filters!.slice(1).map((it) => it.alias)).toEqual([
        "Provides.A.Amount",
        "Provides.B.Amount",
      ]);
    });

    it("collapses cards that ask for the same resources", () => {
      const cards = [
        makeCard({ Type: ["Card"], Requires: ["A"] }),
        makeCard({ Type: ["Card"], Requires: ["A"] }),
        makeCard({ Type: ["Card"], Requires: ["B", "A"] }),
        makeCard({ Type: ["Card"], Requires: ["A", "B"] }),
      ];

      const filters = requirement.ToFilters(cards, config())!;

      // {A} and {A, B} - the duplicates and the reordered pair collapse away.
      expect(filters).toHaveLength(2);
    });

    it("skips cards whose resources are all outside possibleValues", () => {
      const cards = [
        makeCard({ Type: ["Card"], Requires: ["A"] }),
        makeCard({ Type: ["Card"], Requires: ["Z"] }),
      ];

      const filters = requirement.ToFilters(cards, config())!;

      // Z cannot be filtered on, so it must not produce an unsatisfiable clause.
      expect(filters).toHaveLength(1);
      expect(filters[0]!.filters!.slice(1).map((it) => it.alias)).toEqual([
        "Provides.A.Amount",
      ]);
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
