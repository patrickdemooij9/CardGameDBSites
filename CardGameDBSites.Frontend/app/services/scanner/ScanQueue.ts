import type {
  CardVariantReferenceApiModel,
  CardVariantTypeApiModel,
  CollectionCardApiModel,
} from "~/api/default";

export interface ScannedCard {
  baseId: number;
  name: string;
  set: string;
  imageUrl: string;
  urlSegment: string;
  imageVariantIds: number[];
}

export interface ScanQueueEntry extends ScannedCard {
  id: number;
  variantId: number;
  amount: number;
}

export interface VariantOption {
  variantId: number;
  name: string;
  matched: boolean;
}

let nextEntryId = 1;

export function defaultVariantId(
  imageVariantIds: number[],
  variants: CardVariantReferenceApiModel[]
): number | undefined {
  const normal = variants.find(
    (variant) => variant.variantTypeId == null && imageVariantIds.includes(variant.cardVariantId!)
  );
  return normal?.cardVariantId ?? imageVariantIds[0];
}

export function variantOptions(
  imageVariantIds: number[],
  variants: CardVariantReferenceApiModel[],
  variantTypes: Pick<CardVariantTypeApiModel, "id" | "displayName">[]
): VariantOption[] {
  // Reprints and promos share a baseId with the original, so the matched image decides the set, not the card.
  const matchedSets = new Set(
    variants.filter((variant) => imageVariantIds.includes(variant.cardVariantId!)).map((variant) => variant.setId)
  );
  const options = variants
    .filter((variant) => matchedSets.has(variant.setId))
    .map((variant) => ({
      variantId: variant.cardVariantId!,
      name:
        variant.variantTypeId == null
          ? "Normal"
          : (variantTypes.find((type) => type.id === variant.variantTypeId)?.displayName ?? "Unknown"),
      matched: imageVariantIds.includes(variant.cardVariantId!),
    }));
  return [...options.filter((option) => option.matched), ...options.filter((option) => !option.matched)];
}

export function addScan(queue: ScanQueueEntry[], card: ScannedCard, variantId: number, amount: number) {
  const existing = queue.find((entry) => entry.baseId === card.baseId && entry.variantId === variantId);
  if (existing) {
    existing.amount += amount;
    return existing;
  }

  const entry: ScanQueueEntry = { ...card, id: nextEntryId++, variantId, amount };
  queue.unshift(entry);
  return entry;
}

export function collectionAmounts(entries: ScanQueueEntry[], owned: CollectionCardApiModel[]) {
  const amounts: Record<number, number> = {};
  for (const entry of entries) {
    if (entry.amount <= 0) {
      continue;
    }
    if (amounts[entry.variantId] === undefined) {
      amounts[entry.variantId] = owned.find((card) => card.variantId === entry.variantId)?.amount ?? 0;
    }
    amounts[entry.variantId]! += entry.amount;
  }
  return amounts;
}
