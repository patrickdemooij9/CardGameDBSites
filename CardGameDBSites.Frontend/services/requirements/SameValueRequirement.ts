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

  private getIntersection(cards: CardDetailApiModel[], ability: string): string[] | undefined {
    let intersection: string[] | null = null;
    for (const card of cards) {
      const values = card.attributes?.[ability] as string[] | undefined;
      if (!values || values.length === 0) return undefined;

      if (intersection === null) {
        intersection = [...values];
      } else {
        intersection = intersection.filter((v) => values.includes(v));
      }
    }
    return intersection ?? undefined;
  }

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    if (cards.length === 0) return true;
    const intersection = this.getIntersection(cards, config.ability);
    return (intersection?.length ?? 0) > 0;
  }

  ToFilters(cards: CardDetailApiModel[], config: Record<string, any>): CardsQueryFilterClauseApiModel[] | undefined {
    if (cards.length === 0) return undefined;
    const intersection = this.getIntersection(cards, config.ability);
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
