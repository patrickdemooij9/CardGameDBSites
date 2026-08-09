import type { CardDetailApiModel, CardsQueryFilterClauseApiModel } from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";
import { GetCardValue } from "~/helpers/CardHelper";

export default class PointsRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.Points;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    const ability = config.ability as string;
    const min = config.min as number | undefined;
    const max = config.max as number | undefined;

    // Calculate total points from all cards
    let totalPoints = 0;
    for (const card of cards) {
      const pointValue = GetCardValue<string | number>(card, ability);
      if (pointValue === null) {
        // If a card doesn't have the points attribute, treat it as 0
        continue;
      }
      const parsedPoints = typeof pointValue === "number" ? pointValue : Number(pointValue);
      if (!Number.isFinite(parsedPoints)) {
        continue;
      }
      totalPoints += parsedPoints;
    }

    // Check if total is within the range
    if (min !== undefined && totalPoints < min) {
      return false;
    }
    if (max !== undefined && totalPoints > max) {
      return false;
    }

    return true;
  }

  ToFilters(
    cards: CardDetailApiModel[],
    config: Record<string, any>,
  ): CardsQueryFilterClauseApiModel[] | undefined {
    // Points requirements can't be easily converted to filters without knowing
    // the currently selected cards, so we return undefined
    return undefined;
  }
}
