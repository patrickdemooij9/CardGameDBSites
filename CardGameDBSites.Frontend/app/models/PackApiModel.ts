export interface PackItemApiModel {
  id: string;
  variantTypeId?: number | null;
}

export interface PackPostApiModel {
  setId: number;
  items: PackItemApiModel[];
}

export interface PackVerifyCardApiModel {
  baseId: number;
  displayName: string;
  variantTypeId?: number | null;
}

export interface PackVerifySuccessApiModel {
  setId: number;
  cards: PackVerifyCardApiModel[];
}

export interface PackVerifyErrorApiModel {
  errorMessage: string;
  postContent: PackPostApiModel;
}
