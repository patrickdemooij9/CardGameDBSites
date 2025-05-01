import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import EqualValueRequirement from "./EqualValueRequirement";

const requirementHandlers = [
    new EqualValueRequirement()
]

export function GetValidCards(cards: CardDetailApiModel[], requirements: RequirementApiModel[])
{
    let result = [...cards];
    requirements.forEach((requirement) => {
        const requirementHandler = requirementHandlers.find((handler) => handler.RequirementType === requirement.alias);
        if (!requirementHandler) return;

        const cardsThatSucceed: CardDetailApiModel[] = [];
        result.forEach((card) => {
            if (requirementHandler.IsValid(card, requirement.config!)){
                cardsThatSucceed.push(card);
            }
        })
        result = [...cardsThatSucceed];
    })
    return result;
}