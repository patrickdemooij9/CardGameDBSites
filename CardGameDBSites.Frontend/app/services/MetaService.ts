import type { DeckApiModel, ImageCropsApiModel } from "~/api/default";
import { DoFetch } from "~/helpers/RequestsHelper";

export type MetaCardStatApiModel = {
  cardId: number;
  deckCount: number;
  usagePercentage: number;
  winratePercentage: number;
};

export type MetaCardApiModel = {
  baseId: number;
  displayName: string;
  urlSegment: string;
  imageUrl?: ImageCropsApiModel;
};

export default class MetaService {
  /** The meta detail page URL for a card, or null when the card isn't a leader. */
  async getCardMetaUrl(cardId: number): Promise<string | null> {
    const result = await DoFetch<{ url: string | null }>(
      `/api/meta/card-page-url?cardId=${cardId}`
    );
    return result?.url ?? null;
  }

  /** Resolves the card a MetaCardDetail page is about from the requested URL path. */
  async resolveCard(path: string): Promise<MetaCardApiModel | null> {
    return DoFetch<MetaCardApiModel | null>(
      `/api/meta/resolve-card?path=${encodeURIComponent(path)}`
    );
  }

  /** Usage/winrate for the given cards in a period (cards without snapshot data report zeros). */
  async getCardStats(periodId: number, cardIds: number[]): Promise<MetaCardStatApiModel[]> {
    const query = cardIds.map((id) => `cardIds=${id}`).join("&");
    return DoFetch<MetaCardStatApiModel[]>(
      `/api/meta/card-stats?periodId=${periodId}${query ? `&${query}` : ""}`
    );
  }

  /** Top tournament decks piloting a leader in a period (best finish first). */
  async getTopDecks(periodId: number, cardId: number, count: number): Promise<DeckApiModel[]> {
    return DoFetch<DeckApiModel[]>(
      `/api/meta/top-decks?periodId=${periodId}&cardId=${cardId}&count=${count}`
    );
  }
}
