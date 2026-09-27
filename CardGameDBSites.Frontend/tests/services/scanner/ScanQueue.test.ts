import { describe, expect, it } from "vitest";
import type { CardVariantReferenceApiModel } from "~/api/default";
import {
  addScan,
  collectionAmounts,
  defaultVariantId,
  variantOptions,
  type ScannedCard,
  type ScanQueueEntry,
} from "~/services/scanner/ScanQueue";

const normal: CardVariantReferenceApiModel = { cardVariantId: 11, variantTypeId: null, setId: 1 };
const foil: CardVariantReferenceApiModel = { cardVariantId: 12, variantTypeId: 1, setId: 1 };
const hyperspace: CardVariantReferenceApiModel = { cardVariantId: 13, variantTypeId: 2, setId: 1 };
const reprint: CardVariantReferenceApiModel = { cardVariantId: 21, variantTypeId: null, setId: 2 };

const vader: ScannedCard = {
  baseId: 10,
  name: "Darth Vader",
  set: "Spark of Rebellion",
  imageUrl: "vader.png",
  urlSegment: "/cards/sor/darth-vader",
  imageVariantIds: [12, 11],
};

describe("defaultVariantId", () => {
  it("prefers the normal printing among the matched image's variants", () => {
    expect(defaultVariantId([12, 11], [normal, foil, hyperspace])).toBe(11);
  });

  it("falls back to the first matched variant when none is normal", () => {
    expect(defaultVariantId([13, 12], [normal, foil, hyperspace])).toBe(13);
  });
});

describe("variantOptions", () => {
  const types = [
    { id: 1, displayName: "Foil" },
    { id: 2, displayName: "Hyperspace" },
  ];

  it("lists the matched image's variants first, then the other printings in the same set", () => {
    const options = variantOptions([12, 11], [normal, foil, hyperspace, reprint], types);

    expect(options).toEqual([
      { variantId: 11, name: "Normal", matched: true },
      { variantId: 12, name: "Foil", matched: true },
      { variantId: 13, name: "Hyperspace", matched: false },
    ]);
  });

  it("uses the set of the matched printing when the image is a reprint from another set", () => {
    const options = variantOptions([21], [normal, foil, hyperspace, reprint], types);

    expect(options).toEqual([{ variantId: 21, name: "Normal", matched: true }]);
  });
});

describe("addScan", () => {
  it("adds a new entry for a card that is not in the queue", () => {
    const queue: ScanQueueEntry[] = [];

    const entry = addScan(queue, vader, 11, 1);

    expect(queue).toEqual([{ ...vader, id: entry.id, variantId: 11, amount: 1 }]);
  });

  it("adds to the entry with the same variant", () => {
    const queue: ScanQueueEntry[] = [];
    const first = addScan(queue, vader, 11, 1);

    const second = addScan(queue, vader, 11, 2);

    expect(second).toBe(first);
    expect(queue).toHaveLength(1);
    expect(first.amount).toBe(3);
  });

  it("keeps a separate entry when the user changed the earlier one to another variant", () => {
    const queue: ScanQueueEntry[] = [];
    addScan(queue, vader, 11, 1).variantId = 12;

    addScan(queue, vader, 11, 1);

    expect(queue.map((entry) => [entry.variantId, entry.amount])).toEqual([
      [11, 1],
      [12, 1],
    ]);
  });
});

describe("collectionAmounts", () => {
  it("adds the queued amounts on top of what the collection already holds", () => {
    const queue: ScanQueueEntry[] = [
      { ...vader, id: 1, variantId: 11, amount: 2 },
      { ...vader, id: 2, variantId: 12, amount: 1 },
      { ...vader, id: 3, variantId: 11, amount: 1 },
    ];
    const owned = [{ cardId: 10, variantId: 11, amount: 4 }];

    expect(collectionAmounts(queue, owned)).toEqual({ 11: 7, 12: 1 });
  });

  it("skips entries whose amount was set to zero", () => {
    const queue: ScanQueueEntry[] = [{ ...vader, id: 1, variantId: 12, amount: 0 }];

    expect(collectionAmounts(queue, [])).toEqual({});
  });
});
