import { describe, it, expect } from "vitest";
import type {
  CardDetailApiModel,
  DeckApiModel,
  DeckCardGroupApiModel,
} from "~/api/default";
import {
  bodyCards,
  buildRenderModel,
  findArtCrop,
  imageUrl,
  isTextGroup,
  type RenderCard,
} from "~/services/deckImage/model";
import {
  defaultRenderConfig,
  type DeckRenderConfig,
} from "~/services/deckImage/config";

const SWU: DeckRenderConfig = {
  ...defaultRenderConfig(),
  cardAspect: 0.716,
  landscapeTypes: ["Base", "Leader"],
  backImageTypes: ["Leader"],
  typeAttribute: "Card Type",
  costAttribute: "Cost",
  aspectAttribute: "Aspects",
  aspectColors: { Command: "#3E9F4E", Villainy: "#4B4453" },
  artCrops: [
    { typeValue: "Unit", left: 0.055, top: 0.145, right: 0.055, bottom: 0.395 },
    { typeValue: "Event", left: 0.055, top: 0.55, right: 0.055, bottom: 0.07 },
  ],
  eyebrow: null,
};

/** Shatterpoint: different attribute keys entirely, and no bands. */
const SHATTERPOINT: DeckRenderConfig = {
  ...defaultRenderConfig(),
  cardAspect: 0.669,
  typeAttribute: "Unit Type",
  costAttribute: "Squad Points",
  aspectAttribute: null,
  artCrops: [],
  eyebrow: "Strike Team",
};

function card(
  baseId: number,
  displayName: string,
  attributes: Record<string, string[]>,
): CardDetailApiModel {
  return {
    baseId,
    displayName,
    attributes,
    imageUrl: { url: `/media/${baseId}/front.png` },
    backImageUrl: { url: `/media/${baseId}/back.png` },
  };
}

function renderCard(overrides: Partial<RenderCard> = {}): RenderCard {
  return {
    cardId: 1,
    name: "X",
    shortName: "X",
    amount: 1,
    groupId: 1,
    slotId: 0,
    type: "Unit",
    cost: "",
    aspects: [],
    landscape: false,
    faceUrl: "/media/1/front.png",
    cropUrl: null,
    ...overrides,
  };
}

describe("imageUrl", () => {
  it("adds sizing params and preserves an existing query string", () => {
    expect(imageUrl("/media/a.png?v=123", { width: 300 })).toBe(
      "/media/a.png?v=123&width=300&format=webp&quality=80",
    );
  });

  it("starts a query string when the URL has none", () => {
    expect(imageUrl("/media/a.png", { width: 200 })).toBe(
      "/media/a.png?width=200&format=webp&quality=80",
    );
  });

  it("emits the crop band as ImageSharp inset fractions", () => {
    const url = imageUrl("/media/a.png", {
      width: 200,
      crop: {
        typeValue: "Unit",
        left: 0.055,
        top: 0.145,
        right: 0.055,
        bottom: 0.395,
      },
    });
    expect(url).toContain("cc=0.055,0.145,0.055,0.395");
  });

  it("rounds fractional widths, which ImageSharp rejects", () => {
    expect(imageUrl("/media/a.png", { width: 180.8 })).toContain("width=181");
  });
});

describe("findArtCrop", () => {
  it("matches a band by card type", () => {
    expect(findArtCrop(SWU, "Event")?.top).toBe(0.55);
  });

  it("returns null for a type with no band, so it falls back to the full face", () => {
    expect(findArtCrop(SWU, "Upgrade")).toBeNull();
  });

  it("returns null for an empty type", () => {
    expect(findArtCrop(SWU, "")).toBeNull();
  });
});

describe("isTextGroup", () => {
  it("is true when no card in the group carries a type", () => {
    expect(isTextGroup([renderCard({ type: "" })], SWU)).toBe(true);
  });

  it("is false when any card carries a type", () => {
    expect(
      isTextGroup([renderCard({ type: "" }), renderCard({ type: "Unit" })], SWU),
    ).toBe(false);
  });

  it("is false for every group when the game configures no type attribute", () => {
    const noTypes = { ...defaultRenderConfig(), typeAttribute: null };
    expect(isTextGroup([renderCard({ type: "" })], noTypes)).toBe(false);
  });
});

describe("buildRenderModel", () => {
  const groupings: DeckCardGroupApiModel[] = [
    { header: "Deck", groupId: 1 },
    { header: "Sideboard", groupId: 2 },
  ];

  const deck: DeckApiModel = {
    id: 42,
    name: "Knowledge and Defense",
    cards: [
      { cardId: 1, groupId: 1, slotId: 0, amount: 1 },
      { cardId: 2, groupId: 1, slotId: 1, amount: 3 },
      { cardId: 3, groupId: 1, slotId: 2, amount: 2 },
    ],
  };

  const cards = [
    card(1, "Kylo Ren, We're not done yet", {
      "Card Type": ["Leader"],
      Aspects: ["Villainy"],
      Cost: ["7"],
    }),
    card(2, "Bith Brute", {
      "Card Type": ["Unit"],
      Aspects: ["Command"],
      Cost: ["3"],
    }),
    card(3, "Bounty Posting", { "Card Type": ["Event"], Cost: ["1"] }),
  ];

  const build = (overrides: Partial<Parameters<typeof buildRenderModel>[0]> = {}) =>
    buildRenderModel({
      deck,
      cards,
      groupings,
      heroCardIds: [1],
      config: SWU,
      deckUrl: "sw-unlimited-db.com/decks/42",
      ...overrides,
    });

  it("separates hero cards from the body", () => {
    const model = build();
    expect(model.heroCards.map((c) => c.cardId)).toEqual([1]);
    expect(bodyCards(model).map((c) => c.cardId)).toEqual([2, 3]);
  });

  it("resolves the back image for types that display their back face", () => {
    const model = build();
    expect(model.heroCards[0]!.faceUrl).toBe("/media/1/back.png");
    expect(bodyCards(model)[0]!.faceUrl).toBe("/media/2/front.png");
  });

  it("marks configured types as landscape", () => {
    expect(build().heroCards[0]!.landscape).toBe(true);
    expect(bodyCards(build())[0]!.landscape).toBe(false);
  });

  it("sets a crop URL only for types with a band", () => {
    const [bith, bounty] = bodyCards(build());
    expect(bith!.cropUrl).not.toBeNull(); // Unit has a band
    expect(bounty!.cropUrl).not.toBeNull(); // Event has a band
  });

  it("leaves the crop URL null for a type with no band", () => {
    const model = build({
      config: { ...SWU, artCrops: [SWU.artCrops[0]!] }, // Unit only
    });
    expect(bodyCards(model).find((c) => c.type === "Event")!.cropUrl).toBeNull();
  });

  it("counts unique and total copies separately", () => {
    const model = build();
    expect(model.uniqueCount).toBe(3);
    expect(model.totalCount).toBe(6);
  });

  it("takes group headers from the deck type settings", () => {
    expect(build().groups[0]!.header).toBe("Deck");
  });

  it("shortens names at the first comma", () => {
    expect(build().heroCards[0]!.shortName).toBe("Kylo Ren");
  });

  it("collects distinct aspect colours as pips", () => {
    expect(build().aspectPips).toEqual(["#4B4453", "#3E9F4E"]);
  });

  it("skips deck entries whose card failed to load", () => {
    const model = build({ cards: cards.filter((c) => c.baseId !== 3) });
    expect(model.uniqueCount).toBe(2);
  });

  it("reads a different game's attribute keys without any renderer change", () => {
    const spDeck: DeckApiModel = {
      id: 7,
      name: "Hello There!",
      cards: [
        { cardId: 10, groupId: 1, slotId: 0, amount: 1 },
        { cardId: 11, groupId: 2, slotId: 0, amount: 1 },
      ],
    };
    const spCards = [
      card(10, "Clone Sergeant Hunter", {
        "Unit Type": ["Primary"],
        "Squad Points": ["8"],
      }),
      // A mission carries no Unit Type at all.
      card(11, "Sabotage Showdown", {}),
    ];

    const model = buildRenderModel({
      deck: spDeck,
      cards: spCards,
      groupings: [{ header: "Squad 1", groupId: 1 }],
      heroCardIds: [],
      config: SHATTERPOINT,
      deckUrl: "shatterpointdb.com/strike-teams/7",
    });

    expect(model.eyebrow).toBe("Strike Team");
    expect(bodyCards(model).map((c) => c.cost)).toEqual(["8"]);
    expect(model.textCards.map((c) => c.shortName)).toEqual([
      "Sabotage Showdown",
    ]);
    expect(model.aspectPips).toEqual([]);
  });
});
