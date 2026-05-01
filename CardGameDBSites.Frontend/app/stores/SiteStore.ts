import { defineStore } from 'pinia'
import type { SiteSettingsApiModel, SetOverviewSettingsApiModel, DeckTypeSettingsApiModel } from '~/api/default'

const SITE_TTL_MS = 10 * 60 * 1000 // 10 minutes

interface CachedItem<T> {
  data: T
  fetchedAt: number
}

export const useSiteStore = defineStore('site', {
  state: () => ({
    siteSettings: null as CachedItem<SiteSettingsApiModel> | null,
    setOverviewSettings: null as CachedItem<SetOverviewSettingsApiModel> | null,
    deckTypeSettings: {} as Record<number, CachedItem<DeckTypeSettingsApiModel>>,
  }),

  getters: {
    isExpired:
      () =>
      <T>(cached: CachedItem<T> | null): boolean =>
        !cached || Date.now() - cached.fetchedAt > SITE_TTL_MS,
  },

  actions: {
    setSiteSettings(data: SiteSettingsApiModel) {
      this.siteSettings = { data, fetchedAt: Date.now() }
    },
    setSetOverviewSettings(data: SetOverviewSettingsApiModel) {
      this.setOverviewSettings = { data, fetchedAt: Date.now() }
    },
    setDeckTypeSettings(typeId: number, data: DeckTypeSettingsApiModel) {
      this.deckTypeSettings[typeId] = { data, fetchedAt: Date.now() }
    },
  },
})
