import { describe, it, expect, beforeEach, vi } from "vitest";
import { ToHumanReadableText, ParseToHumanReadableText } from "~/app/helpers/DateHelper";

describe("DateHelper", () => {
  describe("ToHumanReadableText", () => {
    it("returns 'Today' when the date is today", () => {
      const today = new Date();
      expect(ToHumanReadableText(today)).toBe("Today");
    });

    it("returns 'Yesterday' when the date was yesterday", () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      expect(ToHumanReadableText(yesterday)).toBe("Yesterday");
    });

    it("returns '{N} days ago' when the date was N days ago (2–30)", () => {
      const daysAgo = new Date();
      daysAgo.setDate(daysAgo.getDate() - 15);
      expect(ToHumanReadableText(daysAgo)).toBe("15 days ago");
    });

    it("returns '{N} days ago' at 30 days boundary", () => {
      const daysAgo = new Date();
      daysAgo.setDate(daysAgo.getDate() - 30);
      expect(ToHumanReadableText(daysAgo)).toBe("30 days ago");
    });

    it("returns 'Month Day' format for dates older than 30 days in current year", () => {
      const now = new Date();
      const oldDate = new Date(now.getFullYear(), 0, 1); // Jan 1 of this year
      const result = ToHumanReadableText(oldDate);
      expect(result).toBe("January 1");
    });

    it("includes the year for dates in a previous year", () => {
      const lastYear = new Date();
      lastYear.setFullYear(lastYear.getFullYear() - 1);
      lastYear.setMonth(5); // June
      lastYear.setDate(15);
      const result = ToHumanReadableText(lastYear);
      expect(result).toContain(String(lastYear.getFullYear()));
      expect(result).toContain("June");
      expect(result).toContain("15");
    });
  });

  describe("ParseToHumanReadableText", () => {
    it("parses an ISO date string and returns human readable text", () => {
      const today = new Date();
      const isoString = today.toISOString();
      expect(ParseToHumanReadableText(isoString)).toBe("Today");
    });

    it("parses a date string from yesterday", () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      expect(ParseToHumanReadableText(yesterday.toISOString())).toBe("Yesterday");
    });
  });
});
