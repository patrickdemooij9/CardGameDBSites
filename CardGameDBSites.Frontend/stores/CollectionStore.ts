import type { CollectionCardApiModel } from "~/api/default";

export const useCollectionStore = defineStore("collection", {
  state: () => ({
    cards: {} as Record<number, CollectionCardApiModel[]>,
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
    setCards(cards: CollectionCardApiModel[]) {
      const cardsGrouped = Object.groupBy(cards, (card) => card.cardId!);
      Object.entries(cardsGrouped).forEach((entry) => {
        this.cards[Number(entry[0])] = entry[1]!;
      });
    },
    clearCards() {
      this.cards = {};
    }
  },
});