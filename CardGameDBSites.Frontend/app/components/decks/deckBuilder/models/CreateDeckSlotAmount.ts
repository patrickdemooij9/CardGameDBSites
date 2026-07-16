import type { RequirementApiModel } from "~/api/default";
import type { CreateDeckModel } from "./CreateDeckModel";
import { GetValidCards } from "~/services/requirements/RequirementService";

export interface CreateDeckSlotAmount {
  GetAmount(): number | undefined;
}

export class FixedDeckAmountConfig implements CreateDeckSlotAmount {
  amount?: number;

  constructor(amount: number) {
    this.amount = amount == 0 ? undefined : amount;
  }

  GetAmount(): number | undefined {
    return this.amount;
  }
}

export class DynamicDeckAmountConfig implements CreateDeckSlotAmount {
  requirements: RequirementApiModel[];
  deck: CreateDeckModel;

  constructor(deck: CreateDeckModel, requirements: RequirementApiModel[]) {
    this.requirements = requirements;
    this.deck = deck;
  }

  GetAmount(): number | undefined {
    var allCards = this.deck.getCards();

    return GetValidCards(
      allCards.map((deckCard) => deckCard.card),
      this.requirements,
    ).length;
  }
}
