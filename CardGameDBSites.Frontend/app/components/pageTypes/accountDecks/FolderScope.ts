/** Which subset of the user's decks the account decks overview is currently showing. */
export type FolderScope =
  | { type: "all" }
  | { type: "unfiled" }
  | { type: "folder"; folderId: number };
