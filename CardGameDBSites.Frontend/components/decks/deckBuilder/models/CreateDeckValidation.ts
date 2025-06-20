import type { CreateDeckValidationItem } from "./CreateDeckValidationItem";

export default class CreateDeckValidation {
  items: CreateDeckValidationItem[];

  constructor(items: CreateDeckValidationItem[]) {
    this.items = items;;
  }

  isValid() {
    return this.items.length === 0;
  }

  combine(validation: CreateDeckValidation) {
    this.items.push(...validation.items);
  }
}
