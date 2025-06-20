import {
  CardSearchFilterClauseType,
  CardSearchFilterMode,
  type CardDetailApiModel,
  type CardsQueryFilterApiModel,
  type CardsQueryFilterClauseApiModel,
} from "~/api/default";
import type OverviewFilterValueModel from "~/components/overviews/OverviewFilterValueModel";
import type { IRequirement } from "./IRequirement";
import CardService from "../CardService";
import RequirementType from "./RequirementType";
import { IsValid } from "./RequirementService";

export default class ResourceRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.Resource;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    const mainCards = cards.filter((card) =>
      config["mainCardsCondition"].every((c: Record<string, any>) => {
        c["alias"] = c["type"];
        return IsValid([card], [c]);
      })
    );
    /*if (
      mainCards.length === 0 ||
      (config.mainAbilityMaxSize > 0 &&
        mainCards.length < config.mainAbilityMaxSize)
    ) {
      return true;
    }*/

    const cardService = new CardService();
    const resourcePool = Object.groupBy(
      mainCards.flatMap(
        (c) => cardService.GetValues<string>(c, config["mainAbility"]) ?? []
      ),
      (item) => item
    );
    const otherCards = cards.filter((it) => !mainCards.includes(it));

    for (let i = 0; i < otherCards.length; i++) {
      const card = otherCards[i];

      const cardValues = cardService.GetValues<string>(card, config.ability);
      if (!cardValues || cardValues.length === 0) {
        return false;
      }

      let valid = true;
      const otherCardResource = Object.groupBy(cardValues, (item) => item);

      Object.entries(otherCardResource).forEach((item) => {
        const mainResourcePool = resourcePool[item[0]];
        if (!mainResourcePool) {
          valid = false;
          return;
        }
        if (config.singleResourceMode) {
          return;
        }
        if (mainResourcePool.length >= item[1]!.length) {
          return;
        }

        valid = false;
      });

      if (!valid) {
        return false;
      }
    }
    return true;
  }
  ToFilters(
    cards: CardDetailApiModel[],
    config: Record<string, any>
  ): CardsQueryFilterClauseApiModel {
    if (config.singleResourceMode) {
      return {}; //TODO: Implement filters for single resource mode
    }

    const mainCards = cards.filter((card) =>
      config["mainCardsCondition"].every((c: Record<string, any>) => {
        c["alias"] = c["type"];
        return IsValid([card], [c]);
      })
    );

    const cardService = new CardService();
    const resourcePool = Object.groupBy(
      mainCards.flatMap(
        (c) => cardService.GetValues<string>(c, config["mainAbility"]) ?? []
      ),
      (item) => item
    );

    const filters: CardsQueryFilterApiModel[] = [];
    const totalMainResources = Object.values(resourcePool).reduce(
      (prev, cur) => (prev += cur?.length ?? 0),
      0
    );
    config.possibleValues.forEach((value: string) => {
      filters.push({
        alias: `${config.ability}.${value}.Amount`,
        values: [
          "1",
          (
            6 -
            totalMainResources +
            (resourcePool[value]?.length ?? 0)
          ).toString(),
        ],
        mode: CardSearchFilterMode.RANGE,
      });
      filters.push({
        alias: `${config.mainAbility}.${value}.Amount`,
        values: ["1", "6"], //TODO: Need to figure out how this will work
        mode: CardSearchFilterMode.RANGE,
      });
    });

    return {
      clauseType: CardSearchFilterClauseType.AND,
      filters,
    };
  }
}
