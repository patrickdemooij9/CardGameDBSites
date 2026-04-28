import { describe, it, expect } from "vitest";
import { GetCardValue, GetCardValues } from "~/app/helpers/CardHelper";
import type { CardDetailApiModel } from "~/app/api/default";

function makeCard(attributes: Record<string, Array<string>>): CardDetailApiModel {
  return { baseId: 1, displayName: "Test Card", attributes };
}

describe("CardHelper", () => {
  describe("GetCardValue", () => {
    it("returns null when the ability does not exist on the card", () => {
      const card = makeCard({});
      expect(GetCardValue(card, "Faction")).toBeNull();
    });

    it("returns undefined when the attribute array is empty", () => {
      const card = makeCard({ Faction: [] });
      expect(GetCardValue(card, "Faction")).toBeUndefined();
    });

    it("returns the first element of a single-value array", () => {
      const card = makeCard({ Faction: ["Fire"] });
      expect(GetCardValue<string>(card, "Faction")).toBe("Fire");
    });

    it("returns the first element when the array has multiple values", () => {
      const card = makeCard({ Tags: ["Hero", "Warrior", "Dragon"] });
      expect(GetCardValue<string>(card, "Tags")).toBe("Hero");
    });

    it("returns a numeric value when the attribute holds numbers", () => {
      const card = makeCard({ Cost: ["3"] });
      expect(GetCardValue<string>(card, "Cost")).toBe("3");
    });
  });

  describe("GetCardValues", () => {
    it("returns null when the ability does not exist on the card", () => {
      const card = makeCard({});
      expect(GetCardValues(card, "Tags")).toBeNull();
    });

    it("returns all values in the array", () => {
      const card = makeCard({ Tags: ["Hero", "Warrior"] });
      expect(GetCardValues<string>(card, "Tags")).toEqual(["Hero", "Warrior"]);
    });

    it("returns an empty array when the attribute array is empty", () => {
      const card = makeCard({ Tags: [] });
      expect(GetCardValues<string>(card, "Tags")).toEqual([]);
    });
  });
});
