import type {
  CardDetailApiModel,
  CardVariantTypeApiModel,
  CardsQueryPostApiModel,
  PagedResultCardDetailApiModel,
} from "~/api/default";
import { DoFetch, DoServerFetch } from "~/helpers/RequestsHelper";

export function useCards() {
  const store = useCardsStore();

  /** Load multiple cards with cache + dedupe */
  const loadCardsByIds = async (ids: number[]) => {
    const missingIds = ids.filter((id) => {
      const cached = store.cards[id];
      return !cached || store.isCardExpired(cached);
    });

    if (missingIds.length === 0) {
      return ids.map((id) => store.cards[id].data);
    }

    const { data, error } = await useAsyncData(
      `cards-${missingIds.join(",")}`,
      () =>
        DoServerFetch<CardDetailApiModel[]>("/api/cards/byIds", true, {
          method: "POST",
          body: missingIds,
        }),
    );

    if (error.value) throw error.value;

    store.setCards(data.value ?? []);

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

    const { data, error } = await useAsyncData(`card-${id}`, () =>
      DoServerFetch<CardDetailApiModel>(`/api/cards/byId?id=${id}`, true),
    );

    if (error.value) throw error.value;

    if (data.value) {
      store.setCards([data.value]);
    }

    return data.value!;
  };

  /** Variant types (cached globally) */
  const loadVariantTypes = async () => {
    if (store.hasVariantTypes && !store.areVariantTypesExpired) {
      return store.variantTypes.data;
    }

    const { data, error } = await useAsyncData("card-variant-types", () =>
      DoFetch<CardVariantTypeApiModel[]>("/api/cards/variantTypes", {
        method: "GET",
      }),
    );

    if (error.value) throw error.value;

    store.setVariantTypes(data.value ?? []);
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
    const { data, error } = await useAsyncData(`card-values-${ability}`, () =>
      DoFetch<string[]>("/api/cards/getAllValues?abilityName=" + ability, {
        method: "GET",
      }),
    );

    if (error.value) throw error.value;

    return data.value ?? [];
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
