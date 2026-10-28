import type { CardFilterCollectionApiModel, CardFilterLandingApiModel } from "~/api/default";
import { DoFetch } from "~/helpers/RequestsHelper";

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
