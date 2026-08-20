/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ImageCropsApiModel } from './ImageCropsApiModel';
export type MetaTierLeaderApiModel = {
    cardId?: number;
    name?: string;
    tier?: string;
    deckCount?: number;
    eventCount?: number;
    wins?: number;
    losses?: number;
    draws?: number;
    top8Count?: number;
    firstPlaceCount?: number;
    winratePercentage?: number;
    metaSharePercentage?: number;
    metaShareDeltaPoints?: number | null;
    winrateDeltaPoints?: number | null;
    isNewEntry?: boolean;
    unrankedReason?: string | null;
    metaUrl?: string | null;
    imageUrl?: ImageCropsApiModel;
};

