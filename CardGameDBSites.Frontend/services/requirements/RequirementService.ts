import { RestrictionType, type CardDetailApiModel, type CardsQueryFilterClauseApiModel, type RequirementApiModel } from "~/api/default";
import EqualValueRequirement from "./EqualValueRequirement";
import type OverviewFilterValueModel from "~/components/overviews/OverviewFilterValueModel";
import NotEqualValueRequirement from "./NotEqualValueRequirement";
import ResourceRequirement from "./ResourceRequirement";
import type { IRequirement } from "./IRequirement";

const requirementHandlers: IRequirement[] = [
    new EqualValueRequirement(),
    new NotEqualValueRequirement(),
    new ResourceRequirement()
]

export function GetValidCards(cards: CardDetailApiModel[], requirements: RequirementApiModel[])
{
    let result = [...cards];
    requirements.forEach((requirement) => {
        const requirementHandler = requirementHandlers.find((handler) => handler.RequirementType === requirement.alias);
        if (!requirementHandler){
            console.warn(`No handler found for requirement type ${requirement.alias}`);
            return;
        };

        const cardsThatSucceed: CardDetailApiModel[] = [];
        result.forEach((card) => {
            if (requirementHandler.IsValid([card], requirement.config!)){
                cardsThatSucceed.push(card);
            }
        })
        result = [...cardsThatSucceed];
    })
    return result;
}

export function IsValid(cards: CardDetailApiModel[], requirements: RequirementApiModel[])
{
    return GetInvalidRequirements(cards, requirements).length === 0;
}

export function GetInvalidRequirements(cards: CardDetailApiModel[], requirements: RequirementApiModel[])
{
    const invalidRequirements: RequirementApiModel[] = [];
    requirements.forEach((requirement) => {
        const requirementHandler = requirementHandlers.find((handler) => handler.RequirementType === requirement.alias);
        if (!requirementHandler){
            console.warn(`No handler found for requirement type ${requirement.alias}`);
            return;
        };

        if (!requirementHandler.IsValid(cards, requirement.config!)){
            invalidRequirements.push(requirement);
        }
    })
    return invalidRequirements;
}

export function GetFilters(cards: CardDetailApiModel[], requirements: RequirementApiModel[], ignorePassive: boolean)
{
    const filters: CardsQueryFilterClauseApiModel[] = [];
    requirements.forEach((requirement) => {
        if (ignorePassive && requirement.restrictionType == RestrictionType.PASSIVE) {
            return;
        }

        const requirementHandler = requirementHandlers.find((handler) => handler.RequirementType === requirement.alias);
        if (!requirementHandler){
            console.warn(`No handler found for requirement type ${requirement.alias}`);
            return;
        }

        const requirementFilterClause = requirementHandler.ToFilters(cards, requirement.config!);
        if (requirementFilterClause){
            filters.push(requirementFilterClause);
        }
    })
    return filters;
}