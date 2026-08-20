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

export type MetaTierName = "S" | "A" | "B" | "C" | "D" | "Unranked";

export type MetaTierLeaderApiModel = {
  cardId: number;
  name: string;
  tier: MetaTierName;
  deckCount: number;
  eventCount: number;
  wins: number;
  losses: number;
  draws: number;
  top8Count: number;
  firstPlaceCount: number;
  /** Game win rate, not match win rate. */
  winratePercentage: number;
  metaSharePercentage: number;
  /** Percentage points vs the previous window. Null when the sample was too thin. */
  metaShareDeltaPoints?: number | null;
  winrateDeltaPoints?: number | null;
  isNewEntry: boolean;
  unrankedReason?: string | null;
  metaUrl?: string | null;
  imageUrl?: ImageCropsApiModel;
};

export type MetaTierListApiModel = {
  periodId: number;
  periodName: string;
  firstEventUtc?: string | null;
  lastEventUtc?: string | null;
  /** Newest tournament date, not "now". */
  lastUpdatedUtc?: string | null;
  totalDecks: number;
  totalEvents: number;
  totalEntrants: number;
  entrantsWithDeck: number;
  deltasAvailable: boolean;
  deltasUnavailableReason?: string | null;
  deltaComparedToUtc?: string | null;
  deltaWeeks: number;
  minDecks: number;
  minEvents: number;
  leaders: MetaTierLeaderApiModel[];
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

  /** Omit periodId for the current period. */
  async getTierList(periodId?: number): Promise<MetaTierListApiModel> {
    return DoFetch<MetaTierListApiModel>(
      `/api/meta/tier-list${periodId != null ? `?periodId=${periodId}` : ""}`
    );
  }

  /** Top tournament decks piloting a leader in a period (best finish first). */
  async getTopDecks(periodId: number, cardId: number, count: number): Promise<DeckApiModel[]> {
    return DoFetch<DeckApiModel[]>(
      `/api/meta/top-decks?periodId=${periodId}&cardId=${cardId}&count=${count}`
    );
  }
}
