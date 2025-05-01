/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiContentModel } from './IApiContentModel';
import type { IApiMediaWithCropsModel } from './IApiMediaWithCropsModel';
export type CardPropertiesModel = {
    displayName?: string | null;
    image?: Array<IApiMediaWithCropsModel> | null;
    backImage?: Array<IApiMediaWithCropsModel> | null;
    attributes?: ApiBlockListModel;
    sections?: ApiBlockListModel;
    questions?: ApiBlockListModel;
    set?: Array<IApiContentModel> | null;
    faqLink?: string | null;
    hideFromDecks?: boolean | null;
    teamRequirements?: ApiBlockListModel;
    squadRequirements?: ApiBlockListModel;
    slotTargetRequirements?: ApiBlockListModel;
    allowedChildren?: Array<IApiContentModel> | null;
    maxChildren?: number | null;
    deckMutations?: ApiBlockListModel;
    embedFooterText?: string | null;
};

