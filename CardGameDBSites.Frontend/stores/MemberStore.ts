import type { MemberApiModel } from "~/api/default";
import { DoFetch } from "~/helpers/RequestsHelper";

export const useMemberStore = defineStore("memberStore", {
  state: () => ({
    members: {} as { [key: number]: MemberApiModel },
  }),
  actions: {
    async loadMembers(memberIds: number[]) {
      const members = await DoFetch<MemberApiModel[]>("/api/member/byIds", {
        method: "POST",
        body: memberIds,
      });

      members.forEach((member) => {
        this.members[member.id] = member;
      });
      return members;
    },
  },
});
