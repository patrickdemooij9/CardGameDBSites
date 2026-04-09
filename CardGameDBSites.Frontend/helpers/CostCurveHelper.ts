import { GetCardValue } from "~/helpers/CardHelper";
import type { CreateDeckModel } from "~/components/decks/deckBuilder/models/CreateDeckModel";

export interface CostCurveEntry {
  label: string;
  count: number;
  heightPercent: number;
}

export function computeCostCurve(
  deck: CreateDeckModel,
  costAttr = "Cost",
): CostCurveEntry[] {
  const counts: Record<string, number> = {};

  deck.getCards().forEach(({ card, amount }) => {
    const cost = GetCardValue<string>(card, costAttr);
    if (cost === null || cost === undefined) return;
    const key = String(cost);
    counts[key] = (counts[key] ?? 0) + amount;
  });

  if (Object.keys(counts).length === 0) return [];

  const sortedKeys = Object.keys(counts).sort((a, b) => {
    const numA = Number(a);
    const numB = Number(b);
    if (!isNaN(numA) && !isNaN(numB)) return numA - numB;
    return a.localeCompare(b);
  });

  const max = Math.max(...Object.values(counts));

  return sortedKeys.map((key) => ({
    label: key,
    count: counts[key],
    heightPercent: max > 0 ? Math.round((counts[key] / max) * 100) : 0,
  }));
}
