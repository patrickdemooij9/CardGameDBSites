import type {
  CardDetailApiModel,
  CardsQueryPostApiModel,
  PagedResultCardDetailApiModel,
} from "~/api/default";

export default class CardService {
  async query(model: CardsQueryPostApiModel) {
    const result = await $fetch<PagedResultCardDetailApiModel>(
      "https://localhost:44344/api/cards/query",
      {
        method: "POST",
        body: model,
      }
    );
    return result!;
  }

  async getValues(ability: string) {
    const result = await $fetch<string[]>(
      "https://localhost:44344/api/cards/getAllValues?abilityName=" + ability,
      {
        method: "GET",
      }
    );
    return result!;
  }

  GetValue<T>(card: CardDetailApiModel, abilityName: string): T | null {
    const value = card.attributes![abilityName];
    if (!value) return null;
    if (Array.isArray(value)) {
      return value[0] as T;
    }
    return value as T;
  }

  GetValues<T>(card: CardDetailApiModel, abilityName: string): T[] | null {
    const value = card.attributes![abilityName];
    if (!value) return null;
    return value as T[];
  }
}
