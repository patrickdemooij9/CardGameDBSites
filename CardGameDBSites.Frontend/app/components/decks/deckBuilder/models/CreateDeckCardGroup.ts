import type { RequirementApiModel } from "~/api/default";
import type { DeckCard } from "../DeckBuilderModels";
import { GetCardValue } from "~/helpers/CardHelper";

export default class CreateDeckCardGroup {
  displayName: string;
  sortBy?: string;
  cards: DeckCard[];

  requirements: RequirementApiModel[];

  constructor(displayName: string) {
    this.displayName = displayName;
    this.cards = [];
    this.requirements = [];
  }

  getOrderedCards() {
    const cards = this.cards.slice();
    if (this.sortBy) {
      return cards.sort((a, b) => {
        const aValue = GetCardValue<string>(a.card, this.sortBy!);
        const bValue = GetCardValue<string>(b.card, this.sortBy!);

        if (Number.isNaN(aValue) || Number.isNaN(bValue)) {
          return (aValue as string).localeCompare(bValue as string);
        }
        return Number.parseInt(aValue!) - Number.parseInt(bValue!);
      });
    }
    return cards;
  }
}
