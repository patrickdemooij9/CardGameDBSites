/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { IApiContentModel } from './IApiContentModel';
import type { IntroductionCompositionPropertiesModel } from './IntroductionCompositionPropertiesModel';
import type { RichTextModel } from './RichTextModel';
export type CardFilterCollectionPropertiesModel = (IntroductionCompositionPropertiesModel & {
    content?: RichTextModel;
    cardAttribute?: Array<IApiContentModel> | null;
    autoGenerateLandings?: boolean | null;
});

