import { describe, it, expect } from "vitest";
import { GetValidCards, IsValid, GetInvalidRequirements } from "~/services/requirements/RequirementService";
import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import { RestrictionType } from "~/api/default";

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
