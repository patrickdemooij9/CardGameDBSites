import type { CardDetailApiModel } from "~/api/default";

export function GetCardValue<T>(
  card: CardDetailApiModel,
  abilityName: string,
): T | null {
  const value = card.attributes![abilityName];
  if (!value) return null;
  if (Array.isArray(value)) {
    return value[0] as T;
  }
  return value as T;
}

export function GetCardValues<T>(
  card: CardDetailApiModel,
  abilityName: string,
): T[] | null {
  const value = card.attributes![abilityName];
  if (!value) return null;
  return value as T[];
}
