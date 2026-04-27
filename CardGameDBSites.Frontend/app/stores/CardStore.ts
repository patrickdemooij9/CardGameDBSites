import { defineStore } from 'pinia'
import type { CardDetailApiModel, CardVariantTypeApiModel } from '~/api/default'

const CARD_TTL_MS = 10 * 60 * 1000 // 10 minutes

interface CachedCard {
  data: CardDetailApiModel
  fetchedAt: number
}

export const useCardsStore = defineStore('cards', {
  state: () => ({
    cards: {} as Record<string, CachedCard>,
    variantTypes: {
      data: [] as CardVariantTypeApiModel[],
      fetchedAt: null as number | null
    }
  }),

  getters: {
    getCard:
      (state) =>
      (id: string): CardDetailApiModel | null =>
        state.cards[id]?.data ?? null,

    isCardExpired:
      () =>
      (cached: CachedCard) =>
        Date.now() - cached.fetchedAt > CARD_TTL_MS,

    hasVariantTypes: state => state.variantTypes.data.length > 0,
    areVariantTypesExpired: state =>
      !state.variantTypes.fetchedAt ||
      Date.now() - state.variantTypes.fetchedAt > CARD_TTL_MS
  },

  actions: {
    setCards(cards: CardDetailApiModel[]) {
      const now = Date.now()
      cards.forEach(card => {
        if (!card.baseId) return
        this.cards[card.baseId] = {
          data: card,
          fetchedAt: now
        }
      })
    },

    setVariantTypes(types: CardVariantTypeApiModel[]) {
      this.variantTypes = {
        data: types,
        fetchedAt: Date.now()
      }
    }
  }
})
