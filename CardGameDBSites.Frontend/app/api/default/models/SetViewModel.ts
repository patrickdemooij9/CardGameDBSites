/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FrequentlyAskedQuestionApiModel } from './FrequentlyAskedQuestionApiModel';
export type SetViewModel = {
    id: number;
    displayName: string;
    urlSegment: string;
    imageUrl?: string | null;
    code?: string | null;
    category?: string | null;
    mainVariants?: Array<number> | null;
    releaseDate?: string | null;
    subHeading?: string | null;
    description?: string | null;
    frequentlyAskedQuestions?: Array<FrequentlyAskedQuestionApiModel>;
};

