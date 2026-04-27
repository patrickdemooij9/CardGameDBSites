import type {
  DeckQueryPostModel,
  PagedResultDeckApiModel,
} from "~/api/default";
import { DoOptionalServerFetch } from "~/helpers/RequestsHelper";

function useDecks() {
  const query = async (model: DeckQueryPostModel) => {
    console.time("Deck query" + model.orderBy);
    const result = await DoOptionalServerFetch<PagedResultDeckApiModel>(
      "/api/decks/query",
      {
        method: "POST",
        body: model,
      },
    );
    console.timeEnd("Deck query" + model.orderBy);
    return result!;
  };

  const toUniqueKey = (model: DeckQueryPostModel) => {
    return `decks-${model.typeId}-${model.page}-${model.take}-${model.status}-${model.orderBy}-{${model.cards?.join(",")}}-${model.userId}-${model.dateFrom}-${model.dateTo}`;
  };

  return {
    query,
    toUniqueKey,
  };
}

export default useDecks;
