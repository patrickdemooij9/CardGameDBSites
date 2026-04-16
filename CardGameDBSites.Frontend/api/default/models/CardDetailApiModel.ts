/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CardPriceApiModel } from './CardPriceApiModel';
import type { CardVariantReferenceApiModel } from './CardVariantReferenceApiModel';
import type { CardDeckMutationApiModel } from './CardDeckMutationApiModel';
import type { CardSlotTargetRequirementApiModel } from './CardSlotTargetRequirementApiModel';
import type { ImageCropsApiModel } from './ImageCropsApiModel';
import type { RequirementApiModel } from './RequirementApiModel';
export type CardDetailApiModel = {
    baseId?: number;
    variantId?: number;
    variantTypeId?: number | null;
    displayName?: string;
    setId?: number;
    setName?: string;
    setCode?: string;
    urlSegment?: string;
    imageUrl?: ImageCropsApiModel;
    backImageUrl?: ImageCropsApiModel;
    attributes?: Record<string, Array<string>>;
    nonLegalDeckTypes?: Array<number>;
    allowedChildren?: Array<number>;
    maxChildren?: number;
    mutations?: Array<CardDeckMutationApiModel>;
    teamRequirements?: Array<RequirementApiModel>;
    squadRequirements?: Array<RequirementApiModel>;
    slotTargetRequirements?: Array<CardSlotTargetRequirementApiModel>;
    price?: CardPriceApiModel;
    variants?: Array<CardVariantReferenceApiModel>;
};

