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

//TODO: Move to better location
const requirementHandlers: IRequirement[] = [
  new EqualValueRequirement(),
  new NotEqualValueRequirement(),
  new ResourceRequirement(),
  new SameValueRequirement(),
];

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
      return undefined;
    }
    var requirements = config["requirements"] as IConditionalRequirementConfig[];
    const filters: CardsQueryFilterClauseApiModel[] = [];
    for (const requirement of requirements) {
        const requirementHandler = requirementHandlers.find(
            (handler) => handler.RequirementType === requirement.type,
        );
        if (!requirementHandler) {
            console.warn(`No handler found for requirement type ${requirement.type}`);
            continue;
        }
        const requirementFilterClause = requirementHandler.ToFilters(cards, requirement.config);
        if (requirementFilterClause) {
            filters.push(...requirementFilterClause);
        }
    }
    if (filters.length === 0) {
        return undefined;
    }
    return filters;
  }
}
