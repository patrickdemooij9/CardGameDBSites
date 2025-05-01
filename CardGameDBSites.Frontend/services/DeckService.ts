import { type DeckApiModel, type DeckQueryPostModel, type PagedResultDeckApiModel } from "~/api/default";

export default class DeckService {

  async get(id: number) {
    const result = await useFetch<DeckApiModel>("https://localhost:44344/api/decks/get?id=" + id, {
      query: {
        id
      }
    });
    return result.data.value!;
  }

  async query(model: DeckQueryPostModel) {
    const result = await useFetch<PagedResultDeckApiModel>("https://localhost:44344/api/decks/query", {
        method: "POST",
        body: model
    });
    return result.data.value!;
  }
}
