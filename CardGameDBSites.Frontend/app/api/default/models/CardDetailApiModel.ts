/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CardPriceApiModel } from './CardPriceApiModel';
import type { CardVariantReferenceApiModel } from './CardVariantReferenceApiModel';
import type { ImageCropsApiModel } from './ImageCropsApiModel';
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
    allowedChildren?: Array<number>;
    maxChildren?: number;
    nonLegalDeckTypes?: Array<number>;
    price?: CardPriceApiModel;
    variants?: Array<CardVariantReferenceApiModel>;
};

