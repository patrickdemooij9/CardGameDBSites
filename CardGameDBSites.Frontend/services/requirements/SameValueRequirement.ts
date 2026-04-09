import {
  CardSearchFilterClauseType,
  CardSearchFilterMode,
  type CardDetailApiModel,
  type CardsQueryFilterClauseApiModel,
} from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";

export default class SameValueRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.SameValue;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    if (cards.length === 0) return true;

    let intersection: string[] | null = null;
    for (const card of cards) {
      const values = card.attributes?.[config.ability] as string[] | undefined;
      if (!values) return false;

      if (intersection === null) {
        intersection = [...values];
      } else {
        intersection = intersection.filter((v) => values.includes(v));
      }
    }
    return (intersection?.length ?? 0) > 0;
  }

  ToFilters(cards: CardDetailApiModel[], config: Record<string, any>): CardsQueryFilterClauseApiModel[] | undefined {
    if (cards.length === 0) return undefined;

    let intersection: string[] | null = null;
    for (const card of cards) {
      const values = card.attributes?.[config.ability] as string[] | undefined;
      if (!values) return undefined;

      if (intersection === null) {
        intersection = [...values];
      } else {
        intersection = intersection.filter((v) => values.includes(v));
      }
    }

    if (!intersection || intersection.length === 0) return undefined;

    return [{
      clauseType: CardSearchFilterClauseType.AND,
      filters: [
        {
          alias: config.ability,
          values: intersection,
          mode: CardSearchFilterMode.CONTAINS,
        },
      ],
    }];
  }
}
