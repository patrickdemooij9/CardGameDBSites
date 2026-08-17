import type {
  ArtCropApiModel,
  CardDetailApiModel,
  DeckApiModel,
  DeckCardGroupApiModel,
  DeckRenderConfigApiModel,
} from "~/api/default";
import { GetCardValue, GetCardValues } from "~/helpers/CardHelper";

/**
 * Turns a deck plus its loaded cards into a flat, render-ready model.
 *
 * All game-specific lookups happen here and nowhere else: which attribute holds the card
 * type, which types are landscape, which types have an art band. The renderer downstream
 * only sees resolved values.
 */

export interface RenderCard {
  cardId: number;
  name: string;
  /** Name before the first comma — SWU titles carry a subtitle the grid has no room for. */
  shortName: string;
  amount: number;
  groupId: number;
  slotId: number;
  type: string;
  cost: string;
  aspects: string[];
  landscape: boolean;
  /** Full card face, already resolved to the front or back image. */
  faceUrl: string;
  /** Illustration band, when this game has a band for this card type. */
  cropUrl: string | null;
}

export interface RenderGroup {
  groupId: number;
  header: string;
  cards: RenderCard[];
}

export interface DeckRenderModel {
  title: string;
  eyebrow: string;
  deckUrl: string;
  /** Cards matching the deck type's main-card rule — leaders, bases, or nothing. */
  heroCards: RenderCard[];
  /** Card groups that render as images. */
  groups: RenderGroup[];
  /** Cards from groups that render as text, such as a Shatterpoint mission. */
  textCards: RenderCard[];
  uniqueCount: number;
  totalCount: number;
  aspectPips: string[];
  config: DeckRenderConfigApiModel;
  siteName: string;
}

export interface BuildModelInput {
  deck: DeckApiModel;
  cards: CardDetailApiModel[];
  groupings: DeckCardGroupApiModel[];
  /** Base ids of hero cards, already resolved against the main-card requirements. */
  heroCardIds: number[];
  config: DeckRenderConfigApiModel;
  deckUrl: string;
  siteName: string;
}

/** Appends ImageSharp params, preserving any already on the URL. */
export function imageUrl(
  url: string,
  opts: { width: number; crop?: ArtCropApiModel | null },
): string {
  const params: string[] = [];
  if (opts.crop) {
    const { left, top, right, bottom } = opts.crop;
    params.push(`cc=${left},${top},${right},${bottom}`);
  }
  params.push(`width=${Math.round(opts.width)}`, "format=webp", "quality=80");

  const separator = url.includes("?") ? "&" : "?";
  return `${url}${separator}${params.join("&")}`;
}

/**
 * Rewrites absolute media URLs to the app's own origin.
 *
 * Card art is served from a different host than the page (api.sw-unlimited-db.com vs
 * sw-unlimited-db.com) and those hosts send no `Access-Control-Allow-Origin` header. Loading
 * it cross-origin therefore either fails outright — when `crossOrigin` is set — or taints the
 * canvas so `toBlob()` throws. The app already proxies `/media/**` from its own origin, so
 * point at that instead and the whole problem disappears.
 *
 * Adding CORS headers to the media host would let us drop this and save the proxy hop.
 */
export function sameOriginMedia(url: string): string {
  if (typeof window === "undefined") return url;
  try {
    const parsed = new URL(url, window.location.origin);
    if (parsed.origin === window.location.origin) return url;
    if (!parsed.pathname.startsWith("/media/")) return url;
    return `${parsed.pathname}${parsed.search}`;
  } catch {
    return url;
  }
}

export function findArtCrop(
  config: DeckRenderConfigApiModel,
  type: string,
): ArtCropApiModel | null {
  if (!type) return null;
  return config.artCrops?.find((crop) => crop.typeValue === type) ?? null;
}

function toRenderCard(
  card: CardDetailApiModel,
  deckCard: { groupId?: number; slotId?: number; amount?: number },
  config: DeckRenderConfigApiModel,
): RenderCard | null {
  const type = config.typeAttribute
    ? (GetCardValue<string>(card, config.typeAttribute) ?? "")
    : "";

  // Leaders and similar print their display face on the back of the card.
  const useBack = config.backImageTypes?.includes(type) ?? false;
  const face = (useBack ? card.backImageUrl : card.imageUrl) ?? card.imageUrl;
  if (!face?.url) return null;

  const cost = config.costAttribute
    ? (GetCardValue<string>(card, config.costAttribute) ?? "")
    : "";
  const aspects = config.aspectAttribute
    ? (GetCardValues<string>(card, config.aspectAttribute) ?? [])
    : [];

  const name = card.displayName ?? "";
  const crop = findArtCrop(config, type);

  return {
    cardId: card.baseId!,
    name,
    shortName: name.split(",")[0]!.trim(),
    amount: deckCard.amount ?? 1,
    groupId: deckCard.groupId ?? 0,
    slotId: deckCard.slotId ?? 0,
    type,
    cost,
    aspects,
    landscape: config.landscapeTypes?.includes(type) ?? false,
    faceUrl: face.url,
    cropUrl: crop ? face.url : null,
  };
}

/**
 * A group renders as text when none of its cards carry a type value — a Shatterpoint
 * mission has no Unit Type, while every unit does.
 *
 * This stands in for SquadConfig.DetailDisplayType, which the settings API does not expose
 * yet. It is guarded on the game having a type attribute at all, so a game with no attribute
 * configured does not collapse every group into text.
 */
export function isTextGroup(
  cards: RenderCard[],
  config: DeckRenderConfigApiModel,
): boolean {
  if (!config.typeAttribute) return false;
  return cards.length > 0 && cards.every((card) => !card.type);
}

export function buildRenderModel({
  deck,
  cards,
  groupings,
  heroCardIds,
  config,
  deckUrl,
  siteName
}: BuildModelInput): DeckRenderModel {
  const cardsById = new Map(cards.map((card) => [card.baseId, card]));

  const rendered: RenderCard[] = [];
  for (const deckCard of deck.cards ?? []) {
    const card = cardsById.get(deckCard.cardId);
    if (!card) continue;
    const renderCard = toRenderCard(card, deckCard, config);
    if (renderCard) rendered.push(renderCard);
  }

  const heroes = new Set(heroCardIds);
  const heroCards = rendered.filter((card) => heroes.has(card.cardId));
  const remaining = rendered.filter((card) => !heroes.has(card.cardId));

  // Group the non-hero cards, preserving the order groups appear in the deck.
  const byGroup = new Map<number, RenderCard[]>();
  for (const card of remaining) {
    const bucket = byGroup.get(card.groupId);
    if (bucket) bucket.push(card);
    else byGroup.set(card.groupId, [card]);
  }

  const groups: RenderGroup[] = [];
  const textCards: RenderCard[] = [];

  for (const [groupId, groupCards] of [...byGroup.entries()].sort(
    (a, b) => a[0] - b[0],
  )) {
    groupCards.sort((a, b) => a.slotId - b.slotId);

    if (isTextGroup(groupCards, config)) {
      textCards.push(...groupCards);
      continue;
    }

    groups.push({
      groupId,
      header:
        groupings.find((g) => g.groupId === groupId)?.header ??
        `Group ${groups.length + 1}`,
      cards: groupCards,
    });
  }

  const aspectPips: string[] = [];
  for (const card of rendered) {
    for (const aspect of card.aspects) {
      const color = config.aspectColors![aspect];
      if (color && !aspectPips.includes(color)) aspectPips.push(color);
    }
  }

  return {
    title: deck.name ?? "Untitled deck",
    eyebrow: config.eyebrow ?? "Deck",
    deckUrl,
    heroCards,
    groups,
    textCards,
    uniqueCount: rendered.length,
    totalCount: rendered.reduce((sum, card) => sum + card.amount, 0),
    aspectPips,
    config,
    siteName
  };
}

/** Cards that drive tier selection: everything except heroes and text groups. */
export function bodyCards(model: DeckRenderModel): RenderCard[] {
  return model.groups.flatMap((group) => group.cards);
}
