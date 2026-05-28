import type {
  CardListModel,
  CardListDetailModel,
  CardListItemModel,
  CardInListModel,
  CreateCardListPostModel,
  UpdateCardListPostModel,
  AddCardListItemPostModel
} from "~/models/CardListModel";
import { DoServerFetch } from "~/helpers/RequestsHelper";
import { useCardListStore } from "~/stores/CardListStore";

export function useCardLists() {
  const store = useCardListStore();

  const loadLists = async (): Promise<CardListModel[]> => {
    if (store.lists) return store.lists;

    const lists = await DoServerFetch<CardListModel[]>("/api/cardlists", true);
    store.setLists(lists);
    return lists;
  };

  const getList = async (id: number): Promise<CardListDetailModel> => {
    return await DoServerFetch<CardListDetailModel>(`/api/cardlists/${id}`, true);
  };

  const getPublicList = async (id: number): Promise<CardListDetailModel> => {
    return await DoServerFetch<CardListDetailModel>(`/api/cardlists/${id}/public`);
  };

  const createList = async (model: CreateCardListPostModel): Promise<CardListModel> => {
    const list = await DoServerFetch<CardListModel>("/api/cardlists", true, {
      method: "POST",
      body: model
    });
    store.addList(list);
    return list;
  };

  const updateList = async (id: number, model: UpdateCardListPostModel): Promise<void> => {
    await DoServerFetch(`/api/cardlists/${id}`, true, {
      method: "PUT",
      body: model
    });
    store.updateList(id, model.name, model.description, model.isPublic);
  };

  const deleteList = async (id: number): Promise<void> => {
    await DoServerFetch(`/api/cardlists/${id}`, true, {
      method: "DELETE"
    });
    store.removeList(id);
  };

  const addItem = async (listId: number, model: AddCardListItemPostModel): Promise<CardListItemModel> => {
    return await DoServerFetch<CardListItemModel>(`/api/cardlists/${listId}/items`, true, {
      method: "POST",
      body: model
    });
  };

  const removeItem = async (listId: number, itemId: number): Promise<void> => {
    await DoServerFetch(`/api/cardlists/${listId}/items/${itemId}`, true, {
      method: "DELETE"
    });
  };

  const getCardStatus = async (cardId: number): Promise<CardInListModel[]> => {
    return await DoServerFetch<CardInListModel[]>("/api/cardlists/cardStatus", true, {
      method: "POST",
      body: JSON.stringify(cardId),
      headers: { "Content-Type": "application/json" }
    });
  };

  return {
    loadLists,
    getList,
    getPublicList,
    createList,
    updateList,
    deleteList,
    addItem,
    removeItem,
    getCardStatus
  };
}
