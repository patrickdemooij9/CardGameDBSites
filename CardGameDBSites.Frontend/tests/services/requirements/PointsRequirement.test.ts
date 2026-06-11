import { describe, it, expect } from "vitest";
import PointsRequirement from "~/services/requirements/PointsRequirement";
import type { CardDetailApiModel } from "~/api/default";

function makeCard(id: number, attributes: Record<string, Array<string | number>>): CardDetailApiModel {
  return { baseId: id, displayName: `Card ${id}`, attributes };
}

describe("PointsRequirement", () => {
  const requirement = new PointsRequirement();

  describe("IsValid", () => {
    it("returns true when total points are within the range", () => {
      const cards = [
        makeCard(1, { Points: ["6"] }),
        makeCard(2, { Points: ["-3"] }),
      ];
      const config = { ability: "Points", min: 0, max: 10 };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("returns false when total points are below minimum", () => {
      const cards = [
        makeCard(1, { Points: ["-5"] }),
      ];
      const config = { ability: "Points", min: 0, max: 10 };
      expect(requirement.IsValid(cards, config)).toBe(false);
    });

    it("returns false when total points are above maximum", () => {
      const cards = [
        makeCard(1, { Points: ["15"] }),
      ];
      const config = { ability: "Points", min: 0, max: 10 };
      expect(requirement.IsValid(cards, config)).toBe(false);
    });

    it("handles numeric point values directly", () => {
      const cards = [
        makeCard(1, { Points: [6] }),
        makeCard(2, { Points: [-3] }),
      ];
      const config = { ability: "Points", min: 0, max: 10 };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("ignores cards without the points attribute", () => {
      const cards = [
        makeCard(1, { Points: ["6"] }),
        makeCard(2, { Name: ["Card 2"] }), // No Points attribute
      ];
      const config = { ability: "Points", min: 0, max: 10 };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("allows no minimum constraint", () => {
      const cards = [
        makeCard(1, { Points: ["-100"] }),
      ];
      const config = { ability: "Points", min: undefined, max: 10 };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("allows no maximum constraint", () => {
      const cards = [
        makeCard(1, { Points: ["100"] }),
      ];
      const config = { ability: "Points", min: 0, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("allows both constraints to be undefined", () => {
      const cards = [
        makeCard(1, { Points: ["-100"] }),
      ];
      const config = { ability: "Points", min: undefined, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("returns true for empty card list when no constraints", () => {
      const cards: CardDetailApiModel[] = [];
      const config = { ability: "Points", min: undefined, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("returns false for empty card list with minimum constraint", () => {
      const cards: CardDetailApiModel[] = [];
      const config = { ability: "Points", min: 1, max: 10 };
      expect(requirement.IsValid(cards, config)).toBe(false);
    });

    it("handles mixed string and numeric point values", () => {
      const cards = [
        makeCard(1, { Points: [6] }),
        makeCard(2, { Points: ["-3"] }),
        makeCard(3, { Points: [-4] }),
      ];
      const config = { ability: "Points", min: -5, max: 5 };
      expect(requirement.IsValid(cards, config)).toBe(true); // 6 - 3 - 4 = -1
    });

    it("matches the example from the issue: A with 6 points", () => {
      const cards = [
        makeCard(1, { Points: ["6"] }),
      ];
      const config = { ability: "Points", min: 0, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("matches the example from the issue: A with 6 + B with -3 = 3", () => {
      const cards = [
        makeCard(1, { Points: ["6"] }),
        makeCard(2, { Points: ["-3"] }),
      ];
      const config = { ability: "Points", min: 0, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(true);
    });

    it("matches the example from the issue: A+B+C = -1 (should fail min 0)", () => {
      const cards = [
        makeCard(1, { Points: ["6"] }),
        makeCard(2, { Points: ["-3"] }),
        makeCard(3, { Points: ["-4"] }),
      ];
      const config = { ability: "Points", min: 0, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(false);
    });

    it("matches the example from the issue: B with -3 (should fail min 0)", () => {
      const cards = [
        makeCard(2, { Points: ["-3"] }),
      ];
      const config = { ability: "Points", min: 0, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(false);
    });

    it("matches the example from the issue: B+C = -7 (should fail min 0)", () => {
      const cards = [
        makeCard(2, { Points: ["-3"] }),
        makeCard(3, { Points: ["-4"] }),
      ];
      const config = { ability: "Points", min: 0, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(false);
    });

    it("matches the example from the issue: B+C+A = -1 (should fail min 0)", () => {
      const cards = [
        makeCard(2, { Points: ["-3"] }),
        makeCard(3, { Points: ["-4"] }),
        makeCard(1, { Points: ["6"] }),
      ];
      const config = { ability: "Points", min: 0, max: undefined };
      expect(requirement.IsValid(cards, config)).toBe(false);
    });
  });

  describe("ToFilters", () => {
    it("returns undefined (points requirements cannot be easily converted to filters)", () => {
      const cards = [makeCard(1, { Points: ["6"] })];
      const config = { ability: "Points", min: 0, max: 10 };
      expect(requirement.ToFilters(cards, config)).toBeUndefined();
    });
  });
});
