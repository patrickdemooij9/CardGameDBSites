import type { CardDetailApiModel, CardsQueryFilterClauseApiModel } from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";

export default class ChildOfRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.ChildOf;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    const allowedChildren = config["allowedChildren"] as number[] | undefined;
    if (!allowedChildren || allowedChildren.length === 0) return false;
    return cards.every((card) => allowedChildren.includes(card.baseId!));
  }

  ToFilters(
    _cards: CardDetailApiModel[],
    _config: Record<string, any>,
  ): CardsQueryFilterClauseApiModel[] | undefined {
    return undefined;
  }
}
