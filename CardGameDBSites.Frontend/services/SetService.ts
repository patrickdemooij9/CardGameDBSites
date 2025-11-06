import type { SetViewModel } from "~/api/default";
import { DoFetch } from "~/helpers/RequestsHelper";

export default class SetService {
    async getAllSets() {
        const result = await DoFetch<SetViewModel[]>(
          "/api/sets/getAll",
          {
            method: "GET",
          }
        );
        return result!;
    }

    async get(key: string){
      return await DoFetch<SetViewModel>(`/api/sets/getByKey?key=${encodeURIComponent(key)}`, {
        method: "GET",
      });
    }
}