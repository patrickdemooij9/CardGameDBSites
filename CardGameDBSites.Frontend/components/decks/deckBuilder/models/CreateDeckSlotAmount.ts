import type { CardDetailApiModel, RequirementApiModel } from "~/api/default";
import { IsValid } from "~/services/requirements/RequirementService";

export interface CreateDeckSlotAmount {
    GetAmount(deckCards?: CardDetailApiModel[]): number | undefined;
}

export class FixedDeckAmountConfig implements CreateDeckSlotAmount {
    amount?: number;

    constructor(amount: number) {
        this.amount = amount == 0 ? undefined : amount;
    }
    GetAmount(_deckCards?: CardDetailApiModel[]): number | undefined {
        return this.amount;
    }
}

export class DynamicDeckAmountConfig implements CreateDeckSlotAmount {
    requirements: RequirementApiModel[];

    constructor(requirements: RequirementApiModel[]) {
        this.requirements = requirements;
    }
    GetAmount(deckCards?: CardDetailApiModel[]): number | undefined {
        if (!deckCards || deckCards.length === 0) return undefined;
        return deckCards.reduce((count, card) => {
            return count + (IsValid([card], this.requirements, false) ? 1 : 0);
        }, 0);
    }
}
