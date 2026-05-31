import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import {
  DisplaySize,
  RequirementType,
  type DeckAmount,
} from "../DeckBuilderModels";
import CreateDeckCardGroup from "./CreateDeckCardGroup";
import {
  FixedDeckAmountConfig,
  type CreateDeckSlotAmount,
} from "./CreateDeckSlotAmount";
import {
  GetInvalidRequirements,
  IsValid,
} from "~/services/requirements/RequirementService";
import type { CreateDeckValidationItem } from "./CreateDeckValidationItem";
import { GetCardValue } from "~/helpers/CardHelper";

export default class CreateDeckSlot {
  id: number;
  label: string;
  cardGroups: CreateDeckCardGroup[];

  minCards: number;
  maxCardAmount: CreateDeckSlotAmount;
  overwriteAmount?: number;
  displaySize: DisplaySize;
  disableRemoval: boolean;
  numberMode: boolean;
  allowMovingToSideboard: boolean;
  showIfTargetSlotIsFilled: number | undefined;

  requirements: RequirementApiModel[] = [];
  useGroupRequirements: boolean = true;

  constructor(id: number, label: string) {
    this.id = id;
    this.label = label;
    this.cardGroups = [];

    this.minCards = 0;
    this.maxCardAmount = new FixedDeckAmountConfig(1);
    this.displaySize = DisplaySize.Small;
    this.disableRemoval = false;
    this.numberMode = false;
    this.allowMovingToSideboard = false;
    this.showIfTargetSlotIsFilled = undefined;
  }

  getAmount() {
    return this.cardGroups
      .flatMap((cardGroup) => cardGroup.cards.map((card) => card.amount))
      .reduce((a, b) => a + b, 0);
  }

  getCardAmount(card: CardDetailApiModel) {
    const allMainCards = this.cardGroups.flatMap((group) => group.cards);
    const childCards = allMainCards
      .flatMap((deckCard) => deckCard.children)
      .flatMap((slot) => slot.cardGroups)
      .flatMap((group) => group.cards);

    return (
      [...allMainCards, ...childCards]
        .find((c) => c.card.baseId === card.baseId)?.amount ?? 0
    );
  }

  getCards() {
    return this.cardGroups
      .flatMap((group) => group.cards)
      .map((deckCard) => deckCard.card);
  }

  getCardMaxAmount(card: CardDetailApiModel) {
    if (this.overwriteAmount && this.overwriteAmount > 0) {
      return this.overwriteAmount;
    }
    const amount = GetCardValue<string | number>(card, "Amount");
    if (amount === null || amount === undefined || amount === "") {
      return 0;
    }
    const parsedAmount = typeof amount === "number" ? amount : Number(amount);
    return Number.isFinite(parsedAmount) ? parsedAmount : 0;
  }

  getMaxAmount() {
    return this.maxCardAmount.GetAmount();
  }

  isFull() {
    const maxAmount = this.getMaxAmount();
    if (maxAmount === undefined) {
      // Unlimited amount
      return false;
    }
    return this.getAmount() >= maxAmount;
  }

  validate(): CreateDeckValidationItem[] | undefined {
    const invalidRequirements = GetInvalidRequirements(
      this.getCards(),
      this.requirements,
      false,
    );
    if (invalidRequirements.length > 0) {
      return invalidRequirements.map((requirement) => ({
        errorMessage: requirement.errorMessage ?? "Not valid requirement",
        showMessage: true,
      }));
    }

    const amount = this.getAmount();
    const maxAmount = this.getMaxAmount();
    return amount >= this.minCards &&
      (maxAmount === undefined || amount <= maxAmount)
      ? undefined
      : [
          {
            errorMessage: "Not valid amount",
            showMessage: false,
          },
        ];
  }

  isInsideSlot(card: CardDetailApiModel) {
    return (
      this.cardGroups
        .flatMap((group) => group.cards)
        .find((c) => c.card.baseId === card.baseId) !== undefined
    );
  }

  isCardAllowedToRemove(card: CardDetailApiModel) {
    if (this.disableRemoval) {
      return false;
    }

    const deckCard = this.cardGroups
      .flatMap((group) => group.cards)
      .find((c) => c.card.baseId === card.baseId);
    if (!deckCard) {
      return false;
    }
    return deckCard.allowRemoval;
  }

  canAddCard(card: CardDetailApiModel, existingAmountInOtherSlots: number = 0) {
    if (this.isFull()) {
      return false;
    }

    const currentSize = this.cardGroups
      .flatMap((group) => group.cards)
      .find((c) => c.card.baseId === card.baseId);

    const amountInThisSlot = currentSize?.amount ?? 0;
    if (
      (currentSize !== undefined || existingAmountInOtherSlots > 0) &&
      amountInThisSlot + existingAmountInOtherSlots >=
        this.getCardMaxAmount(card)
    ) {
      return false;
    }

    return true;
  }

  addCard(card: CardDetailApiModel, amount: number = 1) {
    const currentSize = this.cardGroups
      .flatMap((group) => group.cards)
      .find((c) => c.card.baseId === card.baseId);

    if (currentSize) {
      currentSize.amount += amount;
      return currentSize;
    }
    const childSlots = this._createChildSlots(card);
    const groupToAdd = this.cardGroups.find((group) =>
      IsValid([card], group.requirements, true),
    );
    if (groupToAdd) {
      const deckCard = {
        card,
        amount: amount,
        allowRemoval: true,
        children: childSlots,
      };
      groupToAdd.cards.push(deckCard);
      return deckCard;
    }
  }

  removeCard(card: CardDetailApiModel) {
    const group = this.cardGroups.find((group) =>
      group.cards.some((c) => c.card.baseId === card.baseId),
    );
    const currentSize = group?.cards.find((c) => c.card.baseId === card.baseId);

    if (currentSize) {
      currentSize.amount--;
      if (currentSize.amount <= 0 && group) {
        // Clean up child slots before removing the parent card
        currentSize.children.forEach((childSlot) => childSlot._clearAllCards());
        group.cards = group.cards.filter((c) => c.card.baseId !== card.baseId);
      }
    }
  }

  /** @internal Removes all cards from this slot (used for child slot cleanup). */
  _clearAllCards() {
    this.cardGroups.forEach((group) => {
      group.cards = [];
    });
  }

  /** @internal Creates child slots for a card when it is first added. */
  _createChildSlots(card: CardDetailApiModel): CreateDeckSlot[] {
    const allowedChildren = card.allowedChildren ?? [];
    const maxChildren = card.maxChildren ?? 0;
    if (maxChildren === 0 || allowedChildren.length === 0) {
      return [];
    }

    const childSlot = new CreateDeckSlot(-1, "Battle Tactics");
    childSlot.maxCardAmount = new FixedDeckAmountConfig(maxChildren);
    childSlot.displaySize = DisplaySize.Small;
    childSlot.disableRemoval = false;
    childSlot.numberMode = false;
    childSlot.requirements = [
      {
        alias: RequirementType.ChildOf,
        config: { allowedChildIds: allowedChildren },
      } as RequirementApiModel,
    ];
    childSlot.useGroupRequirements = false; // Child slot should not inherit parent group requirements

    const childGroup = new CreateDeckCardGroup("");
    childSlot.cardGroups.push(childGroup);

    return [childSlot];
  }
}
