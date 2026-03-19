import type { MemberApiModel } from "~/api/default";
import { DoFetch, DoFetch2, DoServerFetch } from "~/helpers/RequestsHelper";

export function useMembers() {
  const store = useMemberStore();
  const { $api } = useNuxtApp()

  const loadMembersByIds = async (ids: number[]): Promise<MemberApiModel[]> => {
    const uniqueIds = [...new Set(ids)];
    const missingIds = uniqueIds.filter((id) => {
      const cached = store.members[id];
      return !cached || store.isMemberExpired(cached);
    });

    if (missingIds.length === 0) {
      return uniqueIds.map((id) => store.members[id].data);
    }

    const data = await DoFetch2<MemberApiModel[]>(
      $api,
      "/api/member/byIds",
      {
        method: "POST",
        body: missingIds,
      },
    );

    store.setMembers(data ?? []);

    return uniqueIds
      .map((id) => store.members[id]?.data)
      .filter(Boolean) as MemberApiModel[];
  };

  const getMemberById = (id: number): MemberApiModel | null => {
    return store.getMember(id);
  };

  return {
    loadMembersByIds,
    getMemberById,
  };
}
