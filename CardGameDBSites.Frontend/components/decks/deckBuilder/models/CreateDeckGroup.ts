import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import type CreateDeckSlot from "./CreateDeckSlot";
import CreateDeckValidation from "./CreateDeckValidation";
import {
  GetInvalidRequirements,
  IsValid,
} from "~/services/requirements/RequirementService";
import type { CreateDeckValidationItem } from "./CreateDeckValidationItem";
import { GetCardValue } from "~/helpers/CardHelper";

export default class CreateDeckGroup {
  id?: number;
  name?: string;
  slots: CreateDeckSlot[] = [];

  requirements: RequirementApiModel[] = [];

  getAmount() {
    let amount = 0;
    this.slots.forEach((slot) => {
      amount += slot.getAmount();
    });
    return amount;
  }

  getMaxAmount() {
    let maxAmount = 0;
    this.slots.forEach((slot) => {
      const amount = slot.getMaxAmount();
      if (amount) {
        maxAmount += amount;
      } else {
        maxAmount += slot.minCards;
      }
    });
    return maxAmount;
  }

  getCards() {
    const cards: CardDetailApiModel[] = [];
    this.slots.forEach((slot) => {
      slot.cardGroups.forEach((cardGroup) => {
        cardGroup.cards.forEach((card) => {
          cards.push(card.card);
        });
      });
    });
    return cards;
  }

  getAvailableSlots() {
    const filledSlotIds = this.slots
      .filter((slot) => slot.isFull())
      .map((slot) => slot.id);

    return this.slots.filter((slot) => {
      return (
        slot.showIfTargetSlotIsFilled == null ||
        filledSlotIds.includes(slot.showIfTargetSlotIsFilled)
      );
    });
  }

  getPointsTotal(): number {
    let points = 0;
    this.slots.forEach((slot) => {
      slot.cardGroups.forEach((cardGroup) => {
        cardGroup.cards.forEach((deckCard) => {
          const pointValue = GetCardValue<number>(deckCard.card, "Points");
          if (pointValue !== null && pointValue !== undefined) {
            points += pointValue;
          }
        });
      });
    });
    return points;
  }

  hasEnoughPoints(): boolean {
    return this.getPointsTotal() >= 0;
  }

  validateGroup(): CreateDeckValidation {
    const errors: CreateDeckValidationItem[] = [];

    const cards: CardDetailApiModel[] = [];
    this.getAvailableSlots().forEach((slot) => {
      const slotErrors = slot.validate();
      if ((slotErrors?.length ?? 0) > 0) {
        errors.push(...slotErrors!);
      }
      cards.push(...slot.getCards());
    });
    const invalidRequirements = GetInvalidRequirements(
      cards,
      this.requirements,
      false
    );
    invalidRequirements.forEach((requirement) => {
      errors.push({
        errorMessage: requirement.errorMessage ?? "Requirement not met",
        showMessage: true,
      });
    });

    // Points validation: only check if any card has a Points attribute
    if (cards.some((card) => GetCardValue<number>(card, "Points") !== null)) {
      if (!this.hasEnoughPoints()) {
        errors.push({
          errorMessage: `Squad has ${Math.abs(this.getPointsTotal())} points too many`,
          showMessage: true,
        });
      }
    }

    return new CreateDeckValidation(errors);
  }
}
