/**
 * Flatten a detected card quadrilateral back into a square, upright image.
 *
 * Canvas can only do affine transforms, so the perspective warp is done by hand: for
 * each output pixel, map back through the inverse homography and bilinearly sample the
 * source. That is also what OpenCV's warpPerspective does, which matters - the Python
 * side builds the reference index and evaluates accuracy with cv2, so the browser has
 * to agree with it or every match degrades for reasons that would be very hard to find.
 */

/** Grey used where a sample falls outside the source frame. Matches MATTE in Python. */
const MATTE = 128

/**
 * Solve for the 3x3 homography taking `src` to `dst`, as a plain 8-unknown linear
 * system with h22 fixed at 1. Gaussian elimination with partial pivoting is plenty
 * stable for four well-separated card corners.
 */
function homography(src: [number, number][], dst: [number, number][]): number[] {
  const a: number[][] = []
  const b: number[] = []

  for (let i = 0; i < 4; i++) {
    const [x, y] = src[i]
    const [u, v] = dst[i]
    a.push([x, y, 1, 0, 0, 0, -x * u, -y * u])
    b.push(u)
    a.push([0, 0, 0, x, y, 1, -x * v, -y * v])
    b.push(v)
  }

  const n = 8
  for (let col = 0; col < n; col++) {
    let pivot = col
    for (let row = col + 1; row < n; row++) {
      if (Math.abs(a[row][col]) > Math.abs(a[pivot][col])) pivot = row
    }
    ;[a[col], a[pivot]] = [a[pivot], a[col]]
    ;[b[col], b[pivot]] = [b[pivot], b[col]]

    const lead = a[col][col]
    if (Math.abs(lead) < 1e-12) continue

    for (let row = 0; row < n; row++) {
      if (row === col) continue
      const factor = a[row][col] / lead
      if (factor === 0) continue
      for (let k = col; k < n; k++) a[row][k] -= factor * a[col][k]
      b[row] -= factor * b[col]
    }
  }

  const h = new Array(9).fill(0)
  for (let i = 0; i < n; i++) h[i] = Math.abs(a[i][i]) < 1e-12 ? 0 : b[i] / a[i][i]
  h[8] = 1
  return h
}

/**
 * Extract a card from a frame given its four corners.
 *
 * `corners` must be ordered top-left, top-right, bottom-right, bottom-left in the
 * card's own frame - the ordering the detector was trained to produce - which is what
 * makes the result upright rather than merely flat.
 *
 * Returns RGB bytes (3 per pixel, no alpha) of a `size` x `size` image.
 */
export function unwarpCard(
  source: ImageData,
  corners: [number, number][],
  size: number,
): Uint8ClampedArray {
  // Invert by solving destination -> source directly; no matrix inversion needed.
  const dst: [number, number][] = [[0, 0], [size, 0], [size, size], [0, size]]
  const inverse = homography(dst, corners)

  const out = new Uint8ClampedArray(size * size * 3)
  const { data, width: sw, height: sh } = source

  for (let v = 0; v < size; v++) {
    for (let u = 0; u < size; u++) {
      // Sample at pixel centres, matching OpenCV's convention.
      const su = u + 0.5
      const sv = v + 0.5
      const w = inverse[6] * su + inverse[7] * sv + inverse[8]
      const x = (inverse[0] * su + inverse[1] * sv + inverse[2]) / w - 0.5
      const y = (inverse[3] * su + inverse[4] * sv + inverse[5]) / w - 0.5

      const o = (v * size + u) * 3

      const x0 = Math.floor(x)
      const y0 = Math.floor(y)
      if (x0 < -1 || y0 < -1 || x0 > sw - 1 || y0 > sh - 1) {
        out[o] = out[o + 1] = out[o + 2] = MATTE
        continue
      }

      const fx = x - x0
      const fy = y - y0
      const x1 = Math.min(x0 + 1, sw - 1)
      const y1 = Math.min(y0 + 1, sh - 1)
      const cx0 = Math.max(x0, 0)
      const cy0 = Math.max(y0, 0)

      const i00 = (cy0 * sw + cx0) * 4
      const i10 = (cy0 * sw + x1) * 4
      const i01 = (y1 * sw + cx0) * 4
      const i11 = (y1 * sw + x1) * 4

      const w00 = (1 - fx) * (1 - fy)
      const w10 = fx * (1 - fy)
      const w01 = (1 - fx) * fy
      const w11 = fx * fy

      for (let c = 0; c < 3; c++) {
        out[o + c] =
          data[i00 + c] * w00 + data[i10 + c] * w10 + data[i01 + c] * w01 + data[i11 + c] * w11
      }
    }
  }

  return out
}

/** Aspect ratio of a detected quad, used to tell portrait cards from landscape ones. */
export function quadOrientation(corners: [number, number][]): 'portrait' | 'landscape' {
  const [tl, tr, br, bl] = corners
  const width = (Math.hypot(tr[0] - tl[0], tr[1] - tl[1]) +
    Math.hypot(br[0] - bl[0], br[1] - bl[1])) / 2
  const height = (Math.hypot(bl[0] - tl[0], bl[1] - tl[1]) +
    Math.hypot(br[0] - tr[0], br[1] - tr[1])) / 2
  return width > height ? 'landscape' : 'portrait'
}

/**
 * Variance of the Laplacian, as a focus measure.
 *
 * A motion-blurred crop will still produce *some* nearest neighbour, and a confidently
 * wrong identification is worse than none, so blurry crops are rejected before they
 * ever reach the index.
 */
export function sharpness(rgb: Uint8ClampedArray, size: number): number {
  let sum = 0
  let sumSq = 0
  let count = 0

  for (let y = 1; y < size - 1; y++) {
    for (let x = 1; x < size - 1; x++) {
      const at = (yy: number, xx: number) => {
        const i = (yy * size + xx) * 3
        return (rgb[i] * 299 + rgb[i + 1] * 587 + rgb[i + 2] * 114) / 1000
      }
      const lap = at(y - 1, x) + at(y + 1, x) + at(y, x - 1) + at(y, x + 1) - 4 * at(y, x)
      sum += lap
      sumSq += lap * lap
      count++
    }
  }

  const mean = sum / count
  return sumSq / count - mean * mean
}
