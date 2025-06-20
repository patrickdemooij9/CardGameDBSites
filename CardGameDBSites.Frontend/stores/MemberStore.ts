import type { CurrentMemberApiModel } from "~/api/default";
import { DoFetch } from "~/helpers/RequestsHelper";
import type MemberModel from "~/models/MemberModel";

export const useMemberStore = defineStore("memberStore", {
  state: () => ({
    member: undefined as MemberModel | undefined,
    validatedLogin: false as boolean,
  }),
  getters: {
    isLoggedIn: (state) => {
      return state.member !== undefined;
    },
  },
  actions: {
    async login(email: string, password: string, rememberMe: boolean) {
      const config = useRuntimeConfig();

      try {
        const member = await $fetch<CurrentMemberApiModel>(
          `${config.public.API_BASE_URL}/api/member/login`,
          {
            method: "POST",
            body: { email, password, rememberMe },
            credentials: "include",
          }
        );
        this.member = {
          id: member.id,
          name: member.displayName,
        };
      } catch (error) {
        throw "Incorrect email/password";
      }
    },
    async validateLogin() {
      if (this.validatedLogin) {
        return;
      }

      try {
        const result = await DoFetch<CurrentMemberApiModel>(
          "/api/member/getcurrentmember",
          {
            credentials: "include"
          }
        );

        this.member = {
          id: result.id,
          name: result.displayName
        };
      } catch (error) {
        this.member = undefined;
      }
      this.validatedLogin = true;
    },
  },
});
