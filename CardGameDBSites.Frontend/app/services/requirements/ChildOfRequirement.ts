import type { CardDetailApiModel, CardsQueryFilterClauseApiModel } from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";

export default class ChildOfRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.ChildOf;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    const allowedChildIds: number[] = config.allowedChildIds ?? [];
    return cards.every((card) => card.baseId !== undefined && allowedChildIds.includes(card.baseId));
  }

  ToFilters(_cards: CardDetailApiModel[], _config: Record<string, any>): CardsQueryFilterClauseApiModel[] | undefined {
    // ChildOf filtering is performed client-side via IsValid; no server-side filter clause needed.
    return undefined;
  }
}
