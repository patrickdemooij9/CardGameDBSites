export function getDeckDetailUrl(overviewUrl: string | null | undefined, deckId: number) {
  const baseUrl = (overviewUrl?.trim() || "/decks").replace(/\/$/, "");
  return `${baseUrl}/${deckId}`;
}

export function getEditDeckUrl(createDeckUrl: string | null | undefined, deckId: number) {
  const baseUrl = (createDeckUrl?.trim() || "/create-deck").replace(/\/$/, "");
  return `${baseUrl}?id=${deckId}`;
}
