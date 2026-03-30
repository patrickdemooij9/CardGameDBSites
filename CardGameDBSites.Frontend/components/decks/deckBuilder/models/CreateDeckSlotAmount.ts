import type { RequirementApiModel } from "~/api/default";

export interface CreateDeckSlotAmount {
    GetAmount(): number | undefined;
}

export class FixedDeckAmountConfig implements CreateDeckSlotAmount {
    amount?: number;

    constructor(amount: number) {
        this.amount = amount == 0 ? undefined : amount;
    }
    GetAmount(): number | undefined {
        return this.amount;
    }
}

export class DynamicDeckAmountConfig implements CreateDeckSlotAmount {
    requirements: RequirementApiModel[];

    constructor(requirements: RequirementApiModel[]) {
        this.requirements = requirements;
    }
    GetAmount(): number | undefined {
        throw new Error("Method not implemented.");
    }
}