/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DailyGameAttemptApiModel } from './DailyGameAttemptApiModel';
import type { DailyGameLeaderboardEntryApiModel } from './DailyGameLeaderboardEntryApiModel';
export type DailyGameBootstrapApiModel = {
    guestSessionToken: string;
    maxAttempts?: number;
    attemptsUsed?: number;
    attemptsLeft?: number;
    elapsedSeconds?: number;
    blurLevel?: number;
    isFinished?: boolean;
    isSolved?: boolean;
    imageDataUrl?: string | null;
    attempts?: Array<DailyGameAttemptApiModel>;
    leaderboard?: Array<DailyGameLeaderboardEntryApiModel>;
    currentPlacement?: DailyGameLeaderboardEntryApiModel;
};

