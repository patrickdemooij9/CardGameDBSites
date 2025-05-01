import type { CardDetailApiModel } from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";

export default class EqualValueRequirement implements IRequirement {
    RequirementType: RequirementType = RequirementType.EqualValue;

    IsValid(card: CardDetailApiModel, config: Record<string, any>): boolean {
        const value = card.attributes![config.ability];
        if (!value) return false;
        return value.some((item) => config.values.includes(item));
    }
}