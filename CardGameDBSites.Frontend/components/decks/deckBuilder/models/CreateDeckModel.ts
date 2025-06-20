import type { CardDetailApiModel } from "~/api/default";
import type CreateDeckGroup from "./CreateDeckGroup";
import type CreateDeckSlot from "./CreateDeckSlot";
import { IsValid } from "~/services/requirements/RequirementService";
import CreateDeckValidation from "./CreateDeckValidation";

export class CreateDeckModel {
  id?: number;
  name?: string;
  description?: string;
  typeId?: number;
  groups: CreateDeckGroup[] = [];

  getSlotsForCard(card: CardDetailApiModel) {
    const slots: CreateDeckSlot[] = [];
    this.groups.forEach((group) => {
      group.slots.forEach((slot) => {
        if (IsValid([card], slot.requirements)){
          slots.push(slot);
        }
      });
    });
    return slots; //TODO: Check requirements
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

  validate(): CreateDeckValidation {
    var result = new CreateDeckValidation([]);
    this.groups.forEach((group) => {
      result.combine(group.validateGroup());
    })
    return result;
  }
}
