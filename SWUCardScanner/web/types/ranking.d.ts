export interface RankedRow {
    row: number;
    score: number;
}
export declare const EXCLUDED: number;
export declare function rankCards(scores: ArrayLike<number>, baseIds: ArrayLike<number>, limit: number): RankedRow[];
