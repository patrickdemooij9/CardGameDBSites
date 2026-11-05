import type { CollectionCardApiModel, SetProgressApiModel } from "~/api/default";

export type SetCollectionMode = "sets" | "cards";

export const useCollectionStore = defineStore("collection", {
  state: () => ({
    cards: {} as Record<number, CollectionCardApiModel[]>,
    // Stays undefined during SSR, so cached server HTML never contains collection data.
    setCollectionMode: undefined as SetCollectionMode | undefined,
    setsProgress: [] as SetProgressApiModel[],
    ownedSetIds: [] as number[],
  }),
  getters: {
    getCards: (state) => {
      return (cardId: number) => state.cards[cardId] ?? [];
    },
    amount: (state) => {
      return (cardId: number) => state.cards[cardId]?.reduce((sum, card) => sum + (card.amount || 0), 0) || 0;
    },
    getSetProgress: (state) => {
      return (setId: number) => state.setsProgress.find((it) => it.setId === setId);
    },
    isSetOwned: (state) => {
      return (setId: number) => state.ownedSetIds.includes(setId);
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
