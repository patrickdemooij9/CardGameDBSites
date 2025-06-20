import type { RequirementApiModel } from "~/api/default";

export interface CreateDeckSlotAmount {
    GetAmount(): number;
}

export class FixedDeckAmountConfig implements CreateDeckSlotAmount {
    amount: number;

    constructor(amount: number) {
        this.amount = amount;
    }
    GetAmount(): number {
        return this.amount;
    }
}

export class DynamicDeckAmountConfig implements CreateDeckSlotAmount {
    requirements: RequirementApiModel[];

    constructor(requirements: RequirementApiModel[]) {
        this.requirements = requirements;
    }
    GetAmount(): number {
        throw new Error("Method not implemented.");
    }
}