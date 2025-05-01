import type { CardDetailApiModel } from "~/api/default";
import type RequirementType from "./RequirementType";

export interface IRequirement {
    RequirementType: RequirementType;
    IsValid(card: CardDetailApiModel, config: Record<string, any>): boolean;
}