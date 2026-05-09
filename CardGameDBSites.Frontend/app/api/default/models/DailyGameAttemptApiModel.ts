/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DailyGameAttributeFeedbackApiModel } from './DailyGameAttributeFeedbackApiModel';
export type DailyGameAttemptApiModel = {
    attemptNumber?: number;
    guessedCardId?: number;
    guessedCardName?: string | null;
    isCorrect?: boolean;
    feedback?: Array<DailyGameAttributeFeedbackApiModel>;
    createdUtc?: string;
};

