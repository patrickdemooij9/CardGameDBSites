export interface CardListModel {
  id: number;
  name: string;
  description?: string | null;
  isPublic: boolean;
  itemCount: number;
  createdDate: string;
}

export interface CardListDetailModel {
  id: number;
  name: string;
  description?: string | null;
  isPublic: boolean;
  createdDate: string;
  ownerName?: string | null;
  items: CardListItemModel[];
}

export interface CardListItemModel {
  id: number;
  cardId: number;
  variantId?: number | null;
  amount: number;
  addedDate: string;
}

export interface CardInListModel {
  listId: number;
  itemId: number;
  variantId?: number | null;
  amount: number;
}

export interface CreateCardListPostModel {
  name: string;
  description?: string | null;
  isPublic: boolean;
}

export interface UpdateCardListPostModel {
  name: string;
  description?: string | null;
  isPublic: boolean;
}

export interface AddCardListItemPostModel {
  cardId: number;
  variantId?: number | null;
  amount: number;
}
