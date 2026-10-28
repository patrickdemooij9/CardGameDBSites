import {
  CardSearchFilterClauseType,
  type CardsQueryFilterApiModel,
  type CardsQueryFilterClauseApiModel,
  type KeywordAttributeApiModel,
  type KeywordValuesApiModel,
} from "~/api/default";
import type { OverviewFilterItemModel } from "~/components/overviews/OverviewFilterModel";

const SEPARATOR = ":";

export const KeywordRelationLabels: Record<string, string> = {
  Has: "Has it",
  Conditional: "Has it conditionally",
  Grants: "Grants it",
  Synergy: "Synergy with it",
};

export function ToKeywordItems(values: KeywordValuesApiModel): OverviewFilterItemModel[] {
  const toItems = (group: string, names: string[]) =>
    names.map((name) => ({ DisplayName: name, Value: name, Group: group }));
  return [
    ...toItems("Triggered abilities", values.triggeredAbilities),
    ...toItems("Keywords", values.keywords),
  ];
}

export interface KeywordSelection {
  keyword: string;
  relation: string;
}

export function ToKeywordValue(keyword: string, relation: string) {
  return `${keyword}${SEPARATOR}${relation}`;
}

export function ParseKeywordValue(value: string): KeywordSelection | undefined {
  const index = value.lastIndexOf(SEPARATOR);
  if (index <= 0) return undefined;
  return { keyword: value.slice(0, index), relation: value.slice(index + 1) };
}

export function FormatKeywordValue(value: string, items: OverviewFilterItemModel[]) {
  const selection = ParseKeywordValue(value);
  if (!selection) return value;

  const keyword = items.find((it) => it.Value === selection.keyword)?.DisplayName ?? selection.keyword;
  const relation = KeywordRelationLabels[selection.relation] ?? selection.relation;
  return `${keyword} (${relation.toLowerCase()})`;
}

export function ToKeywordFilterClauses(
  values: string[],
  matchMode: "any" | "all",
  attributes: KeywordAttributeApiModel[],
): CardsQueryFilterClauseApiModel[] {
  const filters = values.flatMap<CardsQueryFilterApiModel>((value) => {
    const selection = ParseKeywordValue(value);
    const alias = attributes.find((it) => it.relation === selection?.relation)?.alias;
    return selection && alias ? [{ alias, values: [selection.keyword] }] : [];
  });
  if (filters.length === 0) return [];

  if (matchMode === "all") {
    return filters.map((filter) => ({ clauseType: CardSearchFilterClauseType.AND, filters: [filter] }));
  }
  return [{ clauseType: CardSearchFilterClauseType.AND, filters }];
}
