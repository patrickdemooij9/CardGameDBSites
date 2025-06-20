import type { RequirementApiModel } from "~/api/default";
import type { DeckCard } from "../DeckBuilderModels";

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

  getOrderedCards(){
    return this.cards; //TODO: Sort
  }
}
