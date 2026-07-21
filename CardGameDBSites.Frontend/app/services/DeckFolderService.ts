import type {
  CreateDeckFolderPostModel,
  DeckFolderApiModel,
  MoveDecksPostModel,
  UpdateDeckFolderPostModel,
} from "~/api/default";
import { DoServerFetch } from "~/helpers/RequestsHelper";

export default class DeckFolderService {
  async getByUser() {
    const result = await DoServerFetch<DeckFolderApiModel[]>(
      "/api/deckFolders/getByUser",
      true,
    );
    return result ?? [];
  }

  async create(name: string, description?: string) {
    const body: CreateDeckFolderPostModel = { name, description };
    return await DoServerFetch<number>("/api/deckFolders/create", true, {
      method: "POST",
      body,
    });
  }

  async update(id: number, name: string, description?: string) {
    const body: UpdateDeckFolderPostModel = { id, name, description };
    await DoServerFetch("/api/deckFolders/update", true, {
      method: "PUT",
      body,
    });
  }

  async delete(id: number) {
    await DoServerFetch(`/api/deckFolders/delete?id=${id}`, true, {
      method: "DELETE",
    });
  }

  async moveDecks(folderId: number | null, deckIds: number[]) {
    const body: MoveDecksPostModel = { folderId, deckIds };
    await DoServerFetch("/api/deckFolders/moveDecks", true, {
      method: "POST",
      body,
    });
  }
}
