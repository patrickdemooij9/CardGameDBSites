import type { DeckRenderConfigApiModel } from "~/api/default";


export type DeckImageTier = "fullFace" | "artCrop" | "list";
export type DeckImageLayout = "flatGrid" | "groupRows";

/** Output width in pixels. Height is content-driven. */
export const CANVAS_WIDTH = 1080;

/** Side margin used by every layout. */
export const CANVAS_PADDING = 60;

/** Largest group that still reads as a row rather than a grid. */
const MAX_GROUP_SIZE_FOR_ROWS = 6;

/** Uniform cell aspect for art crops, since bands differ in shape per card type. */
export const ART_CROP_ASPECT = 1.32;

export interface GridSpec {
  columns: number;
  cellWidth: number;
  cellHeight: number;
  gap: number;
}

export interface GroupRowSpec {
  cardWidth: number;
  cardHeight: number;
  gap: number;
}

/**
 * True when the deck is built from several small groups (Shatterpoint: N squads of three)
 * rather than one flat pile (Star Wars Unlimited, Skytear Horde).
 */
export function isStructured(groupSizes: number[]): boolean {
  return (
    groupSizes.length > 1 &&
    groupSizes.every((size) => size > 0 && size <= MAX_GROUP_SIZE_FOR_ROWS)
  );
}

export function selectLayout(groupSizes: number[]): DeckImageLayout {
  return isStructured(groupSizes) ? "groupRows" : "flatGrid";
}

/** Tiers this game can actually produce. Art crops need per-game bands authored. */
export function availableTiers(config: DeckRenderConfigApiModel): DeckImageTier[] {
  const tiers: DeckImageTier[] = ["fullFace"];
  if ((config.artCrops?.length ?? 0) > 0) {
    tiers.push("artCrop");
  }
  tiers.push("list");
  return tiers;
}

/**
 * Picks the tier for a deck. `override` lets the player choose in the preview, but is
 * ignored when that tier is unavailable for the game.
 */
export function selectTier(
  bodyCardCount: number,
  config: DeckRenderConfigApiModel,
  override?: DeckImageTier,
): DeckImageTier {
  if (override && availableTiers(config).includes(override)) {
    return override;
  }
  if (bodyCardCount >= config.tierCMin!) {
    return "list";
  }
  if (bodyCardCount >= config.tierBMin! && config.artCrops!.length > 0) {
    return "artCrop";
  }
  return "fullFace";
}

/** Column count for the flat-grid layouts, widening as the deck grows. */
export function columnsFor(tier: DeckImageTier, cardCount: number): number {
  if (tier === "artCrop") {
    return cardCount > 55 ? 7 : 5;
  }
  if (tier === "list") {
    return cardCount > 45 ? 3 : 2;
  }
  return 5;
}

/** Cell geometry for a flat grid of full card faces or art crops. */
export function gridSpec(
  tier: DeckImageTier,
  cardCount: number,
  cardAspect: number,
): GridSpec {
  const columns = columnsFor(tier, cardCount);
  const gap = tier === "artCrop" ? 11 : 14;
  const usable = CANVAS_WIDTH - CANVAS_PADDING * 2 - gap * (columns - 1);
  const cellWidth = usable / columns;
  const cellHeight =
    tier === "artCrop" ? cellWidth / ART_CROP_ASPECT : cellWidth / cardAspect;

  return { columns, cellWidth, cellHeight, gap };
}

/**
 * Card geometry for the grouped layout. Cards shrink as groups stack up so the canvas does
 * not run away — a 2-squad team gets 300px cards, a 4-squad team 252px.
 *
 * The curve is calibrated against Shatterpoint, the only structured game today (2–4 groups).
 * Revisit if a game ships a larger structured format.
 */
export function groupRowSpec(
  groupCount: number,
  cardAspect: number,
): GroupRowSpec {
  const cardWidth = Math.round(
    Math.min(300, Math.max(210, 340 - groupCount * 22)),
  );
  return {
    cardWidth,
    cardHeight: Math.round(cardWidth / cardAspect),
    gap: 20,
  };
}

/** Output is never shorter than this, so a 7-card deck cannot render as a squat banner. */
export function minCanvasHeight(config: DeckRenderConfigApiModel): number {
  return Math.round(CANVAS_WIDTH * Math.max(1, config.minCanvasRatio!));
}
