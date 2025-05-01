import type { CardsQueryPostApiModel, PagedResultCardDetailApiModel } from "~/api/default";

export default class CardService {
    async query(model: CardsQueryPostApiModel) {
        const result = await $fetch<PagedResultCardDetailApiModel>("https://localhost:44344/api/cards/query", {
            method: "POST",
            body: model
        });
        return result!;
      }

    async getValues(ability: string) {
        const result = await $fetch<string[]>("https://localhost:44344/api/cards/getAllValues?abilityName=" + ability, {
            method: "GET"
        });
        return result!;
    }
}