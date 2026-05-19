import {
  CardSearchFilterClauseType,
  CardSearchFilterMode,
  type CardDetailApiModel,
  type CardsQueryFilterClauseApiModel,
} from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";

export default class ChildOfRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.ChildOf;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    const allowedChildIds: number[] = config.allowedChildIds ?? [];
    return cards.every((card) => card.baseId !== undefined && allowedChildIds.includes(card.baseId));
  }

  ToFilters(_cards: CardDetailApiModel[], config: Record<string, any>): CardsQueryFilterClauseApiModel[] | undefined {
    const allowedChildIds: number[] = config.allowedChildIds ?? [];
    if (allowedChildIds.length === 0) {
      return undefined;
    }
    return [
      {
        clauseType: CardSearchFilterClauseType.AND,
        filters: [
          {
            alias: "baseId",
            values: allowedChildIds.map(String),
            mode: CardSearchFilterMode.CONTAINS,
          },
        ],
      },
    ];
  }
}
