import type { CollectionCardApiModel, DeckProgressApiModel } from "~/api/default";
import { DoServerFetch } from "~/helpers/RequestsHelper";
import type { PackPostApiModel, PackVerifySuccessApiModel, PackVerifyErrorApiModel } from "~/models/PackApiModel";
import type { PresetApiModel } from "~/models/PresetApiModel";
import { useCollectionStore } from "~/stores/CollectionStore";

export function useCollection() {
  const store = useCollectionStore();

  const loadCards = async (cardIds: number[]) => {
    if (cardIds.length === 0) return {};

    const cachedIds = cardIds.filter(id => store.cards[id]?.length);
    const missingIds = cardIds.filter(id => !store.cards[id]?.length);

    if (missingIds.length === 0) {
      const result: Record<number, CollectionCardApiModel[]> = {};
      cachedIds.forEach(id => {
        result[id] = store.cards[id]!;
      });
      return result;
    }

    const cards = await DoServerFetch<CollectionCardApiModel[]>(
      "/api/collection/cards",
      true,
      {
        method: "POST",
        body: missingIds
      }
    );

    store.setCards(cards);

    const cardsGrouped = Object.groupBy(cards, (card) => card.cardId!);
    return cardsGrouped;
  };

  const loadDecksProgress = async (deckIds: number[]) => {
    if (deckIds.length === 0) return [];

    const progress = await DoServerFetch<DeckProgressApiModel[]>(
      "/api/collection/decksProgress",
      true,
      {
        method: "POST",
        body: deckIds
      }
    );
    return progress;
  }

  const saveCards = async (cardId: number, values: { [key: number]: number }) => {
    const cards = await DoServerFetch<CollectionCardApiModel[]>(
      "/api/collection/addCards",
      true,
      {
        method: "POST",
        body: values,
        query: {
          cardId
        }
      }
    );
    store.setCards(cards);
  };

  const verifyPack = async (postModel: PackPostApiModel): Promise<PackVerifySuccessApiModel | PackVerifyErrorApiModel> => {
    return await DoServerFetch<PackVerifySuccessApiModel | PackVerifyErrorApiModel>(
      "/api/collection/verifyPack",
      true,
      {
        method: "POST",
        body: postModel
      }
    );
  };

  const addPack = async (postModel: PackPostApiModel): Promise<void> => {
    await DoServerFetch(
      "/api/collection/addPack",
      true,
      {
        method: "POST",
        body: postModel
      }
    );
  };

  const getPresets = async (): Promise<PresetApiModel[]> => {
    return await DoServerFetch<PresetApiModel[]>("/api/collection/presets");
  };

  const applyPreset = async (presetId: string): Promise<void> => {
    await DoServerFetch(
      "/api/collection/addPreset",
      true,
      {
        method: "POST",
        query: { presetId }
      }
    );
  };

  const getCards = (cardId: number) => store.getCards(cardId);
  const getAmount = (cardId: number) => store.amount(cardId);

  return {
    loadCards,
    loadDecksProgress,
    saveCards,
    verifyPack,
    addPack,
    getPresets,
    applyPreset,
    getCards,
    getAmount,
  };
}