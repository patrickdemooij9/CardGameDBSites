import { describe, expect, it } from "vitest";
import { SightingTracker } from "~/services/scanner/SightingTracker";

const vader = { baseId: 1, file: "vader.png" };
const vaderHyperspace = { baseId: 1, file: "vader-hyperspace.png" };
const luke = { baseId: 2, file: "luke.png" };

type TestCard = typeof vader;

function observeAll(tracker: SightingTracker<TestCard>, frames: TestCard[][]) {
  return frames.map((cards) => tracker.observe(cards));
}

describe("SightingTracker", () => {
  it("does not report a card before it reaches the commit threshold", () => {
    const tracker = new SightingTracker<TestCard>({ commitAfter: 3, forgetAfter: 5 });

    const events = observeAll(tracker, [[vader], [vader]]);

    expect(events).toEqual([[], []]);
    expect(tracker.isConfirmed(vader.baseId)).toBe(false);
  });

  it("reports a card once when it is confirmed, however long it stays in view", () => {
    const tracker = new SightingTracker<TestCard>({ commitAfter: 3, forgetAfter: 5 });

    const events = observeAll(tracker, [[vader], [vader], [vader], [vader], [vader]]);

    expect(events).toEqual([[], [], [{ card: vader, added: 1 }], [], []]);
    expect(tracker.isConfirmed(vader.baseId)).toBe(true);
  });

  it("tolerates missed frames shorter than the forget window", () => {
    const tracker = new SightingTracker<TestCard>({ commitAfter: 3, forgetAfter: 2 });

    const events = observeAll(tracker, [[vader], [], [vader], [], [vader]]);

    expect(events.at(-1)).toEqual([{ card: vader, added: 1 }]);
  });

  it("forgets a pending card that has been missing too long", () => {
    const tracker = new SightingTracker<TestCard>({ commitAfter: 3, forgetAfter: 2 });

    const events = observeAll(tracker, [[vader], [vader], [], [], [], [vader]]);

    expect(events.at(-1)).toEqual([]);
  });

  it("counts a card shown again after leaving the frame as another copy", () => {
    const tracker = new SightingTracker<TestCard>({ commitAfter: 2, forgetAfter: 1 });

    const events = observeAll(tracker, [[vader], [vader], [], [], [vader], [vader]]);

    expect(events.flat()).toEqual([
      { card: vader, added: 1 },
      { card: vader, added: 1 },
    ]);
  });

  it("adds the copies that are in view together", () => {
    const tracker = new SightingTracker<TestCard>({ commitAfter: 2, forgetAfter: 5 });

    const events = observeAll(tracker, [[vader], [vader, vader], [vader, vader, vader], [vader]]);

    expect(events).toEqual([[], [{ card: vader, added: 2 }], [{ card: vader, added: 1 }], []]);
  });

  it("reports the image that was matched most often", () => {
    const tracker = new SightingTracker<TestCard>({ commitAfter: 3, forgetAfter: 5 });

    const events = observeAll(tracker, [[vaderHyperspace], [vader], [vaderHyperspace]]);

    expect(events.at(-1)).toEqual([{ card: vaderHyperspace, added: 1 }]);
  });

  it("tracks cards independently", () => {
    const tracker = new SightingTracker<TestCard>({ commitAfter: 2, forgetAfter: 5 });

    const events = observeAll(tracker, [[vader], [vader, luke], [luke]]);

    expect(events).toEqual([[], [{ card: vader, added: 1 }], [{ card: luke, added: 1 }]]);
  });
});
