import type { SetPriceHistoryItemApiModel, SetViewModel } from "~/api/default";
import { DoFetch, DoServerFetch } from "~/helpers/RequestsHelper";

export default class SetService {
  async getAllSets() {
    const result = await DoFetch<SetViewModel[]>("/api/sets/getAll", {
      method: "GET",
    });
    return result!;
  }

  async get(key: string) {
    return await DoFetch<SetViewModel>(
      `/api/sets/getByKey?key=${encodeURIComponent(key)}`,
      {
        method: "GET",
      },
    );
  }

  async getById(id: number) {
    return await DoFetch<SetViewModel>(`/api/sets/get?id=${id}`, {
      method: "GET",
    });
  }

  async getPriceHistory(setId: number) {
    return await DoServerFetch<SetPriceHistoryItemApiModel[]>(
      `/api/sets/priceHistory?setId=${setId}`,
      true,
    );
  }
}
