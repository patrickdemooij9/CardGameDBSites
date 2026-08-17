import { describe, it, expect } from "vitest";
import type { CardDetailApiModel, DeckApiModel } from "~/api/default";
import { planLayout, type Op } from "~/services/deckImage/renderer";
import {
  bodyCards,
  buildRenderModel,
  sameOriginMedia,
  type DeckRenderModel,
} from "~/services/deckImage/model";
import {
  defaultRenderConfig,
  type DeckRenderConfig,
} from "~/services/deckImage/config";
import {
  CANVAS_WIDTH,
  minCanvasHeight,
  type DeckImageTier,
} from "~/services/deckImage/tiers";

/**
 * planLayout only needs font assignment and text measurement, so a stub context keeps
 * these tests in the node environment alongside the rest of the suite.
 */
function stubContext(): CanvasRenderingContext2D {
  return {
    font: "400 16px sans-serif",
    measureText(text: string) {
      // Width has to track the font size, or nothing ever appears to overflow.
      const size = Number(/(\d+(?:\.\d+)?)px/.exec(this.font)?.[1] ?? 16);
      return { width: text.length * size * 0.5 };
    },
  } as unknown as CanvasRenderingContext2D;
}

const SWU: DeckRenderConfig = {
  ...defaultRenderConfig(),
  cardAspect: 0.716,
  landscapeTypes: ["Base", "Leader"],
  backImageTypes: ["Leader"],
  typeAttribute: "Card Type",
  costAttribute: "Cost",
  aspectAttribute: "Aspects",
  aspectColors: { Command: "#3E9F4E" },
  artCrops: [
    { typeValue: "Unit", left: 0.055, top: 0.145, right: 0.055, bottom: 0.395 },
  ],
};

const SHATTERPOINT: DeckRenderConfig = {
  ...defaultRenderConfig(),
  cardAspect: 0.669,
  typeAttribute: "Unit Type",
  costAttribute: "Squad Points",
  artCrops: [],
  eyebrow: "Strike Team",
};

function makeCard(id: number, type: string): CardDetailApiModel {
  return {
    baseId: id,
    displayName: `Card ${id}`,
    attributes: { "Card Type": [type], Cost: ["3"], Aspects: ["Command"] },
    imageUrl: { url: `/media/${id}/front.png` },
    backImageUrl: { url: `/media/${id}/back.png` },
  };
}

/** A flat SWU-shaped deck of `count` units plus a leader and a base. */
function flatDeck(count: number) {
  const cards: CardDetailApiModel[] = [
    makeCard(1, "Leader"),
    makeCard(2, "Base"),
    ...Array.from({ length: count }, (_, i) => makeCard(100 + i, "Unit")),
  ];
  const deck: DeckApiModel = {
    id: 1,
    name: "Test deck",
    cards: cards.map((c) => ({
      cardId: c.baseId,
      groupId: 1,
      slotId: 0,
      amount: 1,
    })),
  };
  return buildRenderModel({
    deck,
    cards,
    groupings: [{ header: "Deck", groupId: 1 }],
    heroCardIds: [1, 2],
    config: SWU,
    deckUrl: "example.com/decks/1",
  });
}

/** A Shatterpoint-shaped deck: `squads` groups of three plus a mission text group. */
function squadDeck(squads: number) {
  const cards: CardDetailApiModel[] = [];
  const deckCards: DeckApiModel["cards"] = [];
  let id = 1;

  for (let squad = 1; squad <= squads; squad++) {
    for (const role of ["Primary", "Secondary", "Support"]) {
      cards.push({
        baseId: id,
        displayName: `Unit ${id}`,
        attributes: { "Unit Type": [role], "Squad Points": ["4"] },
        imageUrl: { url: `/media/${id}/front.png` },
      });
      deckCards.push({ cardId: id, groupId: squad, slotId: 0, amount: 1 });
      id++;
    }
  }

  // The mission carries no type attribute at all — that is what marks it as a text group.
  cards.push({
    baseId: id,
    displayName: "Sabotage Showdown",
    attributes: {},
    imageUrl: { url: `/media/${id}/front.png` },
  });
  deckCards.push({ cardId: id, groupId: squads + 1, slotId: 0, amount: 1 });

  return buildRenderModel({
    deck: { id: 2, name: "Strike team", cards: deckCards },
    cards,
    groupings: Array.from({ length: squads }, (_, i) => ({
      header: `Squad ${i + 1}`,
      groupId: i + 1,
    })),
    heroCardIds: [],
    config: SHATTERPOINT,
    deckUrl: "example.com/strike-teams/2",
  });
}

/** Cards actually placed on the canvas, however they are drawn. */
function placedCardIds(ops: Op[]): number[] {
  return ops
    .filter((op) => op.kind === "card" || op.kind === "listRow")
    .map((op) => (op as Extract<Op, { kind: "card" | "listRow" }>).card.cardId);
}

describe("planLayout", () => {
  const tiers: DeckImageTier[] = ["fullFace", "artCrop", "list"];

  it.each(tiers)(
    "places every hero and body card exactly once on the %s tier",
    (tier) => {
      const model = flatDeck(40);
      const plan = planLayout(stubContext(), model, tier);

      const expected = [
        ...model.heroCards.map((c) => c.cardId),
        ...bodyCards(model).map((c) => c.cardId),
      ].sort((a, b) => a - b);

      expect(placedCardIds(plan.ops).sort((a, b) => a - b)).toEqual(expected);
    },
  );

  it("keeps the hero cards on the densest tier rather than dropping them", () => {
    const model = flatDeck(86);
    const plan = planLayout(stubContext(), model, "artCrop");
    for (const hero of model.heroCards) {
      expect(placedCardIds(plan.ops)).toContain(hero.cardId);
    }
  });

  it("does not draw text-group cards as images", () => {
    const model = squadDeck(2);
    const plan = planLayout(stubContext(), model, "fullFace");

    expect(model.textCards).toHaveLength(1);
    expect(placedCardIds(plan.ops)).not.toContain(model.textCards[0]!.cardId);
    expect(plan.ops.some((op) => op.kind === "mission")).toBe(true);
  });

  it("emits one section label per squad in the grouped layout", () => {
    const plan = planLayout(stubContext(), squadDeck(4), "fullFace");
    const labels = plan.ops.filter((op) => op.kind === "sectionLabel");
    expect(labels).toHaveLength(4);
  });

  it("counts squads rather than unique cards in the grouped metadata line", () => {
    const plan = planLayout(stubContext(), squadDeck(3), "fullFace");
    expect(plan.metaText).toContain("3 squads");
  });

  it("grows taller as the deck grows", () => {
    const small = planLayout(stubContext(), flatDeck(10), "fullFace").height;
    const large = planLayout(stubContext(), flatDeck(80), "fullFace").height;
    expect(large).toBeGreaterThan(small);
  });

  it("never renders shorter than the minimum canvas", () => {
    const plan = planLayout(stubContext(), squadDeck(2), "list");
    expect(plan.height).toBeGreaterThanOrEqual(minCanvasHeight(SHATTERPOINT));
  });

  it("keeps every placed card inside the canvas width", () => {
    for (const tier of tiers) {
      const plan = planLayout(stubContext(), flatDeck(60), tier);
      for (const op of plan.ops) {
        if (op.kind !== "card") continue;
        expect(op.x).toBeGreaterThanOrEqual(0);
        expect(op.x + op.w).toBeLessThanOrEqual(CANVAS_WIDTH);
      }
    }
  });

  it("wraps a long title instead of letting it overflow", () => {
    const model: DeckRenderModel = {
      ...flatDeck(5),
      title: "An extremely long deck name that will not fit on one single line",
    };
    const plan = planLayout(stubContext(), model, "fullFace");
    expect(plan.titleLines.length).toBeGreaterThan(1);
  });
});

describe("sameOriginMedia", () => {
  it("returns the URL unchanged outside a browser", () => {
    // `window` is undefined in the node test environment.
    expect(sameOriginMedia("https://api.example.com/media/a/b.png")).toBe(
      "https://api.example.com/media/a/b.png",
    );
  });
});
