import type { CardDetailApiModel, CardsQueryFilterClauseApiModel } from "~/api/default";
import type RequirementType from "./RequirementType";

export interface IInvertRequirement {
    RequirementType: RequirementType;

    InvertFilter(cards: CardDetailApiModel[], config: Record<string, any>): CardsQueryFilterClauseApiModel[] | undefined;
}