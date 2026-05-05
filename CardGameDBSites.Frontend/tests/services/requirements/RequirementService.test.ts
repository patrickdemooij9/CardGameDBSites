import { describe, it, expect } from "vitest";
import { GetValidCards, IsValid, GetInvalidRequirements, ApplyInternalFilterRestrictions } from "~/services/requirements/RequirementService";
import type { CardDetailApiModel, CardsQueryFilterClauseApiModel, RequirementApiModel } from "~/api/default";
import { CardSearchFilterClauseType, CardSearchFilterMode, RestrictionType } from "~/api/default";
import { OverviewFilterType, type OverviewFilterModel } from "~/components/overviews/OverviewFilterModel";

function makeCard(id: number, attributes: Record<string, Array<string>>): CardDetailApiModel {
  return { baseId: id, displayName: `Card ${id}`, attributes };
}

function makeRequirement(
  alias: string,
  config: Record<string, any>,
  restrictionType: RestrictionType = RestrictionType.HARD,
): RequirementApiModel {
  return { alias, config, restrictionType, errorMessage: `Requirement '${alias}' not met` };
}

describe("RequirementService", () => {
  describe("GetValidCards", () => {
    it("returns all cards when there are no requirements", () => {
      const cards = [makeCard(1, { Faction: ["Fire"] }), makeCard(2, { Faction: ["Water"] })];
      expect(GetValidCards(cards, [])).toEqual(cards);
    });

    it("filters out cards that do not satisfy an EqualValue requirement", () => {
      const fire = makeCard(1, { Faction: ["Fire"] });
      const water = makeCard(2, { Faction: ["Water"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] });
      expect(GetValidCards([fire, water], [req])).toEqual([fire]);
    });

    it("returns an empty list when no cards match the requirements", () => {
      const water = makeCard(1, { Faction: ["Water"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] });
      expect(GetValidCards([water], [req])).toEqual([]);
    });

    it("handles unknown requirement aliases gracefully (warns and skips)", () => {
      const card = makeCard(1, { Faction: ["Fire"] });
      const req = makeRequirement("UnknownRequirement", {});
      // Should not throw, unknown requirements are skipped (warned in console)
      expect(GetValidCards([card], [req])).toEqual([card]);
    });
  });

  describe("IsValid", () => {
    it("returns true when there are no requirements", () => {
      const cards = [makeCard(1, { Faction: ["Fire"] })];
      expect(IsValid(cards, [], false)).toBe(true);
    });

    it("returns true when all cards satisfy the requirements", () => {
      const card = makeCard(1, { Faction: ["Fire"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] });
      expect(IsValid([card], [req], false)).toBe(true);
    });

    it("returns false when a card does not satisfy a HARD requirement", () => {
      const card = makeCard(1, { Faction: ["Water"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] });
      expect(IsValid([card], [req], false)).toBe(false);
    });

    it("skips FILTER type requirements", () => {
      const card = makeCard(1, { Faction: ["Water"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] }, RestrictionType.FILTER);
      // FILTER requirements are not enforced in IsValid
      expect(IsValid([card], [req], false)).toBe(true);
    });

    it("skips PASSIVE requirements when ignorePassive is true", () => {
      const card = makeCard(1, { Faction: ["Water"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] }, RestrictionType.PASSIVE);
      expect(IsValid([card], [req], true)).toBe(true);
    });

    it("enforces PASSIVE requirements when ignorePassive is false", () => {
      const card = makeCard(1, { Faction: ["Water"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] }, RestrictionType.PASSIVE);
      expect(IsValid([card], [req], false)).toBe(false);
    });
  });

  describe("GetInvalidRequirements", () => {
    it("returns an empty list when all requirements are met", () => {
      const card = makeCard(1, { Faction: ["Fire"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] });
      expect(GetInvalidRequirements([card], [req], false)).toHaveLength(0);
    });

    it("returns the failing requirements when a card does not satisfy them", () => {
      const card = makeCard(1, { Faction: ["Water"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] });
      const invalid = GetInvalidRequirements([card], [req], false);
      expect(invalid).toHaveLength(1);
      expect(invalid[0]).toBe(req);
    });

    it("returns multiple invalid requirements when several are not met", () => {
      const card = makeCard(1, { Faction: ["Water"], Type: ["Leader"] });
      const req1 = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] });
      const req2 = makeRequirement("NotEqualValue", { ability: "Type", values: ["Leader"] });
      const invalid = GetInvalidRequirements([card], [req1, req2], false);
      expect(invalid).toHaveLength(2);
    });

    it("does not return FILTER requirements even when invalid", () => {
      const card = makeCard(1, { Faction: ["Water"] });
      const req = makeRequirement("EqualValue", { ability: "Faction", values: ["Fire"] }, RestrictionType.FILTER);
      expect(GetInvalidRequirements([card], [req], false)).toHaveLength(0);
    });
  });
});

function makeFilter(alias: string, items: string[] = [], autoFill = false): OverviewFilterModel {
  return {
    Alias: alias,
    DisplayName: alias,
    Type: OverviewFilterType.DROPDOWN,
    Items: items.map((v) => ({ DisplayName: v, Value: v })),
    AutoFillValues: autoFill,
  };
}

function makeAndClause(alias: string, values: string[]): CardsQueryFilterClauseApiModel {
  return {
    clauseType: CardSearchFilterClauseType.AND,
    filters: [{ alias, values, mode: CardSearchFilterMode.CONTAINS }],
  };
}

describe("ApplyInternalFilterRestrictions", () => {
  it("returns all public filters unchanged when there are no internal clauses", () => {
    const filter = makeFilter("Card Type", ["Leader", "Unit"]);
    expect(ApplyInternalFilterRestrictions([], [filter])).toEqual([filter]);
  });

  it("hides a filter entirely when the internal restriction has a single value", () => {
    const filter = makeFilter("Card Type", ["Leader", "Unit"]);
    const clauses = [makeAndClause("Card Type", ["Leader"])];
    expect(ApplyInternalFilterRestrictions(clauses, [filter])).toHaveLength(0);
  });

  it("restricts filter items to the internally allowed values when restriction has multiple values", () => {
    const filter = makeFilter("Card Type", ["Leader", "Unit", "Event", "Upgrade"]);
    const clauses = [makeAndClause("Card Type", ["Unit", "Event", "Upgrade"])];
    const result = ApplyInternalFilterRestrictions(clauses, [filter]);
    expect(result).toHaveLength(1);
    expect(result[0]!.Items.map((i) => i.Value)).toEqual(["Unit", "Event", "Upgrade"]);
  });

  it("does not affect filters whose alias does not match any internal restriction", () => {
    const filter = makeFilter("Faction", ["Fire", "Water"]);
    const clauses = [makeAndClause("Card Type", ["Leader"])];
    expect(ApplyInternalFilterRestrictions(clauses, [filter])).toEqual([filter]);
  });

  it("ignores NOT clauses (they do not define an allowed-value restriction)", () => {
    const filter = makeFilter("Card Type", ["Leader", "Unit"]);
    const notClause: CardsQueryFilterClauseApiModel = {
      clauseType: CardSearchFilterClauseType.NOT,
      filters: [{ alias: "Card Type", values: ["Leader"], mode: CardSearchFilterMode.CONTAINS }],
    };
    expect(ApplyInternalFilterRestrictions([notClause], [filter])).toEqual([filter]);
  });

  it("converts an auto-fill filter to pre-defined items using restriction values when restriction has multiple values", () => {
    const filter = makeFilter("Card Type", [], true);
    const clauses = [makeAndClause("Card Type", ["Unit", "Event"])];
    const result = ApplyInternalFilterRestrictions(clauses, [filter]);
    expect(result).toHaveLength(1);
    expect(result[0]!.AutoFillValues).toBe(false);
    expect(result[0]!.Items.map((i) => i.Value)).toEqual(["Unit", "Event"]);
  });

  it("hides an auto-fill filter when the internal restriction has a single value", () => {
    const filter = makeFilter("Card Type", [], true);
    const clauses = [makeAndClause("Card Type", ["Leader"])];
    expect(ApplyInternalFilterRestrictions(clauses, [filter])).toHaveLength(0);
  });

  it("applies restrictions independently to each filter", () => {
    const cardTypeFilter = makeFilter("Card Type", ["Leader", "Unit", "Event"]);
    const factionFilter = makeFilter("Faction", ["Fire", "Water"]);
    const clauses = [makeAndClause("Card Type", ["Unit", "Event"])];
    const result = ApplyInternalFilterRestrictions(clauses, [cardTypeFilter, factionFilter]);
    expect(result).toHaveLength(2);
    expect(result[0]!.Items.map((i) => i.Value)).toEqual(["Unit", "Event"]);
    expect(result[1]!.Items.map((i) => i.Value)).toEqual(["Fire", "Water"]);
  });
});
