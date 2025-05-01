import { defineStore } from "pinia";
import type { CardDetailApiModel } from "~/api/default";

export const useCardsStore = defineStore("cardStore", {
  state: () => ({
    cards: {} as { [key: string]: CardDetailApiModel },
  }),
  actions: {
    async loadCards(cardIds: number[]) {
      const cards = await $fetch<CardDetailApiModel[]>(
        "https://localhost:44344/api/cards/byIds",
        {
          method: "POST",
          body: cardIds,
        }
      );

      cards.forEach((card) => {
        this.cards[card.baseId!] = card;
      });
      return cards;
    },
  },
});
