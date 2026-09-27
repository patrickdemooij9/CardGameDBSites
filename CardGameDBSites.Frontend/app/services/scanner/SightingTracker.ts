interface ScannableCard {
  baseId: number;
  file: string;
}

interface ImageHits<T> {
  card: T;
  hits: number;
}

interface PendingSighting<T> {
  hits: number;
  lastFrame: number;
  images: Map<string, ImageHits<T>>;
  reportedCopies: number;
}

export interface SightingTrackerOptions {
  commitAfter: number;
  forgetAfter: number;
}

export interface ScanEvent<T> {
  card: T;
  added: number;
}

export class SightingTracker<T extends ScannableCard> {
  private frame = 0;
  private readonly pending = new Map<number, PendingSighting<T>>();

  constructor(private readonly options: SightingTrackerOptions) {}

  observe(cards: T[]): ScanEvent<T>[] {
    this.frame++;
    this.forgetStale();

    const copiesInView = new Map<number, number>();
    for (const card of cards) {
      copiesInView.set(card.baseId, (copiesInView.get(card.baseId) ?? 0) + 1);
      this.recordImage(card);
    }

    const events: ScanEvent<T>[] = [];
    for (const [baseId, copies] of copiesInView) {
      const sighting = this.pending.get(baseId)!;
      sighting.hits++;
      sighting.lastFrame = this.frame;

      if (sighting.hits >= this.options.commitAfter && copies > sighting.reportedCopies) {
        events.push({ card: this.mostMatchedImage(sighting), added: copies - sighting.reportedCopies });
        sighting.reportedCopies = copies;
      }
    }
    return events;
  }

  isConfirmed(baseId: number) {
    return (this.pending.get(baseId)?.hits ?? 0) >= this.options.commitAfter;
  }

  private recordImage(card: T) {
    let sighting = this.pending.get(card.baseId);
    if (!sighting) {
      sighting = { hits: 0, lastFrame: this.frame, images: new Map(), reportedCopies: 0 };
      this.pending.set(card.baseId, sighting);
    }
    const image = sighting.images.get(card.file) ?? { card, hits: 0 };
    image.hits++;
    sighting.images.set(card.file, image);
  }

  private mostMatchedImage(sighting: PendingSighting<T>) {
    let best: ImageHits<T> | undefined;
    for (const image of sighting.images.values()) {
      if (!best || image.hits > best.hits) {
        best = image;
      }
    }
    return best!.card;
  }

  private forgetStale() {
    for (const [baseId, sighting] of this.pending) {
      const missedFrames = this.frame - sighting.lastFrame - 1;
      if (missedFrames > this.options.forgetAfter) {
        this.pending.delete(baseId);
      }
    }
  }
}
