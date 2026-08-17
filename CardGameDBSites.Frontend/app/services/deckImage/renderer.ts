import {
  ART_CROP_ASPECT,
  CANVAS_PADDING,
  CANVAS_WIDTH,
  gridSpec,
  groupRowSpec,
  minCanvasHeight,
  selectLayout,
  type DeckImageTier,
} from "./tiers";
import {
  bodyCards,
  findArtCrop,
  imageUrl,
  sameOriginMedia,
  type DeckRenderModel,
  type RenderCard,
} from "./model";

/**
 * Canvas renderer for the deck image.
 *
 * Laid out in two passes: `planLayout` positions everything and returns the total height,
 * then `paint` draws it. Height is content-driven rather than predicted, which is what keeps
 * a 7-card strike team and an 88-card Twin Suns deck both looking deliberate.
 */

const FONT_STACK = '"Open Sans", "Helvetica Neue", Helvetica, Arial, sans-serif';
const font = (weight: number, size: number) => `${weight} ${size}px ${FONT_STACK}`;

const BG_TOP = "#0B1020";
const BG_BOTTOM = "#1A2244";
const TEXT = "#FFFFFF";
const MUTED = "#9AA3C0";
const INK = "#20263F";
const CHIP = "#2A3358";

const FOOTER_HEIGHT = 118;
/** Clearance reserved below the last content block so the footer never overlaps it. */
const FOOTER_CLEARANCE = 150;

export type Op =
  | { kind: "card"; card: RenderCard; x: number; y: number; w: number; h: number; crop: boolean }
  | { kind: "sectionLabel"; text: string; meta: string | null; y: number }
  | { kind: "listRow"; card: RenderCard; x: number; y: number; w: number }
  | { kind: "mission"; label: string; text: string; y: number; h: number };

export interface LayoutPlan {
  height: number;
  headerBottom: number;
  titleLines: string[];
  titleSize: number;
  metaText: string;
  ops: Op[];
}

// ---- text helpers ---------------------------------------------------------

function fitTitle(
  ctx: CanvasRenderingContext2D,
  text: string,
  maxWidth: number,
): { lines: string[]; size: number } {
  for (const size of [60, 54, 48, 42, 38]) {
    ctx.font = font(800, size);
    if (ctx.measureText(text).width <= maxWidth) {
      return { lines: [text], size };
    }
  }

  // Still too wide: break onto two lines at the best word boundary.
  const size = 42;
  ctx.font = font(800, size);
  const words = text.split(" ");
  let best = { lines: [text], diff: Number.MAX_VALUE };
  for (let i = 1; i < words.length; i++) {
    const a = words.slice(0, i).join(" ");
    const b = words.slice(i).join(" ");
    const diff = Math.abs(ctx.measureText(a).width - ctx.measureText(b).width);
    if (diff < best.diff) best = { lines: [a, b], diff };
  }
  return { lines: best.lines, size };
}

function ellipsize(
  ctx: CanvasRenderingContext2D,
  text: string,
  maxWidth: number,
): string {
  if (ctx.measureText(text).width <= maxWidth) return text;
  let clipped = text;
  while (clipped.length > 1 && ctx.measureText(`${clipped}…`).width > maxWidth) {
    clipped = clipped.slice(0, -1);
  }
  return `${clipped.trim()}…`;
}

function letterspaced(
  ctx: CanvasRenderingContext2D,
  text: string,
  x: number,
  y: number,
  spacing: number,
  align: "left" | "center",
) {
  const chars = [...text];
  const total =
    chars.reduce((sum, c) => sum + ctx.measureText(c).width, 0) +
    spacing * (chars.length - 1);
  let cursor = align === "center" ? x - total / 2 : x;
  for (const char of chars) {
    ctx.fillText(char, cursor, y);
    cursor += ctx.measureText(char).width + spacing;
  }
}

function roundRect(
  ctx: CanvasRenderingContext2D,
  x: number,
  y: number,
  w: number,
  h: number,
  r: number,
) {
  ctx.beginPath();
  if (typeof ctx.roundRect === "function") {
    ctx.roundRect(x, y, w, h, r);
  } else {
    const radius = Math.min(r, w / 2, h / 2);
    ctx.moveTo(x + radius, y);
    ctx.arcTo(x + w, y, x + w, y + h, radius);
    ctx.arcTo(x + w, y + h, x, y + h, radius);
    ctx.arcTo(x, y + h, x, y, radius);
    ctx.arcTo(x, y, x + w, y, radius);
    ctx.closePath();
  }
}

// ---- layout ---------------------------------------------------------------

export function planLayout(
  ctx: CanvasRenderingContext2D,
  model: DeckRenderModel,
  tier: DeckImageTier,
): LayoutPlan {
  const { config } = model;
  const body = bodyCards(model);
  const layout = selectLayout(model.groups.map((g) => g.cards.length));
  const ops: Op[] = [];
  const contentWidth = CANVAS_WIDTH - CANVAS_PADDING * 2;

  // --- header
  const title = fitTitle(ctx, model.title, contentWidth);
  let y = 44 + 22; // top padding + eyebrow
  y += title.size * title.lines.length + 12;
  y += 5 + 16; // accent rule
  y += 30; // meta line

  const metaText =
    layout === "groupRows"
      ? `${model.groups.length} squads · ${model.uniqueCount} cards`
      : `${model.uniqueCount} unique · ${model.totalCount} cards`;

  const headerBottom = y;
  y += 28;

  // --- heroes
  // Every tier shows these: they are excluded from the body groups, so skipping the row
  // would silently drop them from the image while the header still counted them.
  if (model.heroCards.length > 0) {
    const heroHeight = tier === "list" ? 210 : tier === "artCrop" ? 140 : 150;
    let heroWidth = Math.round(heroHeight * config.cardAspect!);
    let landscapeWidth = Math.round(heroHeight * 1.395);
    const widthOf = (card: RenderCard) =>
      card.landscape ? landscapeWidth : heroWidth;

    // Shrink if the row would overflow.
    const gap = 20;
    let total =
      model.heroCards.reduce((sum, c) => sum + widthOf(c), 0) +
      gap * (model.heroCards.length - 1);
    if (total > contentWidth) {
      const scale = contentWidth / total;
      heroWidth = Math.round(heroWidth * scale);
      landscapeWidth = Math.round(landscapeWidth * scale);
      total = contentWidth;
    }

    let x = CANVAS_PADDING + (contentWidth - total) / 2;
    for (const card of model.heroCards) {
      const w = widthOf(card);
      const h = Math.round(card.landscape ? w / 1.395 : w / config.cardAspect!);
      ops.push({ kind: "card", card, x, y: y + (heroHeight - h), w, h, crop: false });
      x += w + gap;
    }
    y += heroHeight + 28;
  }

  // --- body
  if (tier === "list") {
    const columns = body.length > 45 ? 3 : 2;
    const rowHeight = 40;
    const colGap = 26;
    const colWidth = (contentWidth - colGap * (columns - 1)) / columns;
    const rows = Math.ceil(body.length / columns);

    ops.push({ kind: "sectionLabel", text: "Decklist", meta: null, y });
    y += 38;

    const ordered = [...body].sort(
      (a, b) =>
        a.type.localeCompare(b.type) ||
        (parseInt(a.cost, 10) || 0) - (parseInt(b.cost, 10) || 0) ||
        a.name.localeCompare(b.name),
    );

    // Column-major so the list reads down each column, not across.
    ordered.forEach((card, index) => {
      const col = Math.floor(index / rows);
      const row = index % rows;
      ops.push({
        kind: "listRow",
        card,
        x: CANVAS_PADDING + col * (colWidth + colGap),
        y: y + row * rowHeight,
        w: colWidth,
      });
    });
    y += rows * rowHeight;
  } else if (layout === "groupRows") {
    const spec = groupRowSpec(model.groups.length, config.cardAspect!);

    for (const group of model.groups) {
      const points = group.cards.reduce(
        (sum, c) => sum + Math.max(0, parseInt(c.cost, 10) || 0),
        0,
      );
      ops.push({
        kind: "sectionLabel",
        text: group.header,
        meta: points > 0 ? `${points} SP` : null,
        y,
      });
      y += 36;

      const total =
        spec.cardWidth * group.cards.length + spec.gap * (group.cards.length - 1);
      let x = CANVAS_PADDING + (contentWidth - total) / 2;
      for (const card of group.cards) {
        ops.push({
          kind: "card",
          card,
          x,
          y,
          w: spec.cardWidth,
          h: spec.cardHeight,
          crop: false,
        });
        x += spec.cardWidth + spec.gap;
      }
      y += spec.cardHeight + 26;
    }
  } else {
    const crop = tier === "artCrop";
    const spec = gridSpec(tier, body.length, config.cardAspect!);

    for (const group of model.groups) {
      if (model.groups.length > 1 || group.header) {
        ops.push({ kind: "sectionLabel", text: group.header, meta: null, y });
        y += 38;
      }

      group.cards.forEach((card, index) => {
        const col = index % spec.columns;
        const row = Math.floor(index / spec.columns);
        ops.push({
          kind: "card",
          card,
          x: CANVAS_PADDING + col * (spec.cellWidth + spec.gap),
          y: y + row * (spec.cellHeight + spec.gap),
          w: spec.cellWidth,
          h: spec.cellHeight,
          crop,
        });
      });

      const rows = Math.ceil(group.cards.length / spec.columns);
      y += rows * (spec.cellHeight + spec.gap) + 14;
    }
  }

  // --- text groups (a Shatterpoint mission, and anything shaped like one)
  if (model.textCards.length > 0) {
    const height = 78;
    ops.push({
      kind: "mission",
      label: "Mission",
      text: model.textCards.map((c) => c.shortName).join(" · "),
      y: y + 6,
      h: height,
    });
    y += height + 12;
  }

  const height = Math.max(
    Math.round(y + FOOTER_CLEARANCE),
    minCanvasHeight(config),
  );

  return { height, headerBottom, titleLines: title.lines, titleSize: title.size, metaText, ops };
}

// ---- image loading --------------------------------------------------------

function loadImage(src: string): Promise<HTMLImageElement | null> {
  return new Promise((resolve) => {
    const img = new Image();
    // Required or the canvas is tainted and toBlob() fails.
    img.crossOrigin = "anonymous";
    img.onload = () => resolve(img);
    img.onerror = () => resolve(null);
    img.src = src;
  });
}

async function loadCardImages(
  ops: Op[],
  model: DeckRenderModel,
  scale: number,
): Promise<Map<string, HTMLImageElement | null>> {
  const wanted = new Map<string, string>();

  for (const op of ops) {
    if (op.kind !== "card") continue;
    const crop = op.crop ? findArtCrop(model.config, op.card.type) : null;
    const key = `${op.card.cardId}:${op.crop}`;
    // Request roughly 2x the drawn size: sharper than letting the browser downscale
    // an original, and far lighter on memory for an 88-card deck.
    wanted.set(
      key,
      sameOriginMedia(imageUrl(op.card.faceUrl, { width: op.w * scale, crop })),
    );
  }

  const entries = await Promise.all(
    [...wanted.entries()].map(async ([key, url]) => {
      const img = await loadImage(url);
      return [key, img] as const;
    }),
  );
  return new Map(entries);
}

// ---- painting -------------------------------------------------------------

function paintBackground(
  ctx: CanvasRenderingContext2D,
  model: DeckRenderModel,
  height: number,
) {
  const gradient = ctx.createLinearGradient(0, 0, 0, height);
  gradient.addColorStop(0, BG_TOP);
  gradient.addColorStop(1, BG_BOTTOM);
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, CANVAS_WIDTH, height);

  // A soft wash of the deck's dominant colour, so identity shows without a rainbow.
  const dominant = model.aspectPips[0];
  if (dominant) {
    const glow = ctx.createRadialGradient(
      CANVAS_WIDTH / 2, 120, 0,
      CANVAS_WIDTH / 2, 120, 620,
    );
    glow.addColorStop(0, `${dominant}55`);
    glow.addColorStop(1, `${dominant}00`);
    ctx.fillStyle = glow;
    ctx.fillRect(0, 0, CANVAS_WIDTH, 620);
  }
}

function paintHeader(
  ctx: CanvasRenderingContext2D,
  model: DeckRenderModel,
  plan: LayoutPlan,
) {
  const centre = CANVAS_WIDTH / 2;
  ctx.textBaseline = "top";
  ctx.textAlign = "left";

  ctx.font = font(600, 15);
  ctx.fillStyle = MUTED;
  letterspaced(ctx, model.eyebrow.toUpperCase(), centre, 44, 3.4, "center");

  ctx.textAlign = "center";
  ctx.font = font(800, plan.titleSize);
  ctx.fillStyle = TEXT;
  let y = 44 + 22;
  for (const line of plan.titleLines) {
    ctx.fillText(line, centre, y);
    y += plan.titleSize;
  }

  y += 12;
  ctx.fillStyle = "#E8913A";
  roundRect(ctx, centre - 75, y, 150, 5, 3);
  ctx.fill();
  y += 5 + 16;

  ctx.font = font(500, 20);
  const pipSize = 22;
  const pipGap = 9;
  const textWidth = ctx.measureText(plan.metaText).width;
  const pipsWidth = model.aspectPips.length
    ? model.aspectPips.length * pipSize + model.aspectPips.length * pipGap
    : 0;
  let x = centre - (pipsWidth + textWidth) / 2;

  for (const color of model.aspectPips) {
    ctx.beginPath();
    ctx.arc(x + pipSize / 2, y + 12, pipSize / 2, 0, Math.PI * 2);
    ctx.fillStyle = color;
    ctx.fill();
    ctx.strokeStyle = "rgba(255,255,255,0.22)";
    ctx.lineWidth = 2;
    ctx.stroke();
    x += pipSize + pipGap;
  }

  ctx.textAlign = "left";
  ctx.fillStyle = MUTED;
  ctx.fillText(plan.metaText, x, y);
}

function paintCard(
  ctx: CanvasRenderingContext2D,
  op: Extract<Op, { kind: "card" }>,
  image: HTMLImageElement | null,
  accent: string,
) {
  const radius = op.crop ? 7 : 8;

  ctx.save();
  ctx.shadowColor = "rgba(0,0,0,0.55)";
  ctx.shadowBlur = 14;
  ctx.shadowOffsetY = 5;
  ctx.fillStyle = CHIP;
  roundRect(ctx, op.x, op.y, op.w, op.h, radius);
  ctx.fill();
  ctx.restore();

  if (image) {
    ctx.save();
    roundRect(ctx, op.x, op.y, op.w, op.h, radius);
    ctx.clip();
    // Cover the cell: crops vary in shape per card type, faces are already correct.
    const scale = Math.max(op.w / image.width, op.h / image.height);
    const dw = image.width * scale;
    const dh = image.height * scale;
    ctx.drawImage(image, op.x + (op.w - dw) / 2, op.y + (op.h - dh) / 2, dw, dh);
    ctx.restore();
  }

  if (op.crop) {
    // Own name plate over a scrim, so the cell never depends on the card's printed layout.
    ctx.save();
    roundRect(ctx, op.x, op.y, op.w, op.h, radius);
    ctx.clip();
    const scrim = ctx.createLinearGradient(0, op.y + op.h * 0.45, 0, op.y + op.h);
    scrim.addColorStop(0, "rgba(4,7,16,0)");
    scrim.addColorStop(1, "rgba(4,7,16,0.94)");
    ctx.fillStyle = scrim;
    ctx.fillRect(op.x, op.y + op.h * 0.45, op.w, op.h * 0.55);

    ctx.font = font(700, Math.max(10, Math.round(op.w * 0.108)));
    ctx.fillStyle = TEXT;
    ctx.textAlign = "center";
    ctx.textBaseline = "bottom";
    ctx.fillText(
      ellipsize(ctx, op.card.shortName, op.w - 12),
      op.x + op.w / 2,
      op.y + op.h - 6,
    );
    ctx.restore();
  }

  if (op.card.amount > 1) {
    const size = Math.round(op.w * (op.crop ? 0.26 : 0.24));
    const bx = op.x + op.w - size - 5;
    const by = op.crop ? op.y + 5 : op.y + op.h - size - 5;

    ctx.fillStyle = accent;
    roundRect(ctx, bx, by, size, size, 5);
    ctx.fill();

    ctx.font = font(800, Math.round(size * 0.58));
    ctx.fillStyle = INK;
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(String(op.card.amount), bx + size / 2, by + size / 2 + 1);
  }
}

function paintSectionLabel(
  ctx: CanvasRenderingContext2D,
  op: Extract<Op, { kind: "sectionLabel" }>,
  accent: string,
) {
  ctx.textBaseline = "top";
  ctx.textAlign = "left";
  ctx.font = font(700, 19);
  ctx.fillStyle = accent;
  letterspaced(ctx, op.text.toUpperCase(), CANVAS_PADDING, op.y, 2.8, "left");

  const labelWidth =
    ctx.measureText(op.text.toUpperCase()).width + 2.8 * op.text.length;
  let ruleEnd = CANVAS_WIDTH - CANVAS_PADDING;

  if (op.meta) {
    ctx.font = font(700, 19);
    ctx.fillStyle = MUTED;
    ctx.textAlign = "right";
    ctx.fillText(op.meta, CANVAS_WIDTH - CANVAS_PADDING, op.y);
    ruleEnd -= ctx.measureText(op.meta).width + 14;
    ctx.textAlign = "left";
  }

  const ruleStart = CANVAS_PADDING + labelWidth + 14;
  if (ruleEnd > ruleStart) {
    ctx.fillStyle = "rgba(255,255,255,0.14)";
    ctx.fillRect(ruleStart, op.y + 11, ruleEnd - ruleStart, 1);
  }
}

function paintListRow(
  ctx: CanvasRenderingContext2D,
  op: Extract<Op, { kind: "listRow" }>,
  model: DeckRenderModel,
) {
  const rowHeight = 40;
  ctx.fillStyle = "rgba(255,255,255,0.06)";
  ctx.fillRect(op.x, op.y + rowHeight - 1, op.w, 1);

  const centreY = op.y + rowHeight / 2 - 1;
  let x = op.x;

  if (op.card.cost) {
    ctx.beginPath();
    ctx.arc(x + 13, centreY, 13, 0, Math.PI * 2);
    ctx.fillStyle = CHIP;
    ctx.fill();

    ctx.font = font(800, 15);
    ctx.fillStyle = TEXT;
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(op.card.cost, x + 13, centreY + 1);
    x += 34;
  }

  const aspectColor = model.config.aspectColors![op.card.aspects[0] ?? ""];
  if (aspectColor) {
    ctx.beginPath();
    ctx.arc(x + 4.5, centreY, 4.5, 0, Math.PI * 2);
    ctx.fillStyle = aspectColor;
    ctx.fill();
    x += 17;
  }

  ctx.textAlign = "right";
  ctx.textBaseline = "middle";
  ctx.font = font(700, 16);
  const amount = op.card.amount > 1 ? `×${op.card.amount}` : "";
  const amountWidth = amount ? ctx.measureText(amount).width + 10 : 0;
  if (amount) {
    ctx.fillStyle = "#E8913A";
    ctx.fillText(amount, op.x + op.w, centreY);
  }

  ctx.textAlign = "left";
  ctx.font = font(500, 18);
  ctx.fillStyle = TEXT;
  ctx.fillText(
    ellipsize(ctx, op.card.shortName, op.w - (x - op.x) - amountWidth),
    x,
    centreY,
  );
}

function paintMission(
  ctx: CanvasRenderingContext2D,
  op: Extract<Op, { kind: "mission" }>,
  accent: string,
) {
  const width = CANVAS_WIDTH - CANVAS_PADDING * 2;
  ctx.fillStyle = "rgba(255,255,255,0.055)";
  roundRect(ctx, CANVAS_PADDING, op.y, width, op.h, 12);
  ctx.fill();
  ctx.strokeStyle = "rgba(255,255,255,0.10)";
  ctx.lineWidth = 1;
  ctx.stroke();

  const centreY = op.y + op.h / 2;
  ctx.textBaseline = "middle";
  ctx.textAlign = "left";

  ctx.font = font(700, 15);
  ctx.fillStyle = accent;
  const label = op.label.toUpperCase();
  letterspaced(ctx, label, CANVAS_PADDING + 28, centreY, 3.6, "left");
  const labelWidth = ctx.measureText(label).width + 3.6 * label.length;

  ctx.font = font(600, 27);
  ctx.fillStyle = TEXT;
  ctx.fillText(
    ellipsize(ctx, op.text, width - labelWidth - 76),
    CANVAS_PADDING + 28 + labelWidth + 20,
    centreY,
  );
}

function paintFooter(
  ctx: CanvasRenderingContext2D,
  model: DeckRenderModel,
  height: number,
) {
  const top = height - FOOTER_HEIGHT;
  const fade = ctx.createLinearGradient(0, top, 0, height);
  fade.addColorStop(0, "rgba(0,0,0,0)");
  fade.addColorStop(1, "rgba(0,0,0,0.62)");
  ctx.fillStyle = fade;
  ctx.fillRect(0, top, CANVAS_WIDTH, FOOTER_HEIGHT);

  ctx.textAlign = "left";
  ctx.textBaseline = "alphabetic";
  ctx.font = font(800, 27);

  const parts: Array<[string, string]> = [
    [model.siteName, TEXT]
  ];
  let x = CANVAS_PADDING;
  const brandY = top + 52;
  for (const [text, color] of parts) {
    if (!text) continue;
    ctx.fillStyle = color;
    ctx.fillText(text, x, brandY);
    x += ctx.measureText(text).width;
  }

  ctx.font = font(500, 19);
  ctx.fillStyle = MUTED;
  ctx.fillText(model.deckUrl, CANVAS_PADDING, brandY + 30);
}

// ---- entry point ----------------------------------------------------------

export interface RenderResult {
  canvas: HTMLCanvasElement;
  width: number;
  height: number;
  /** Cards whose art failed to load; they render as placeholder chips. */
  missingImages: number;
}

export async function renderDeckImage(
  model: DeckRenderModel,
  tier: DeckImageTier,
  scale = 2,
): Promise<RenderResult> {
  // Canvas text silently falls back to a different face if fonts are not ready yet.
  if (typeof document !== "undefined" && document.fonts?.ready) {
    await document.fonts.ready;
  }

  const canvas = document.createElement("canvas");
  const measureCtx = canvas.getContext("2d");
  if (!measureCtx) throw new Error("Could not get a 2D canvas context");

  const plan = planLayout(measureCtx, model, tier);
  const images = await loadCardImages(plan.ops, model, scale);

  canvas.width = CANVAS_WIDTH * scale;
  canvas.height = plan.height * scale;

  const ctx = canvas.getContext("2d")!;
  ctx.scale(scale, scale);

  const accent = "#E8913A";
  paintBackground(ctx, model, plan.height);
  paintHeader(ctx, model, plan);

  let missingImages = 0;
  for (const op of plan.ops) {
    switch (op.kind) {
      case "card": {
        const image = images.get(`${op.card.cardId}:${op.crop}`) ?? null;
        if (!image) missingImages++;
        paintCard(ctx, op, image, accent);
        break;
      }
      case "sectionLabel":
        paintSectionLabel(ctx, op, accent);
        break;
      case "listRow":
        paintListRow(ctx, op, model);
        break;
      case "mission":
        paintMission(ctx, op, accent);
        break;
    }
  }

  paintFooter(ctx, model, plan.height);

  return { canvas, width: CANVAS_WIDTH, height: plan.height, missingImages };
}

export function canvasToBlob(
  canvas: HTMLCanvasElement,
  type = "image/webp",
  quality = 0.92,
): Promise<Blob> {
  return new Promise((resolve, reject) => {
    canvas.toBlob(
      (blob) =>
        blob
          ? resolve(blob)
          : reject(new Error("Could not encode the deck image")),
      type,
      quality,
    );
  });
}

export { ART_CROP_ASPECT };
