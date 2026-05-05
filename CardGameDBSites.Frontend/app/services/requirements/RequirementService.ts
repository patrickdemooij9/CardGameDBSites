import { CardSearchFilterClauseType, CardSearchFilterMode, RestrictionType, type CardDetailApiModel, type CardsQueryFilterClauseApiModel, type RequirementApiModel } from "~/api/default";
import EqualValueRequirement from "./EqualValueRequirement";
import type OverviewFilterValueModel from "~/components/overviews/OverviewFilterValueModel";
import NotEqualValueRequirement from "./NotEqualValueRequirement";
import ResourceRequirement from "./ResourceRequirement";
import type { IRequirement } from "./IRequirement";
import ConditionalRequirement from "./ConditionalRequirement";
import SameValueRequirement from "./SameValueRequirement";
import type { OverviewFilterModel } from "~/components/overviews/OverviewFilterModel";

const requirementHandlers: IRequirement[] = [
    new EqualValueRequirement(),
    new NotEqualValueRequirement(),
    new ResourceRequirement(),
    new ConditionalRequirement(),
    new SameValueRequirement(),
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

export function IsValid(cards: CardDetailApiModel[], requirements: RequirementApiModel[], ignorePassive: boolean)
{
    return GetInvalidRequirements(cards, requirements, ignorePassive).length === 0;
}

export function GetInvalidRequirements(cards: CardDetailApiModel[], requirements: RequirementApiModel[], ignorePassive: boolean)
{
    const invalidRequirements: RequirementApiModel[] = [];
    requirements.forEach((requirement) => {
        if (requirement.restrictionType === RestrictionType.FILTER) {
            return;
        }
        if (ignorePassive && requirement.restrictionType == RestrictionType.PASSIVE) {
            return;
        }

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
            filters.push(...requirementFilterClause);
        }
    })
    return filters;
}

export function ApplyInternalFilterRestrictions(
    internalClauses: CardsQueryFilterClauseApiModel[],
    publicFilters: OverviewFilterModel[]
): OverviewFilterModel[] {
    const restrictionMap = new Map<string, string[]>();
    internalClauses.forEach((clause) => {
        if (clause.clauseType === CardSearchFilterClauseType.AND) {
            clause.filters?.forEach((filter) => {
                if (filter.mode === CardSearchFilterMode.CONTAINS && filter.values?.length) {
                    restrictionMap.set(filter.alias, filter.values);
                }
            });
        }
    });

    if (restrictionMap.size === 0) {
        return publicFilters;
    }

    return publicFilters
        .map((filter) => {
            const restriction = restrictionMap.get(filter.Alias);
            if (!restriction) {
                return filter;
            }
            if (restriction.length === 1) {
                return null;
            }
            if (filter.AutoFillValues) {
                return {
                    ...filter,
                    AutoFillValues: false,
                    Items: restriction.map((value) => ({
                        DisplayName: value,
                        Value: value,
                    })),
                };
            }
            return {
                ...filter,
                Items: filter.Items.filter((item) => restriction.includes(item.Value)),
            };
        })
        .filter((filter): filter is OverviewFilterModel => filter !== null);
}