export interface RankedRow {
  row: number
  score: number
}

export const EXCLUDED = -Infinity

// Best row per distinct card, so a card that owns many index rows still gets a real rival.
export function rankCards(
  scores: ArrayLike<number>,
  baseIds: ArrayLike<number>,
  limit: number,
): RankedRow[] {
  const ranked: RankedRow[] = []
  const seen = new Set<number>()

  while (ranked.length < limit) {
    let best = -1
    for (let row = 0; row < scores.length; row++) {
      if (scores[row] === EXCLUDED || seen.has(baseIds[row])) continue
      if (best < 0 || scores[row] > scores[best]) best = row
    }
    if (best < 0) break
    seen.add(baseIds[best])
    ranked.push({ row: best, score: scores[best] })
  }
  return ranked
}
