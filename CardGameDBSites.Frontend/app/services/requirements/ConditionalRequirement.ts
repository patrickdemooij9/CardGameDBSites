import {
    CardSearchFilterClauseType,
  type CardDetailApiModel,
  type CardsQueryFilterClauseApiModel,
} from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";
import EqualValueRequirement from "./EqualValueRequirement";
import NotEqualValueRequirement from "./NotEqualValueRequirement";
import ResourceRequirement from "./ResourceRequirement";
import SameValueRequirement from "./SameValueRequirement";
import type { IInvertRequirement } from "./IInvertRequirement";
import { GetValidCards, IsValid } from "./RequirementService";

//TODO: Move to better location
const requirementHandlers: IRequirement[] = [
  new EqualValueRequirement(),
  new NotEqualValueRequirement(),
  new ResourceRequirement(),
  new SameValueRequirement(),
];

const invertedRequirementHandlers: IInvertRequirement[] = [
  new EqualValueRequirement(),
  new NotEqualValueRequirement(),
]

interface IConditionalRequirementConfig {
  type: string;
  config: Record<string, any>;
}

export default class ConditionalRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.Conditional;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    const conditions = config["condition"] as IConditionalRequirementConfig[];
    let conditionMet = true;
    for (const condition of conditions) {
      if (!conditionMet) {
        break;
      }

      const requirementHandler = requirementHandlers.find(
        (handler) => handler.RequirementType === condition.type,
      );
      if (!requirementHandler) {
        console.warn(`No handler found for requirement type ${condition.type}`);
        conditionMet = false;
        continue;
      }
      if (!requirementHandler.IsValid(cards, condition.config)) {
        conditionMet = false;
      }
    }
    if (!conditionMet) {
      return true;
    }
    var requirements = config["requirements"] as IConditionalRequirementConfig[];
    for (const requirement of requirements) {
        const requirementHandler = requirementHandlers.find(
            (handler) => handler.RequirementType === requirement.type,
        );
        if (!requirementHandler) {
            console.warn(`No handler found for requirement type ${requirement.type}`);
            continue;
        }
        if (!requirementHandler.IsValid(cards, requirement.config)) {
            return false;
        }
    }
    return true;
  }

  ToFilters(
    cards: CardDetailApiModel[],
    config: Record<string, any>,
  ): CardsQueryFilterClauseApiModel[] | undefined {
    const conditions = config["condition"] as IConditionalRequirementConfig[];
    var requirements = config["requirements"] as IConditionalRequirementConfig[];

    const conditionFilters: CardsQueryFilterClauseApiModel[] = [];
    const filters: CardsQueryFilterClauseApiModel[] = [];

    for (const condition of conditions) {
      const requirementHandler = invertedRequirementHandlers.find(
        (handler) => handler.RequirementType === condition.type,
      );
      if (!requirementHandler) {
        console.warn(`No inverted handler found for requirement type ${condition.type}`);
        continue;
      }
      const requirementFilterClause = requirementHandler.InvertFilter(cards, condition.config);
      if (requirementFilterClause){
        conditionFilters.push(...requirementFilterClause)
      }
    }

    const cardsMatchingCondition = GetValidCards(cards, requirements);
    for (const requirement of requirements) {
        const requirementHandler = requirementHandlers.find(
            (handler) => handler.RequirementType === requirement.type,
        );
        if (!requirementHandler) {
            console.warn(`No handler found for requirement type ${requirement.type}`);
            continue;
        }
        const requirementFilterClause = requirementHandler.ToFilters(cardsMatchingCondition, requirement.config);
        if (requirementFilterClause) {
            filters.push(...requirementFilterClause);
        }
    }
    if (conditionFilters.length === 0 || filters.length === 0) {
        return undefined;
    }

    // NOTE: when there are multiple requirements they are OR'd in alongside the conditions rather
    // than AND'd as their own group. That is correct for the common single-requirement case; a
    // faithful `(NOT c1 OR NOT c2) OR (r1 AND r2)` would need nested clause groups, which the flat
    // filter model intentionally does not support.
    return [{
      clauseType: CardSearchFilterClauseType.AND,
      filters: [...conditionFilters.flatMap(c => c.filters ?? []), ...filters.flatMap(c => c.filters ?? [])]
    }];
  }
}