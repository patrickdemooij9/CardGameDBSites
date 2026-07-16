import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import type CreateDeckGroup from "./CreateDeckGroup";
import type CreateDeckSlot from "./CreateDeckSlot";
import { IsValid } from "~/services/requirements/RequirementService";
import CreateDeckValidation from "./CreateDeckValidation";
import type { CreateDeckValidationItem } from "./CreateDeckValidationItem";
import type { DeckCard } from "../DeckBuilderModels";

export class CreateDeckModel {
  id?: number;
  name?: string;
  description?: string;
  typeId?: number;
  overwriteAmount?: number;
  requirements: RequirementApiModel[] = [];
  groups: CreateDeckGroup[] = [];

  hasSideboard: boolean = false;
  sideboardSlot: CreateDeckSlot | undefined;
  sideboardGroup: CreateDeckGroup | undefined;

  pickDefaultName(
    defaultNames: string[] | undefined,
    random: () => number = Math.random,
  ) {
    if (this.name || !defaultNames || defaultNames.length === 0) {
      return;
    }
    const index = Math.floor(random() * defaultNames.length);
    this.name = defaultNames[index];
  }

  moveCard(
    fromSlot: CreateDeckSlot,
    toSlot: CreateDeckSlot,
    card: CardDetailApiModel,
  ) {
    if (!toSlot || !fromSlot) {
      return;
    }

    fromSlot.removeCard(card);
    toSlot.addCard(card);
  }

  getSlotsForCard(card: CardDetailApiModel) {
    const slots: CreateDeckSlot[] = [];
    this.groups.forEach((group) => {
      group.slots.forEach((slot) => {
        if (IsValid([card], slot.requirements, true)) {
          slots.push(slot);
        }
        // Also check child slots of cards already in this slot
        slot.cardGroups.forEach((cardGroup) => {
          cardGroup.cards.forEach((deckCard) => {
            deckCard.children.forEach((childSlot) => {
              if (IsValid([card], childSlot.requirements, true)) {
                slots.push(childSlot);
              }
            });
          });
        });
      });
    });
    if (this.hasSideboard && this.sideboardGroup) {
      this.sideboardGroup.slots.forEach((slot) => {
        if (IsValid([card], slot.requirements, true)) {
          slots.push(slot);
        }
      });
    }
    return slots;
  }

  getTotalCardAmount(card: CardDetailApiModel): number {
    let total = 0;
    this.groups.forEach((group) => {
      group.slots.forEach((slot) => {
        total += slot.getCardAmount(card);
      });
    });
    if (this.sideboardGroup) {
      this.sideboardGroup.slots.forEach((slot) => {
        total += slot.getCardAmount(card);
      });
    }
    return total;
  }

  getCardAmountInOtherSlots(
    excludeSlot: CreateDeckSlot,
    card: CardDetailApiModel,
  ): number {
    return this.getTotalCardAmount(card) - excludeSlot.getCardAmount(card);
  }

  getSideboardAmount(): number {
    return this.sideboardGroup?.getAmount() ?? 0;
  }

  getDeckAmount() {
    let amount = 0;
    this.groups.forEach((group) => {
      amount += group.getAmount();
    });
    return amount;
  }

  getDeckMaxAmount() {
    let maxAmount = 0;
    this.groups.forEach((group) => {
      maxAmount += group.getMaxAmount();
    });
    return maxAmount;
  }

  isLegalCard(card: CardDetailApiModel): boolean {
    if (!this.typeId) return true;
    const nonLegalDeckTypes = card.nonLegalDeckTypes ?? [];
    return !nonLegalDeckTypes.includes(this.typeId);
  }

  getCards(): DeckCard[] {
    const allCards: DeckCard[] = [];
    this.groups.forEach((group) => {
      group.slots.forEach((slot) => {
        slot.cardGroups.forEach((cardGroup) => {
          cardGroup.cards.forEach((cardWrapper) => {
            allCards.push(cardWrapper);
          });
        });
      });
    });
    if (this.sideboardGroup) {
      this.sideboardGroup.slots.forEach((slot) => {
        slot.cardGroups.forEach((cardGroup) => {
          cardGroup.cards.forEach((cardWrapper) => {
            allCards.push(cardWrapper);
          });
        });
      });
    }
    return allCards;
  }

  getIllegalCards(): CardDetailApiModel[] {
    return this.getCards()
      .map((card) => card.card)
      .filter((card) => !this.isLegalCard(card));
  }

  isLegalDeck(): boolean {
    return this.getIllegalCards().length === 0;
  }

  validate(): CreateDeckValidation {
    var result = new CreateDeckValidation([]);
    this.groups.forEach((group) => {
      result.combine(group.validateGroup());
    });
    return result;
  }
}
