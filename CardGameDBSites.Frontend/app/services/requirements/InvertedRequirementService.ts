import type { CardDetailApiModel, CardsQueryFilterClauseApiModel } from "~/api/default";
import type { IInvertRequirement } from "./IInvertRequirement";
import EqualValueRequirement from "./EqualValueRequirement";
import NotEqualValueRequirement from "./NotEqualValueRequirement";

export interface IInvertibleCondition {
  type: string;
  config: Record<string, any>;
}

const invertedRequirementHandlers: IInvertRequirement[] = [
  new EqualValueRequirement(),
  new NotEqualValueRequirement(),
];

export function InvertConditions(
  cards: CardDetailApiModel[],
  conditions: IInvertibleCondition[],
): CardsQueryFilterClauseApiModel[] | undefined {
  const clauses: CardsQueryFilterClauseApiModel[] = [];
  for (const condition of conditions) {
    const requirementHandler = invertedRequirementHandlers.find(
      (handler) => handler.RequirementType === condition.type,
    );
    if (!requirementHandler) {
      return undefined;
    }
    const inverted = requirementHandler.InvertFilter(cards, condition.config);
    if (!inverted) {
      return undefined;
    }
    clauses.push(...inverted);
  }
  return clauses;
}
