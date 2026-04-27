import { GetCardValue } from "~/helpers/CardHelper";
import type { CreateDeckModel } from "~/components/decks/deckBuilder/models/CreateDeckModel";

export interface CostCurveEntry {
  label: string;
  count: number;
  heightPercent: number;
}

export interface CostCurveTypeDataset {
  label: string;
  data: number[];
}

export interface CostCurveChartData {
  labels: string[];
  datasets: CostCurveTypeDataset[];
}

const COST_OVERFLOW_THRESHOLD = 9;
const COST_OVERFLOW_LABEL = "9+";

function toCostLabel(raw: string): string {
  const num = Number(raw);
  if (!isNaN(num) && num >= COST_OVERFLOW_THRESHOLD) return COST_OVERFLOW_LABEL;
  return raw;
}

function sortCostLabels(labels: string[]): string[] {
  return [...labels].sort((a, b) => {
    if (a === COST_OVERFLOW_LABEL) return 1;
    if (b === COST_OVERFLOW_LABEL) return -1;
    const numA = Number(a);
    const numB = Number(b);
    if (!isNaN(numA) && !isNaN(numB)) return numA - numB;
    return a.localeCompare(b);
  });
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
    count: counts[key]!,
    heightPercent: max > 0 ? Math.round((counts[key]! / max) * 100) : 0,
  }));
}

const DEFAULT_COST_LABELS: string[] = [
  "0", "1", "2", "3", "4", "5", "6", "7", "8", COST_OVERFLOW_LABEL,
];

export function computeCostCurveChart(
  deck: CreateDeckModel,
  costAttr = "Cost",
  typeAttr?: string,
): CostCurveChartData {
  const typeLabelSet = new Set<string>();
  const counts: Record<string, Record<string, number>> = {};

  deck.getCards().forEach(({ card, amount }) => {
    const cost = GetCardValue<string>(card, costAttr);
    if (cost === null || cost === undefined) return;

    const costLabel = toCostLabel(String(cost));
    const typeLabel =
      typeAttr ? (GetCardValue<string>(card, typeAttr) ?? "Unknown") : "Cards";

    typeLabelSet.add(typeLabel);

    if (!counts[typeLabel]) counts[typeLabel] = {};
    counts[typeLabel][costLabel] = (counts[typeLabel][costLabel] ?? 0) + amount;
  });

  const labels = DEFAULT_COST_LABELS;
  const types = typeLabelSet.size > 0 ? Array.from(typeLabelSet).sort() : ["Cards"];

  const datasets: CostCurveTypeDataset[] = types.map((type) => ({
    label: type,
    data: labels.map((label) => counts[type]?.[label] ?? 0),
  }));

  return { labels, datasets };
}
