const LOCALE = "en-GB";

export function formatNumber(value: number): string {
  return value.toLocaleString(LOCALE);
}

export function formatDay(value: string | null | undefined): string {
  if (!value) return "";
  return new Date(value).toLocaleDateString(LOCALE, {
    day: "numeric",
    month: "short",
    year: "numeric",
  });
}

export function formatLongDay(value: string | null | undefined): string {
  if (!value) return "";
  return new Date(value).toLocaleDateString(LOCALE, {
    day: "numeric",
    month: "long",
    year: "numeric",
  });
}

export function formatMonthYear(value: string | null | undefined): string {
  if (!value) return "";
  return new Date(value).toLocaleDateString(LOCALE, { month: "long", year: "numeric" });
}
