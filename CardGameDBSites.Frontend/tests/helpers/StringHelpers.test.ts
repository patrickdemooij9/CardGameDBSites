import { describe, it, expect } from "vitest";
import { firstCharUppercase } from "~/helpers/StringHelpers";

describe("StringHelpers", () => {
  describe("firstCharUppercase", () => {
    it("capitalizes the first character of a lowercase string", () => {
      expect(firstCharUppercase("hello")).toBe("Hello");
    });

    it("returns the same string when the first character is already uppercase", () => {
      expect(firstCharUppercase("World")).toBe("World");
    });

    it("handles a single character string", () => {
      expect(firstCharUppercase("a")).toBe("A");
    });

    it("handles a string starting with a number", () => {
      expect(firstCharUppercase("1test")).toBe("1test");
    });

    it("handles an empty string", () => {
      expect(firstCharUppercase("")).toBe("");
    });

    it("capitalizes only the first character and leaves the rest unchanged", () => {
      expect(firstCharUppercase("hELLO")).toBe("HELLO");
    });
  });
});
