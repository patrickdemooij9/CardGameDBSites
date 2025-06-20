import { defineStore } from "pinia";
import type { CardDetailApiModel } from "~/api/default";
import { DoFetch } from "~/helpers/RequestsHelper";

export const useCardsStore = defineStore("cardStore", {
  state: () => ({
    cards: {} as { [key: string]: CardDetailApiModel },
  }),
  actions: {
    async loadCards(cardIds: number[]) {
      const cards = await DoFetch<CardDetailApiModel[]>(
        "/api/cards/byIds",
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

    async loadCard(cardId: string){
      const card = await DoFetch<CardDetailApiModel>(
        "/api/cards/byId?id=" + cardId,
        {
          method: "GET"
        }
      );
      return card;
    }
  },
});
