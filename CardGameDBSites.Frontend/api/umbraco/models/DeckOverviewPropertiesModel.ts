/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IApiContentModel } from './IApiContentModel';
import type { RichTextModel } from './RichTextModel';
export type DeckOverviewPropertiesModel = {
    title?: string | null;
    description?: RichTextModel;
    decksPerRow?: number | null;
    squadCards?: number | null;
    squadSettings?: Array<IApiContentModel> | null;
};

