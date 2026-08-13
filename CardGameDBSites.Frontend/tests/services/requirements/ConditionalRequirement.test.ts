import { describe, it, expect } from "vitest";
import ConditionalRequirement from "~/services/requirements/ConditionalRequirement";
import type { CardDetailApiModel, CardsQueryFilterClauseApiModel } from "~/api/default";

function makeCard(attributes: Record<string, Array<string>>): CardDetailApiModel {
  return { baseId: 1, displayName: "Test Card", attributes };
}

function aliasesOf(clause: CardsQueryFilterClauseApiModel) {
  return clause.filters!.map((it) => it.alias);
}

describe("ConditionalRequirement", () => {
  const requirement = new ConditionalRequirement();

  const leaderCondition = {
    type: "EqualValue",
    config: { ability: "Type", values: ["Leader"] },
  };

  describe("ToFilters", () => {
    it("distributes the negated conditions over every requirement clause", () => {
      const card = makeCard({ Type: ["Leader"], Provides: ["A"] });

      const filters = requirement.ToFilters([card], {
        condition: [leaderCondition],
        requirements: [
          { type: "EqualValue", config: { ability: "Faction", values: ["Red"] } },
          { type: "EqualValue", config: { ability: "Rarity", values: ["Rare"] } },
        ],
      })!;

      // Two requirements stay AND'd as separate clauses, each carrying the negated condition.
      expect(filters).toHaveLength(2);
      expect(aliasesOf(filters[0]!)).toEqual(["Type", "Faction"]);
      expect(aliasesOf(filters[1]!)).toEqual(["Type", "Rarity"]);
      expect(filters[0]!.filters![0]!.negate).toBe(true);
      expect(filters[1]!.filters![0]!.negate).toBe(true);
    });

    it("keeps a Subset resource requirement's exclusion clauses separate", () => {
      // Leader provides the pool {A, B, C}; D is a possible value outside the pool.
      const leader = makeCard({ Type: ["Leader"], Provides: ["A", "B", "C"] });

      const filters = requirement.ToFilters([leader], {
        condition: [leaderCondition],
        requirements: [
          {
            type: "Resource",
            config: {
              mainAbility: "Provides",
              ability: "Requires",
              resourceMode: "Subset",
              possibleValues: ["A", "B", "C", "D"],
              mainCardsCondition: [leaderCondition],
            },
          },
        ],
      })!;

      // One clause for the in-pool ranges, plus one per excluded value. Flattening these
      // into a single clause would OR the exclusion in and let {A, D} cards through.
      expect(filters).toHaveLength(2);
      expect(aliasesOf(filters[0]!)).toEqual([
        "Type",
        "Requires.A.Amount",
        "Requires.B.Amount",
        "Requires.C.Amount",
      ]);
      expect(aliasesOf(filters[1]!)).toEqual(["Type", "Requires.D.Amount"]);

      const exclusion = filters[1]!.filters![1]!;
      expect(exclusion.values).toEqual(["0"]);
      expect(exclusion.mode).toBe("Lower");
    });

    it("narrows the cards by the conditions, not by the requirements", () => {
      // The condition selects on Faction, while main cards are identified by Type. Feeding the
      // requirements to GetValidCards instead would keep every character (a character on its own
      // trivially satisfies the resource requirement) and drop the card, widening the pool to
      // {A, B}. Narrowing by the condition keeps only the Red cards, so the pool is just {A}.
      const cards = [
        makeCard({ Type: ["Character"], Faction: ["Red"], Provides: ["A"] }),
        makeCard({ Type: ["Character"], Faction: ["Blue"], Provides: ["B"] }),
        makeCard({ Type: ["Card"], Faction: ["Red"], Requires: ["A"] }),
      ];

      const filters = requirement.ToFilters(cards, {
        condition: [{ type: "EqualValue", config: { ability: "Faction", values: ["Red"] } }],
        requirements: [
          {
            type: "Resource",
            config: {
              mainAbility: "Provides",
              ability: "Requires",
              resourceMode: "ContainsAny",
              possibleValues: ["A", "B"],
              mainCardsCondition: [
                { type: "EqualValue", config: { ability: "Type", values: ["Character"] } },
              ],
            },
          },
        ],
      })!;

      expect(filters).toHaveLength(1);
      // Negated condition, then only the Red character's resource - not Blue's.
      expect(aliasesOf(filters[0]!)).toEqual(["Faction", "Requires.A.Amount"]);
    });

    it("returns undefined when there are no requirement filters", () => {
      const card = makeCard({ Type: ["Leader"] });

      const filters = requirement.ToFilters([card], {
        condition: [leaderCondition],
        requirements: [],
      });

      expect(filters).toBeUndefined();
    });
  });
});
