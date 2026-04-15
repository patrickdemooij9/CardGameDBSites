import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import { DisplaySize, type DeckAmount } from "../DeckBuilderModels";
import type CreateDeckCardGroup from "./CreateDeckCardGroup";
import { FixedDeckAmountConfig, type CreateDeckSlotAmount } from "./CreateDeckSlotAmount";
import { GetInvalidRequirements, IsValid } from "~/services/requirements/RequirementService";
import type { CreateDeckValidationItem } from "./CreateDeckValidationItem";
import { GetCardValue } from "~/helpers/CardHelper";

export default class CreateDeckSlot {
  id: number;
  label: string;
  cardGroups: CreateDeckCardGroup[];

  minCards: number;
  maxCardAmount: CreateDeckSlotAmount;
  displaySize: DisplaySize;
  disableRemoval: boolean;
  numberMode: boolean;
  showIfTargetSlotIsFilled: number | undefined;

  requirements: RequirementApiModel[] = [];

  constructor(id: number, label: string) {
    this.id = id;
    this.label = label;
    this.cardGroups = [];

    this.minCards = 0;
    this.maxCardAmount = new FixedDeckAmountConfig(1);
    this.displaySize = DisplaySize.Small;
    this.disableRemoval = false;
    this.numberMode = false;
    this.showIfTargetSlotIsFilled = undefined;
  }

  getAmount() {
    return this.cardGroups
      .flatMap((cardGroup) => cardGroup.cards.map((card) => card.amount))
      .reduce((a, b) => a + b, 0);
  }

  getCardAmount(card: CardDetailApiModel){
    return this.cardGroups
      .flatMap((group) => group.cards)
      .find((c) => c.card.baseId === card.baseId)?.amount ?? 0;
  }

  getCards() {
    return this.cardGroups.flatMap((group) => group.cards).map((deckCard) => deckCard.card);
  }

  getCardMaxAmount(card: CardDetailApiModel) {
    return GetCardValue<number>(card, "Amount") ?? 0;
  }

  getMaxAmount(deckCards?: CardDetailApiModel[]) {
    return this.maxCardAmount.GetAmount(deckCards);
  }

  isFull(deckCards?: CardDetailApiModel[]) {
    const maxAmount = this.getMaxAmount(deckCards);
    if (maxAmount === undefined) { // Unlimited amount
      return false;
    }
    return this.getAmount() >= maxAmount;
  }

  validate(deckCards?: CardDetailApiModel[]): CreateDeckValidationItem[] | undefined {
    const invalidRequirements = GetInvalidRequirements(this.getCards(), this.requirements, false);
    if (invalidRequirements.length > 0){
      return invalidRequirements.map((requirement) => ({
        errorMessage: requirement.errorMessage ?? "Not valid requirement",
        showMessage: true
      }))
    }

    const amount = this.getAmount();
    const maxAmount = this.getMaxAmount(deckCards);
    return amount >= this.minCards && (maxAmount === undefined || amount <= maxAmount) ? undefined : [{
      errorMessage: "Not valid amount",
      showMessage: false,
    }];
  }

  isInsideSlot(card: CardDetailApiModel) {
    return this.cardGroups
      .flatMap((group) => group.cards)
      .find((c) => c.card.baseId === card.baseId) !== undefined;
  }

  isCardAllowedToRemove(card: CardDetailApiModel) {
    if (this.disableRemoval) {
      return false;
    }

    const deckCard = this.cardGroups
      .flatMap((group) => group.cards)
      .find((c) => c.card.baseId === card.baseId);
    if (!deckCard){
      return false;
    }
    return deckCard.allowRemoval;
  }

  canAddCard(card: CardDetailApiModel, deckCards?: CardDetailApiModel[]) {
    if (this.isFull(deckCards)){
      return false;
    }

    const currentSize = this.cardGroups
      .flatMap((group) => group.cards)
      .find((c) => c.card.baseId === card.baseId);

    if (currentSize && currentSize.amount >= this.getCardMaxAmount(card)) {
      return false;
    }

    return true;
  }

  addCard(card: CardDetailApiModel) {
    const currentSize = this.cardGroups
      .flatMap((group) => group.cards)
      .find((c) => c.card.baseId === card.baseId);

    if (currentSize) {
      currentSize.amount++;
    } else {
      const groupToAdd = this.cardGroups.find((group) => IsValid([card], group.requirements, true));
      if (groupToAdd){
        groupToAdd.cards.push({ card, amount: 1, allowRemoval: true, children: [] });
      }
    }
  }

  removeCard(card: CardDetailApiModel) {
    const group = this.cardGroups.find((group) => group.cards.some((c) => c.card.baseId === card.baseId));
    const currentSize = group?.cards
      .find((c) => c.card.baseId === card.baseId);

    if (currentSize) {
      currentSize.amount--;
      if (currentSize.amount <= 0 && group) {
        group.cards = group.cards.filter((c) => c.card.baseId !== card.baseId);
      }
    }
  }
}
