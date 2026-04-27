import {
  CardSearchFilterClauseType,
  CardSearchFilterMode,
  type CardDetailApiModel,
  type CardsQueryFilterApiModel,
  type CardsQueryFilterClauseApiModel,
} from "~/api/default";
import type { IRequirement } from "./IRequirement";
import RequirementType from "./RequirementType";
import { IsValid } from "./RequirementService";
import { GetCardValues } from "~/helpers/CardHelper";
import ResourceMode from "./ResourceMode";

function resolveResourceMode(config: Record<string, any>): ResourceMode {
  if (config.resourceMode) {
    return config.resourceMode as ResourceMode;
  }
  return config.singleResourceMode ? ResourceMode.Subset : ResourceMode.Budget;
}

function getMainCards(
  cards: CardDetailApiModel[],
  config: Record<string, any>,
): CardDetailApiModel[] {
  return cards.filter((card) =>
    config["mainCardsCondition"].every((c: Record<string, any>) => {
      c["alias"] = c["type"];
      return IsValid([card], [c], false);
    }),
  );
}

function buildResourcePool(
  mainCards: CardDetailApiModel[],
  mainAbility: string,
): Record<string, string[]> {
  return Object.groupBy(
    mainCards.flatMap((c) => GetCardValues<string>(c, mainAbility) ?? []),
    (item) => item,
  ) as Record<string, string[]>;
}

export default class ResourceRequirement implements IRequirement {
  RequirementType: RequirementType = RequirementType.Resource;

  IsValid(cards: CardDetailApiModel[], config: Record<string, any>): boolean {
    const mainCards = getMainCards(cards, config);
    if (mainCards.length === 0) return false;

    const resourcePool = buildResourcePool(mainCards, config["mainAbility"]);
    const otherCards = cards.filter((it) => !mainCards.includes(it));
    const mode = resolveResourceMode(config);

    for (let i = 0; i < otherCards.length; i++) {
      const card = otherCards[i];
      const cardValues = GetCardValues<string>(card!, config.ability);
      if (!cardValues || cardValues.length === 0) {
        return false;
      }

      const cardResource = Object.groupBy(cardValues, (item) => item);

      switch (mode) {
        case ResourceMode.ContainsAny: {
          const hasOverlap = Object.keys(cardResource).some(
            (key) => resourcePool[key],
          );
          if (!hasOverlap) return false;
          break;
        }

        case ResourceMode.Budget: {
          const totalMainResources = Object.values(resourcePool).reduce(
            (prev, cur) => prev + (cur?.length ?? 0),
            0,
          );
          const totalBudget = config.totalBudget ?? 6;
          const remainingBudget = totalBudget - totalMainResources;

          const newResourceCount = Object.entries(cardResource)
            .filter(([key]) => !resourcePool[key])
            .reduce((prev, [, vals]) => prev + (vals?.length ?? 0), 0);

          if (newResourceCount > remainingBudget) return false;
          break;
        }

        case ResourceMode.Subset: {
          for (const [key, values] of Object.entries(cardResource)) {
            if (!resourcePool[key]) return false;
            const mainPool = resourcePool[key];
            if (!mainPool || mainPool.length < (values?.length ?? 0))
              return false;
          }
          break;
        }
      }
    }
    return true;
  }

  ToFilters(
    cards: CardDetailApiModel[],
    config: Record<string, any>,
  ): CardsQueryFilterClauseApiModel[] | undefined {
    const mainCards = getMainCards(cards, config);
    if (mainCards.length === 0) return undefined;

    const resourcePool = buildResourcePool(mainCards, config["mainAbility"]);
    const mode = resolveResourceMode(config);
    const possibleValues: string[] = config.possibleValues ?? [];

    switch (mode) {
      case ResourceMode.ContainsAny: {
        const filters: CardsQueryFilterApiModel[] = possibleValues
          .filter((value) => resourcePool[value])
          .map((value) => ({
            alias: `${config.ability}.${value}.Amount`,
            values: ["1"],
            mode: CardSearchFilterMode.HIGHER,
          }));

        if (filters.length === 0) return undefined;
        return [
          {
            clauseType: CardSearchFilterClauseType.AND,
            filters,
          },
        ];
      }

      case ResourceMode.Budget: {
        const filters: CardsQueryFilterApiModel[] = [];
        const totalMainResources = Object.values(resourcePool).reduce(
          (prev, cur) => (prev += cur?.length ?? 0),
          0,
        );

        const mainBudget = config["mainAbilityMaxSize"] ?? 6;
        config.possibleValues.forEach((value: string) => {
          filters.push({
            alias: `${config.ability}.${value}.Amount`,
            values: [
              "1",
              (
                mainBudget -
                totalMainResources +
                (resourcePool[value]?.length ?? 0)
              ).toString(),
            ],
            mode: CardSearchFilterMode.RANGE,
          });
          filters.push({
            alias: `${config.mainAbility}.${value}.Amount`,
            values: ["1", mainBudget.toString()], //TODO: Need to figure out how this will work
            mode: CardSearchFilterMode.RANGE,
          });
        });

        return [
          {
            clauseType: CardSearchFilterClauseType.AND,
            filters,
          },
        ];
      }

      case ResourceMode.Subset: {
        const yesFilters: CardsQueryFilterApiModel[] = [];
        const noFilters: CardsQueryFilterClauseApiModel[] = [];

        possibleValues.forEach((value: string) => {
          if (!resourcePool[value] || !resourcePool[value].length) {
            noFilters.push({
              clauseType: CardSearchFilterClauseType.AND,
              filters: [
                {
                  alias: `${config.ability}.${value}.Amount`,
                  values: ["0"],
                  mode: CardSearchFilterMode.LOWER,
                },
              ],
            });
            return;
          }
          yesFilters.push({
            alias: `${config.ability}.${value}.Amount`,
            values: ["1", resourcePool[value].length.toString()],
            mode: CardSearchFilterMode.RANGE,
          });
        });

        return [
          {
            clauseType: CardSearchFilterClauseType.AND,
            filters: yesFilters,
          },
          ...noFilters,
        ];
      }

      default:
        return undefined;
    }
  }
}
