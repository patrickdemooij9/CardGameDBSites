/**
 * Card identification: un-warp a detection, describe it as a vector, and find the
 * nearest reference.
 *
 * Every step here has an exact counterpart in model/descriptors.py, because the index
 * is built in Python and searched here. Where the two could plausibly disagree - image
 * downscaling, blur kernel, channel order - this file does the arithmetic explicitly
 * rather than delegating to the browser, whose filters are unspecified and vary.
 *
 * The descriptor is a hybrid, chosen on measurements in compare_descriptors.py:
 * a normalised pixel grid supplies instance-level detail (a plain CNN embedding scored
 * 24 points worse on its own), a MobileNetV3 embedding supplies tolerance to corner
 * error, and a whitened PCA projection makes the index small and, as it turns out,
 * more robust still.
 */
export interface IndexCard {
    file: string;
    name: string;
    set: string;
    type: string;
    face: string;
    orientation: 'portrait' | 'landscape';
    baseId: number;
    /** Every printing that uses this exact image, e.g. a card and its foil. */
    variantIds: number[];
    url: string;
    urlSegment: string;
}
export interface IndexConfig {
    dim: number;
    count: number;
    unwarpSize: number;
    pixelGrid: number;
    pixelWeight: number;
    /** Binomial-blur passes subtracted from the pixel grid to cancel glare gradients. */
    highpass?: number;
    embedSize: number;
    embedDim: number;
    pcaDim: number;
    embedder: string;
    storage?: string;
}
/** Why a candidate was not asserted. Null when it was accepted. */
export type RejectReason = 'blurry' | 'low-score' | 'low-margin' | 'no-index' | null;
export interface Candidate {
    card: IndexCard;
    score: number;
}
export interface MatchResult {
    /** Best candidate, present even when rejected, so the UI can explain itself. */
    card: IndexCard | null;
    /** Cosine similarity to the best reference, in [-1, 1]. */
    score: number;
    /** Gap to the second-best card. Small gaps mean "two cards look alike", not "no idea". */
    margin: number;
    /** Laplacian variance of the crop; low means too blurry to trust. */
    sharpness: number;
    /** Whether this passed every gate and should be shown as an identification. */
    accepted: boolean;
    reason: RejectReason;
    /** Runners-up, for diagnosing a near-miss. */
    candidates: Candidate[];
    /** The un-warped crop the matcher actually saw. Only populated in debug mode. */
    crop?: Uint8ClampedArray;
    cropSize?: number;
}
export interface MatchOptions {
    minScore: number;
    minMargin: number;
    minSharpness: number;
}
export declare const DEFAULT_MATCH_OPTIONS: MatchOptions;
export declare class CardMatcher {
    private session;
    private inputName;
    /** Reference vectors, PCA space, row-major [count x pcaDim], each unit length once scaled. */
    private index;
    private indexScale;
    private pcaMean;
    private pcaComponents;
    private pcaScale;
    cards: IndexCard[];
    config: IndexConfig | null;
    lastMatchMs: number;
    get ready(): boolean;
    load(base: string, fetchFile?: (url: string) => Promise<Response>): Promise<void>;
    dispose(): Promise<void>;
    /** Build the raw (pre-PCA) descriptor for one un-warped crop. */
    private describe;
    private project;
    /**
     * Identify one detected card.
     *
     * Always returns a result rather than null, carrying the best candidate and the reason
     * it was rejected. "No match" has several quite different causes - too blurry, too
     * weak, or torn between two look-alike cards - and they need different fixes, so
     * collapsing them all into null makes the thing undiagnosable from the outside.
     *
     * In `debug` mode the blur gate does not short-circuit and the crop is returned, so a
     * caller can show what the matcher actually saw and what it would have picked.
     */
    identify(frame: ImageData, corners: [number, number][], options?: Partial<MatchOptions>, debug?: boolean): Promise<MatchResult>;
}
