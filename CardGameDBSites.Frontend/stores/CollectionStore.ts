import type { CollectionCardApiModel } from "~/api/default";
import { DoFetch, DoServerFetch } from "~/helpers/RequestsHelper";

export const useCollectionStore = defineStore("collectionStore", {
  state: () => ({
    cards: {} as { [key: number]: CollectionCardApiModel[] },
  }),
  getters: {
    getCards: (state) => {
        return (cardId: number) => state.cards[cardId] ?? [];
    },
    amount: (state) => {
        return (cardId: number) => state.cards[cardId]?.reduce((sum, card) => sum + (card.amount || 0), 0) || 0;
    }
  },
  actions: {
    async loadCards(cardIds: number[]) {
      const cards = await DoServerFetch<CollectionCardApiModel[]>(
        "/api/collection/cards",
        true,
        {
          method: "POST",
          body: cardIds
        }
      );

      const cardsGrouped = Object.groupBy(cards, (card) => card.cardId!);
      Object.entries(cardsGrouped).forEach((entry) => {
        this.cards[Number(entry[0])] = entry[1]!;
      });
      return cardsGrouped;
    },

    async save(values: {[key: number]: number}){
      const cards = await DoServerFetch<CollectionCardApiModel[]>(
        "/api/collection/addCards",
        true,
        {
          method: "POST",
          body: values
        }
      )
      const cardsGrouped = Object.groupBy(cards, (card) => card.cardId!);
      Object.entries(cardsGrouped).forEach((entry) => {
        this.cards[Number(entry[0])] = entry[1]!;
      });
    }
  },
});
