import type {
  CardDetailApiModel,
  DeckApiModel,
  DeckTypeSettingsApiModel,
  SiteSettingsApiModel,
} from "~/api/default";
import {
  bodyCards,
  buildRenderModel,
  type DeckRenderModel,
} from "~/services/deckImage/model";
import {
  canvasToBlob,
  renderDeckImage,
  type RenderResult,
} from "~/services/deckImage/renderer";
import {
  availableTiers,
  selectTier,
  type DeckImageTier,
} from "~/services/deckImage/tiers";

export interface DeckImageInput {
  deck: DeckApiModel;
  cards: CardDetailApiModel[];
  settings: SiteSettingsApiModel;
  deckTypeSettings: DeckTypeSettingsApiModel;
  /** Base ids matching the deck type's main-card rule. */
  heroCardIds: number[];
  /** Absolute page URL, shown in the footer. */
  deckUrl: string;
}

export const TIER_LABELS: Record<DeckImageTier, string> = {
  fullFace: "Card grid",
  artCrop: "Compact",
  list: "Text list",
};

/**
 * Renders a deck image in the browser.
 *
 * Client-side because it costs no server or Cloudflare Worker CPU, and because a live
 * preview lets the player pick a density — which is the part that actually gets the image
 * posted.
 */
export function useDeckImage(input: DeckImageInput) {
  const config = input.settings.renderConfig;

  const model: DeckRenderModel = buildRenderModel({
    deck: input.deck,
    cards: input.cards,
    groupings: input.deckTypeSettings.groupings ?? [],
    heroCardIds: input.heroCardIds,
    config,
    deckUrl: input.deckUrl,
    siteName: input.settings.siteName
  });

  const tiers = availableTiers(config);
  const defaultTier = selectTier(bodyCards(model).length, config);

  const tier = ref<DeckImageTier>(defaultTier);
  const isRendering = ref(false);
  const previewUrl = ref<string | null>(null);
  const error = ref<string | null>(null);
  const result = ref<RenderResult | null>(null);

  let objectUrl: string | null = null;

  const revoke = () => {
    if (objectUrl) {
      URL.revokeObjectURL(objectUrl);
      objectUrl = null;
    }
  };

  const render = async () => {
    isRendering.value = true;
    error.value = null;
    try {
      const rendered = await renderDeckImage(model, tier.value);
      const blob = await canvasToBlob(rendered.canvas);

      revoke();
      objectUrl = URL.createObjectURL(blob);
      previewUrl.value = objectUrl;
      result.value = rendered;
    } catch (e) {
      // The usual cause is a tainted canvas: card art served without CORS headers.
      error.value =
        e instanceof Error ? e.message : "Could not build the deck image";
    } finally {
      isRendering.value = false;
    }
  };

  const download = () => {
    if (!previewUrl.value) return;
    const link = document.createElement("a");
    link.href = previewUrl.value;
    link.download = `${model.title.replace(/[^\w\- ]+/g, "").trim() || "deck"}.webp`;
    link.click();
  };

  watch(tier, render);
  onScopeDispose(revoke);

  return {
    model,
    tiers,
    tier,
    isRendering,
    previewUrl,
    error,
    result,
    render,
    download,
  };
}

export default useDeckImage;
