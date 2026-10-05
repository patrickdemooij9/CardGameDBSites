/**
 * Flatten a detected card quadrilateral back into a square, upright image.
 *
 * Canvas can only do affine transforms, so the perspective warp is done by hand: for
 * each output pixel, map back through the inverse homography and bilinearly sample the
 * source. That is also what OpenCV's warpPerspective does, which matters - the Python
 * side builds the reference index and evaluates accuracy with cv2, so the browser has
 * to agree with it or every match degrades for reasons that would be very hard to find.
 */
/**
 * Extract a card from a frame given its four corners.
 *
 * `corners` must be ordered top-left, top-right, bottom-right, bottom-left in the
 * card's own frame - the ordering the detector was trained to produce - which is what
 * makes the result upright rather than merely flat.
 *
 * Returns RGB bytes (3 per pixel, no alpha) of a `size` x `size` image.
 */
export declare function unwarpCard(source: ImageData, corners: [number, number][], size: number): Uint8ClampedArray;
/** Aspect ratio of a detected quad, used to tell portrait cards from landscape ones. */
export declare function quadOrientation(corners: [number, number][]): 'portrait' | 'landscape';
/**
 * Variance of the Laplacian, as a focus measure.
 *
 * A motion-blurred crop will still produce *some* nearest neighbour, and a confidently
 * wrong identification is worse than none, so blurry crops are rejected before they
 * ever reach the index.
 */
export declare function sharpness(rgb: Uint8ClampedArray, size: number): number;
