import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import type CreateDeckGroup from "./CreateDeckGroup";
import CreateDeckSlot from "./CreateDeckSlot";
import { IsValid } from "~/services/requirements/RequirementService";
import CreateDeckValidation from "./CreateDeckValidation";
import type { CreateDeckValidationItem } from "./CreateDeckValidationItem";
import type { DeckCard } from "../DeckBuilderModels";
import CreateDeckCardGroup from "./CreateDeckCardGroup";
import { FixedDeckAmountConfig } from "./CreateDeckSlotAmount";
import RequirementType from "~/services/requirements/RequirementType";

export class CreateDeckModel {
  id?: number;
  name?: string;
  description: string = "";
  typeId?: number;
  groups: CreateDeckGroup[] = [];
  hasDynamicGroups: boolean = false;
  maxDynamicSlots: number = 0;
  preselectFirstSlot: boolean = true;

  // Track requirements added dynamically by cards (for removal on card removal)
  private cardTeamRequirements = new Map<number, RequirementApiModel[]>();
  private cardSquadRequirements = new Map<number, { group: CreateDeckGroup; reqs: RequirementApiModel[] }[]>();
  private cardSlotRequirements = new Map<number, { slot: CreateDeckSlot; reqs: RequirementApiModel[] }[]>();

  getSlotsForCard(card: CardDetailApiModel) {
    const slots: CreateDeckSlot[] = [];
    this.groups.forEach((group) => {
      if (!IsValid([card], group.requirements, true)) {
        return;
      }
      group.getAvailableSlots().forEach((slot) => {
        if (IsValid([card], slot.requirements, true)){
          slots.push(slot);
        }
      });
    });
    return slots;
  }

  getGroupForSlot(slot: CreateDeckSlot): CreateDeckGroup | undefined {
    return this.groups.find((group) =>
      group.slots.some((s) => s === slot)
    );
  }

  getAvailableGroups(): CreateDeckGroup[] {
    if (!this.hasDynamicGroups) {
      return this.groups;
    }
    return this.groups.filter(
      (group) => group.getAmount() > 0 || group.getMaxAmount() !== 0
    );
  }

  getDeckAmount(){
    let amount = 0;
    this.groups.forEach((group) => {
      amount += group.getAmount();
    });
    return amount;
  }

  getDeckMaxAmount(){
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
    return allCards;
  }

  getIllegalCards(): CardDetailApiModel[] {
    return this.getCards().map((card) => card.card).filter(card => !this.isLegalCard(card));
  }

  isLegalDeck(): boolean {
    return this.getIllegalCards().length === 0;
  }

  validate(): CreateDeckValidation {
    var result = new CreateDeckValidation([]);
    this.groups.forEach((group) => {
      result.combine(group.validateGroup());
    })
    return result;
  }

  // High-level addCard with side-effects: mutations, requirements, child slots
  addCard(group: CreateDeckGroup, slot: CreateDeckSlot, card: CardDetailApiModel): void {
    slot.addCard(card);
    this._applyMutations(card, group, slot, true);
    this._addCardRequirements(card, group);
    this._handleChildSlots(card, slot, false);
  }

  // High-level removeCard with side-effects
  removeCard(group: CreateDeckGroup, slot: CreateDeckSlot, card: CardDetailApiModel): void {
    // Remove child slots first
    this._handleChildSlots(card, slot, true);
    this._removeCardRequirements(card, group);
    this._applyMutations(card, group, slot, false);
    slot.removeCard(card);
  }

  private _applyMutations(
    card: CardDetailApiModel,
    group: CreateDeckGroup,
    _slot: CreateDeckSlot,
    isAdd: boolean
  ): void {
    const mutations = card.mutations ?? [];
    for (const mutation of mutations) {
      const targetSlot = mutation.slotId === 0
        ? _slot
        : group.slots.find((s) => s.id === mutation.slotId);
      if (!targetSlot) continue;

      const change = isAdd ? mutation.change : -mutation.change;
      if (mutation.alias === "minCards") {
        targetSlot.minCards = Math.max(0, targetSlot.minCards + change);
      }
    }
  }

  private _addCardRequirements(card: CardDetailApiModel, group: CreateDeckGroup): void {
    const cardId = card.baseId;
    if (!cardId) return;

    // Team-level requirements (not yet wired to deck-level validation, stored for removal)
    const teamReqs = card.teamRequirements ?? [];
    if (teamReqs.length > 0) {
      this.cardTeamRequirements.set(cardId, teamReqs);
    }

    // Squad-level requirements
    const squadReqs = card.squadRequirements ?? [];
    if (squadReqs.length > 0) {
      group.requirements.push(...squadReqs);
      const existing = this.cardSquadRequirements.get(cardId) ?? [];
      existing.push({ group, reqs: squadReqs });
      this.cardSquadRequirements.set(cardId, existing);
    }

    // Slot-target requirements
    const slotTargetReqs = card.slotTargetRequirements ?? [];
    for (const slotTarget of slotTargetReqs) {
      const targetSlot = group.slots.find((s) => s.id === slotTarget.slotId);
      if (!targetSlot || !slotTarget.requirements) continue;

      targetSlot.requirements.push(...slotTarget.requirements);
      const existing = this.cardSlotRequirements.get(cardId) ?? [];
      existing.push({ slot: targetSlot, reqs: slotTarget.requirements });
      this.cardSlotRequirements.set(cardId, existing);
    }
  }

  private _removeCardRequirements(card: CardDetailApiModel, group: CreateDeckGroup): void {
    const cardId = card.baseId;
    if (!cardId) return;

    // Team requirements
    this.cardTeamRequirements.delete(cardId);

    // Squad requirements
    const squadEntries = this.cardSquadRequirements.get(cardId);
    if (squadEntries) {
      for (const entry of squadEntries) {
        for (const req of entry.reqs) {
          const idx = entry.group.requirements.indexOf(req);
          if (idx > -1) entry.group.requirements.splice(idx, 1);
        }
      }
      this.cardSquadRequirements.delete(cardId);
    }

    // Slot requirements
    const slotEntries = this.cardSlotRequirements.get(cardId);
    if (slotEntries) {
      for (const entry of slotEntries) {
        for (const req of entry.reqs) {
          const idx = entry.slot.requirements.indexOf(req);
          if (idx > -1) entry.slot.requirements.splice(idx, 1);
        }
      }
      this.cardSlotRequirements.delete(cardId);
    }
  }

  private _handleChildSlots(
    card: CardDetailApiModel,
    slot: CreateDeckSlot,
    isRemove: boolean
  ): void {
    if (!isRemove) {
      // Add child slot when card supports children
      const maxChildren = card.maxChildren ?? 0;
      if (maxChildren <= 0) return;

      // Check dynamic slot cap
      if (this.maxDynamicSlots > 0) {
        const activeChildSlots = this.getCards()
          .flatMap((dc) => dc.children)
          .length;
        if (activeChildSlots >= this.maxDynamicSlots) return;
      }

      const childSlot = new CreateDeckSlot(-1, "Add-on");
      childSlot.maxCardAmount = new FixedDeckAmountConfig(maxChildren);
      childSlot.disableRemoval = false;
      childSlot.requirements = [
        {
          alias: RequirementType.ChildOf,
          config: {
            parentId: card.baseId,
            allowedChildren: card.allowedChildren ?? [],
          },
        } as any,
      ];
      const defaultGroup = new CreateDeckCardGroup("");
      childSlot.cardGroups = [defaultGroup];

      // Attach child slot to the DeckCard
      const deckCard = slot.cardGroups
        .flatMap((cg) => cg.cards)
        .find((dc) => dc.card.baseId === card.baseId);
      if (deckCard) {
        deckCard.children.push(childSlot);
      }
    } else {
      // Remove child slots from the DeckCard
      const deckCard = slot.cardGroups
        .flatMap((cg) => cg.cards)
        .find((dc) => dc.card.baseId === card.baseId);
      if (deckCard) {
        deckCard.children = [];
      }
    }
  }
}
