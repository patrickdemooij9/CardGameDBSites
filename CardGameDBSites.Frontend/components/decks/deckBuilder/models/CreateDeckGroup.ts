import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import type CreateDeckSlot from "./CreateDeckSlot";
import CreateDeckValidation from "./CreateDeckValidation";
import { GetInvalidRequirements, IsValid } from "~/services/requirements/RequirementService";
import type { CreateDeckValidationItem } from "./CreateDeckValidationItem";

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
      maxAmount += slot.getMaxAmount();
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

  validateGroup(): CreateDeckValidation {
    const errors: CreateDeckValidationItem[] = [];

    const cards: CardDetailApiModel[] = [];
    this.slots.forEach((slot) => {
      const slotErrors = slot.validate();
      if ((slotErrors?.length ?? 0) > 0) {
        errors.push(...slotErrors!);
      }
      cards.push(...slot.getCards());
    });
    const invalidRequirements = GetInvalidRequirements(cards, this.requirements);
    invalidRequirements.forEach((requirement) => {
      errors.push({
        errorMessage: requirement.errorMessage ?? "Requirement not met",
        showMessage: true,
      });
    });
    return new CreateDeckValidation(errors);
  }

  getAvailableSlots() {
    return this.slots; //TODO: This should work with dynamic slots
  }
}
