import type { CardFilterCollectionApiModel, CardFilterLandingApiModel } from "~/api/default";
import type { IApiContentModelBase } from "~/api/umbraco";
import { DoFetch } from "~/helpers/RequestsHelper";
import type { CardFilterPageContentModel } from "~/models/CardFilterPageContentModel";

const fetchCollections = () =>
  DoFetch<CardFilterCollectionApiModel[]>("/api/cards/filterLandings").catch(() => []);
const options = { default: (): CardFilterCollectionApiModel[] => [] };

export type CardFilterLandingMatch = {
  collection: CardFilterCollectionApiModel;
  landing: CardFilterLandingApiModel;
};

export function NormalizePath(path: string) {
  return path.replace(/\/+$/, "").toLowerCase();
}

export function ToGeneratedLandingContent(
  landing: CardFilterLandingApiModel,
  baseContent: IApiContentModelBase,
): CardFilterPageContentModel {
  return {
    ...baseContent,
    name: landing.name,
    route: { ...baseContent.route, path: landing.url },
    properties: {},
  };
}

export async function useCardFilterLandings() {
  const { data } = await useAsyncData("card-filter-landings", fetchCollections, options);
  const collections = computed(() => data.value ?? []);

  function findLanding(attribute: string, value: string): CardFilterLandingMatch | undefined {
    const normalizedValue = value.toLowerCase();
    for (const collection of collections.value) {
      if (collection.attribute !== attribute) continue;
      const landing = collection.landings.find((it) => it.value.toLowerCase() === normalizedValue);
      if (landing) return { collection, landing };
    }
    return undefined;
  }

  function findByUrl(url: string): CardFilterLandingMatch | undefined {
    const path = NormalizePath(url);
    for (const collection of collections.value) {
      const landing = collection.landings.find((it) => NormalizePath(it.url) === path);
      if (landing) return { collection, landing };
    }
    return undefined;
  }

  function findCollectionByUrl(url: string) {
    const path = NormalizePath(url);
    return collections.value.find((it) => NormalizePath(it.url) === path);
  }

  return { collections, findLanding, findByUrl, findCollectionByUrl };
}
