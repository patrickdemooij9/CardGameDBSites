import { defineStore } from "pinia";
import type { MemberApiModel } from "~/api/default";

const MEMBER_TTL_MS = 10 * 60 * 1000;

interface CachedMember {
  data: MemberApiModel;
  fetchedAt: number;
}

export const useMemberStore = defineStore("memberStore", {
  state: () => ({
    members: {} as Record<number, CachedMember>,
  }),
  getters: {
    isMemberExpired:
      () =>
      (cached: CachedMember): boolean =>
        Date.now() - cached.fetchedAt > MEMBER_TTL_MS,
    getMember:
      (state) =>
      (id: number): MemberApiModel | null =>
        state.members[id]?.data ?? null,
  },
  actions: {
    setMembers(members: MemberApiModel[]) {
      const now = Date.now();
      members.forEach((member) => {
        this.members[member.id] = {
          data: member,
          fetchedAt: now,
        };
      });
    },
  },
});
