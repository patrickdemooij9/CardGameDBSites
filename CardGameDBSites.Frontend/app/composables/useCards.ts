import type {
  CardDetailApiModel,
  CardVariantTypeApiModel,
  CardsQueryPostApiModel,
  PagedResultCardDetailApiModel,
} from "~/api/default";
import { DoServerFetch } from "~/helpers/RequestsHelper";
import { useCardsStore } from "~/stores/CardStore";

export function useCards() {
  const store = useCardsStore();
  const { $api } = useNuxtApp();

  /** Load multiple cards with cache + dedupe */
  const loadCardsByIds = async (ids: number[]) => {
    const missingIds = ids.filter((id) => {
      const cached = store.cards[id];
      return !cached || store.isCardExpired(cached);
    });

    if (missingIds.length === 0) {
      return ids.map((id) => store.cards[id]!.data);
    }

    const data = await $api<CardDetailApiModel[]>("/api/cards/byIds", {
      method: "POST",
      body: missingIds,
    });

    store.setCards(data ?? []);

    return ids
      .map((id) => store.cards[id]?.data)
      .filter(Boolean) as CardDetailApiModel[];
  };

  /** Load a single card. TODO: I should get rid of this and always use the number id everywhere */
  const loadCardById = async (id: string) => {
    const cached = store.cards[id];
    if (cached && !store.isCardExpired(cached)) {
      return cached.data;
    }

    const data = await $api<CardDetailApiModel>(`/api/cards/byId?id=${id}`);

    if (data) {
      store.setCards([data]);
    }

    return data;
  };

  /** Variant types (cached globally) */
  const loadVariantTypes = async () => {
    if (store.hasVariantTypes && !store.areVariantTypesExpired) {
      return store.variantTypes.data;
    }

    const data = await $api<CardVariantTypeApiModel[]>(
      "/api/cards/variantTypes",
      { method: "GET" },
    );

    store.setVariantTypes(data ?? []);
    return store.variantTypes.data;
  };

  /** Query cards (no store cache – intentional) */
  const queryCards = async (model: CardsQueryPostApiModel) => {
    return await DoServerFetch<PagedResultCardDetailApiModel>("/api/cards/query", true, {
      method: "POST",
      body: model,
    });
  };

  const getAbilityValues = async (ability: string) => {
    const data = await $api<string[]>("/api/cards/getAllValues", {
      method: "GET",
      query: { abilityName: ability },
    });

    return data ?? [];
  };

  return {
    loadCardsByIds,
    loadCardById,
    loadVariantTypes,
    queryCards,
    getAbilityValues,
    getCard: store.getCard,
  };
}
