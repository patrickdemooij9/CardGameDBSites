/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckBuilderDeckCardGroupApiModel } from './DeckBuilderDeckCardGroupApiModel';
import type { DeckBuilderDynamicAmountViewModel } from './DeckBuilderDynamicAmountViewModel';
import type { DeckBuilderFixedAmountViewModel } from './DeckBuilderFixedAmountViewModel';
import type { DeckBuilderSlotAmountApiModel } from './DeckBuilderSlotAmountApiModel';
import type { RequirementApiModel } from './RequirementApiModel';
export type DeckBuilderSlotApiModel = {
    id: number;
    name: string;
    cardGroups?: Array<DeckBuilderDeckCardGroupApiModel>;
    minCards?: number;
    maxCardAmount: (DeckBuilderSlotAmountApiModel | DeckBuilderDynamicAmountViewModel | DeckBuilderFixedAmountViewModel);
    displaySize?: string;
    disableRemoval?: boolean;
    numberMode?: boolean;
    allowMovingToSideboard?: boolean;
    showIfTargetSlotIsFilled?: number | null;
    requirements?: Array<RequirementApiModel>;
};

