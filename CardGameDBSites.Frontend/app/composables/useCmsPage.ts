import { defineAsyncComponent, type Component } from "vue";
import { DoFetch } from "~/helpers/RequestsHelper";
import type { ApiContentModel } from "~/models/ApiContentModel";

const pageComponents: { [key: string]: Component } = {
  cardOverview: defineAsyncComponent(() => import("~/components/pageTypes/CardOverviewPage.vue")),
  contentPage: defineAsyncComponent(() => import("~/components/pageTypes/contentPage.vue")),
  deckDetail: defineAsyncComponent(() => import("~/components/pageTypes/DeckDetail.vue")),
  deckOverview: defineAsyncComponent(() => import("~/components/pageTypes/DeckOverviewPage.vue")),
  homepage: defineAsyncComponent(() => import("~/components/pageTypes/homepage.vue")),
  card: defineAsyncComponent(() => import("~/components/pageTypes/CardDetailPage.vue")),
  cardVariant: defineAsyncComponent(() => import("~/components/pageTypes/CardDetailPage.vue")),
  createSquad: defineAsyncComponent(() => import("~/components/pageTypes/CreateDeck.vue")),
  login: defineAsyncComponent(() => import("~/components/pageTypes/LoginPage.vue")),
  accountDecks: defineAsyncComponent(() => import("~/components/pageTypes/AccountDecks.vue")),
  register: defineAsyncComponent(() => import("~/components/pageTypes/RegisterPage.vue")),
  forgotPassword: defineAsyncComponent(() => import("~/components/pageTypes/ForgotPasswordPage.vue")),
  setOverview: defineAsyncComponent(() => import("~/components/pageTypes/SetOverviewPage.vue")),
  set: defineAsyncComponent(() => import("~/components/pageTypes/SetPage.vue")),
  collectionPage: defineAsyncComponent(() => import("~/components/pageTypes/CollectionPage.vue")),
  blogOverview: defineAsyncComponent(() => import("~/components/pageTypes/BlogOverviewPage.vue")),
  blogDetail: defineAsyncComponent(() => import("~/components/pageTypes/BlogDetailPage.vue")),
  dailyGame: defineAsyncComponent(() => import("~/components/pageTypes/DailyGamePage.vue")),
  proxyCards: defineAsyncComponent(() => import("~/components/pageTypes/ProxyCardsPage.vue")),
  accountCards: defineAsyncComponent(() => import("~/components/pageTypes/AccountCards.vue")),
  metaPage: defineAsyncComponent(() => import("~/components/pageTypes/MetaPage.vue")),
  metaCardOverview: defineAsyncComponent(() => import("~/components/pageTypes/MetaCardOverviewPage.vue")),
  metaCardDetail: defineAsyncComponent(() => import("~/components/pageTypes/MetaCardDetailPage.vue")),
};

export function ToContentSlug(url: string) {
  return url.replace(/^\/+|\/+$/g, "");
}

export function useCmsPage() {
  const resolveComponent = (contentType: string) => pageComponents[contentType];

  const loadContent = (url: string) =>
    DoFetch<ApiContentModel>(`/umbraco/delivery/api/v2/content/item/${ToContentSlug(url)}`);

  return { resolveComponent, loadContent };
}
