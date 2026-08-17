import { describe, it, expect } from "vitest";
import {
  ART_CROP_ASPECT,
  CANVAS_WIDTH,
  availableTiers,
  columnsFor,
  gridSpec,
  groupRowSpec,
  isStructured,
  minCanvasHeight,
  selectLayout,
  selectTier,
} from "~/services/deckImage/tiers";
import {
  defaultRenderConfig,
  type ArtCrop,
  type DeckRenderConfig,
} from "~/services/deckImage/config";

const band: ArtCrop = {
  typeValue: "Unit",
  left: 0.055,
  top: 0.145,
  right: 0.055,
  bottom: 0.395,
};

/** A game with art bands authored, i.e. Star Wars Unlimited. */
function withCrops(overrides: Partial<DeckRenderConfig> = {}): DeckRenderConfig {
  return { ...defaultRenderConfig(), artCrops: [band], ...overrides };
}

/** A game with none, i.e. Shatterpoint or Skytear Horde. */
function withoutCrops(
  overrides: Partial<DeckRenderConfig> = {},
): DeckRenderConfig {
  return { ...defaultRenderConfig(), artCrops: [], ...overrides };
}

describe("isStructured", () => {
  it("is false for a single flat group", () => {
    expect(isStructured([88])).toBe(false);
    expect(isStructured([23])).toBe(false);
  });

  it("is true for several small groups", () => {
    expect(isStructured([3, 3])).toBe(true); // Shatterpoint squad
    expect(isStructured([3, 3, 3])).toBe(true); // duel
    expect(isStructured([3, 3, 3, 3])).toBe(true); // large
  });

  it("is false when any group is too big to read as a row", () => {
    expect(isStructured([3, 7])).toBe(false);
  });

  it("is false for no groups at all", () => {
    expect(isStructured([])).toBe(false);
  });

  it("ignores empty groups rather than treating them as rows", () => {
    expect(isStructured([3, 0])).toBe(false);
  });

  it("maps onto the layout choice", () => {
    expect(selectLayout([3, 3])).toBe("groupRows");
    expect(selectLayout([36])).toBe("flatGrid");
  });
});

describe("availableTiers", () => {
  it("omits the art-crop tier when the game has no bands authored", () => {
    expect(availableTiers(withoutCrops())).toEqual(["fullFace", "list"]);
  });

  it("includes it when bands exist", () => {
    expect(availableTiers(withCrops())).toEqual([
      "fullFace",
      "artCrop",
      "list",
    ]);
  });
});

describe("selectTier", () => {
  const swu = withCrops();

  it("uses full faces for small and mid-size decks", () => {
    expect(selectTier(7, swu)).toBe("fullFace"); // Shatterpoint squad
    expect(selectTier(23, swu)).toBe("fullFace"); // SWU Premier median
    expect(selectTier(45, swu)).toBe("fullFace");
  });

  it("switches to art crops at the threshold", () => {
    expect(selectTier(46, swu)).toBe("artCrop");
    expect(selectTier(83, swu)).toBe("artCrop"); // Twin Suns median
    expect(selectTier(140, swu)).toBe("artCrop");
  });

  it("forces the list past the upper threshold", () => {
    expect(selectTier(141, swu)).toBe("list");
    expect(selectTier(254, swu)).toBe("list"); // observed Twin Suns maximum
  });

  it("never picks art crops for a game without bands", () => {
    const shatterpoint = withoutCrops();
    expect(selectTier(83, shatterpoint)).toBe("fullFace");
    expect(selectTier(140, shatterpoint)).toBe("fullFace");
  });

  it("still forces the list past the upper threshold without bands", () => {
    expect(selectTier(141, withoutCrops())).toBe("list");
  });

  it("respects per-deck-type threshold overrides", () => {
    const eager = withCrops({ tierBMin: 20, tierCMin: 50 });
    expect(selectTier(23, eager)).toBe("artCrop");
    expect(selectTier(60, eager)).toBe("list");
  });

  it("honours a player override when that tier is available", () => {
    expect(selectTier(23, swu, "list")).toBe("list");
    expect(selectTier(88, swu, "fullFace")).toBe("fullFace");
  });

  it("ignores an override the game cannot produce", () => {
    expect(selectTier(23, withoutCrops(), "artCrop")).toBe("fullFace");
  });
});

describe("columnsFor", () => {
  it("widens the art-crop grid for dense decks", () => {
    expect(columnsFor("artCrop", 40)).toBe(5);
    expect(columnsFor("artCrop", 88)).toBe(7);
  });

  it("adds a third list column for long decks", () => {
    expect(columnsFor("list", 20)).toBe(2);
    expect(columnsFor("list", 88)).toBe(3);
  });

  it("keeps full faces at five columns regardless of count", () => {
    expect(columnsFor("fullFace", 7)).toBe(5);
    expect(columnsFor("fullFace", 88)).toBe(5);
  });
});

describe("gridSpec", () => {
  it("fits the columns and gaps inside the canvas", () => {
    const spec = gridSpec("fullFace", 36, 0.716);
    const used =
      spec.cellWidth * spec.columns + spec.gap * (spec.columns - 1) + 60 * 2;
    expect(Math.round(used)).toBe(CANVAS_WIDTH);
  });

  it("derives full-face cell height from the game's card aspect", () => {
    const swu = gridSpec("fullFace", 36, 0.716);
    const shatterpoint = gridSpec("fullFace", 13, 0.669);

    expect(swu.cellHeight).toBeCloseTo(swu.cellWidth / 0.716, 5);
    // Narrower cards are taller in the same column width.
    expect(shatterpoint.cellHeight).toBeGreaterThan(swu.cellHeight);
  });

  it("uses the uniform crop aspect rather than the card aspect for art crops", () => {
    const spec = gridSpec("artCrop", 88, 0.716);
    expect(spec.cellHeight).toBeCloseTo(spec.cellWidth / ART_CROP_ASPECT, 5);
  });

  it("produces shorter cells for art crops than for full faces", () => {
    const crop = gridSpec("artCrop", 88, 0.716);
    const face = gridSpec("fullFace", 88, 0.716);
    expect(crop.cellHeight).toBeLessThan(face.cellHeight);
  });
});

describe("groupRowSpec", () => {
  it("shrinks cards as squads stack up", () => {
    const two = groupRowSpec(2, 0.669);
    const four = groupRowSpec(4, 0.669);
    expect(two.cardWidth).toBeGreaterThan(four.cardWidth);
  });

  it("clamps to the readable range at both extremes", () => {
    expect(groupRowSpec(1, 0.669).cardWidth).toBe(300);
    expect(groupRowSpec(12, 0.669).cardWidth).toBe(210);
  });

  it("keeps three cards plus gaps inside the canvas", () => {
    for (const groups of [2, 3, 4]) {
      const spec = groupRowSpec(groups, 0.669);
      const used = spec.cardWidth * 3 + spec.gap * 2 + 60 * 2;
      expect(used).toBeLessThanOrEqual(CANVAS_WIDTH);
    }
  });
});

describe("minCanvasHeight", () => {
  it("never lets output be wider than it is tall", () => {
    expect(minCanvasHeight(defaultRenderConfig())).toBe(CANVAS_WIDTH);
  });

  it("ignores a ratio below square, which would reintroduce banners", () => {
    expect(minCanvasHeight(withoutCrops({ minCanvasRatio: 0.5 }))).toBe(
      CANVAS_WIDTH,
    );
  });

  it("honours a taller minimum", () => {
    expect(minCanvasHeight(withoutCrops({ minCanvasRatio: 1.25 }))).toBe(1350);
  });
});
