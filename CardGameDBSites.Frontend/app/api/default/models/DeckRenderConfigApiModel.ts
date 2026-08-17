/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ArtCropApiModel } from './ArtCropApiModel';
export type DeckRenderConfigApiModel = {
    cardAspect?: number;
    landscapeTypes?: Array<string>;
    backImageTypes?: Array<string>;
    typeAttribute?: string | null;
    costAttribute?: string | null;
    aspectAttribute?: string | null;
    aspectColors?: Record<string, string>;
    artCrops?: Array<ArtCropApiModel>;
    eyebrow?: string | null;
    tierBMin?: number;
    tierCMin?: number;
    minCanvasRatio?: number;
};

