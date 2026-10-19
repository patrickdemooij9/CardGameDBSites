import { CardSearchFilterClauseType, type CardsQueryFilterClauseApiModel } from "~/api/default";

export interface NumberRange {
  min: number;
  max: number;
}

const MAX_EXPANDED_VALUES = 500;

export function ParseRangeValue(value: string): NumberRange | undefined {
  const match = /^(-?\d+)(?:-(-?\d+))?$/.exec(value.trim());
  if (!match) return undefined;

  const first = Number(match[1]);
  const second = match[2] === undefined ? first : Number(match[2]);
  return { min: Math.min(first, second), max: Math.max(first, second) };
}

export function ToRangeValue(range: NumberRange) {
  return range.min === range.max ? `${range.min}` : `${range.min}-${range.max}`;
}

export function FormatRangeValue(value: string) {
  const range = ParseRangeValue(value);
  if (!range) return value;
  return range.min === range.max ? `${range.min}` : `${range.min} - ${range.max}`;
}

export function GetRangeBounds(values: string[]): NumberRange | undefined {
  const numbers = values.map(Number).filter((n) => Number.isInteger(n));
  if (numbers.length === 0) return undefined;
  return { min: Math.min(...numbers), max: Math.max(...numbers) };
}

// Ability values are indexed as text, so a range is sent as the list of whole numbers it covers.
export function ToRangeFilterClauses(alias: string, values: string[]): CardsQueryFilterClauseApiModel[] {
  const range = values.length > 0 ? ParseRangeValue(values[0]!) : undefined;
  if (!range || range.max - range.min >= MAX_EXPANDED_VALUES) return [];

  const covered: string[] = [];
  for (let i = range.min; i <= range.max; i++) {
    covered.push(i.toString());
  }

  return [
    {
      clauseType: CardSearchFilterClauseType.AND,
      filters: [{ alias, values: covered }],
    },
  ];
}
