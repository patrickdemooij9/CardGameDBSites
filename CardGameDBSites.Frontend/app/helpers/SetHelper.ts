import type { SetViewModel } from "~/api/default";

export function GetBannerColor(set: SetViewModel): string {
  const color = set.bannerColor?.trim();
  if (!color) return "#1f2937";
  // Umbraco's color picker stores hex values without the leading #.
  return /^[0-9a-f]{3,8}$/i.test(color) ? `#${color}` : color;
}

export function GetReleaseDate(set: SetViewModel): Date | undefined {
  if (!set.releaseDate) return undefined;
  // The API sends dates without a zone; read them as UTC so no time zone shifts the day.
  const hasZone = /(Z|[+-]\d{2}:\d{2})$/.test(set.releaseDate);
  const date = new Date(hasZone ? set.releaseDate : `${set.releaseDate}Z`);
  // An empty Umbraco date comes through as 0001-01-01.
  return isNaN(date.getTime()) || date.getFullYear() < 1900 ? undefined : date;
}

export function FormatReleaseDate(set: SetViewModel): string | undefined {
  // Fixed locale and time zone so the server and client render the same text.
  return GetReleaseDate(set)?.toLocaleDateString("en-GB", {
    day: "numeric",
    month: "short",
    year: "numeric",
    timeZone: "UTC",
  });
}

export function FormatSetMeta(set: SetViewModel, showDate: boolean, showCardCount: boolean): string {
  return [
    showDate ? FormatReleaseDate(set) : undefined,
    showCardCount && set.cardCount > 0 ? `${set.cardCount} cards` : undefined,
  ]
    .filter(Boolean)
    .join(" · ");
}

export function IsUpcoming(set: SetViewModel): boolean {
  const date = GetReleaseDate(set);
  return !!date && date.getTime() > Date.now();
}

export function GetLatestSet(sets: SetViewModel[]): SetViewModel | undefined {
  let latest: SetViewModel | undefined;
  let latestTime = -Infinity;
  for (const set of sets) {
    const time = GetReleaseDate(set)?.getTime();
    if (time !== undefined && time > latestTime) {
      latest = set;
      latestTime = time;
    }
  }
  return latest;
}

export function GetProgressPercent(owned: number, total: number): number {
  return total > 0 ? (owned / total) * 100 : 0;
}
