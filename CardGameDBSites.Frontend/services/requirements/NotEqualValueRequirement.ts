import {
  CardSearchFilterClauseType,
  CardSearchFilterMode,
  type CardDetailApiModel,
  type CardsQueryFilterApiModel,
  type CardsQueryFilterClauseApiModel,
} from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";
import type OverviewFilterValueModel from "~/components/overviews/OverviewFilterValueModel";

export default class NotEqualValueRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.NotEqualValue;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    return !cards.some((card) => {
      const value = card.attributes![config.ability];
      if (!value) return false;
      return value.some((item) => config.values.includes(item));
    });
  }

  ToFilters(cards: CardDetailApiModel[], config: Record<string, any>): CardsQueryFilterClauseApiModel {
    const values = config.values as string[];
    return {
      clauseType: CardSearchFilterClauseType.NOT,
      filters: [
        {
          alias: config.ability,
          values: values,
          mode: CardSearchFilterMode.CONTAINS,
        },
      ],
    };
  }
}
