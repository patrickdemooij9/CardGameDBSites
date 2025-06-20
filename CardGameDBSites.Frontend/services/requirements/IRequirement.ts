import type { CardDetailApiModel, CardsQueryFilterClauseApiModel } from "~/api/default";
import type RequirementType from "./RequirementType";

export interface IRequirement {
    RequirementType: RequirementType;

    IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean;
    ToFilters(cards: CardDetailApiModel[], config: Record<string, any>): CardsQueryFilterClauseApiModel;
}