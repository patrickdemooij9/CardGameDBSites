import type { CurrentMemberApiModel, RegisterPostModel } from "~/api/default";
import { DoFetch } from "~/helpers/RequestsHelper";
import type MemberModel from "~/models/MemberModel";

export const useAccountStore = defineStore("accountStore", {
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
          `${config.public.API_BASE_URL}/api/account/login`,
          {
            method: "POST",
            body: { email, password, rememberMe },
            credentials: "include",
          }
        );
        this.member = {
          id: member.id,
          name: member.displayName,
          likedDecks: member.likedDecks || [],
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
          "/api/account/getcurrentmember",
          {
            credentials: "include",
          }
        );

        this.member = {
          id: result.id,
          name: result.displayName,
          likedDecks: result.likedDecks || [],
        };
      } catch (error) {
        this.member = undefined;
      }
      this.validatedLogin = true;
    },
    async register(model: RegisterPostModel) {
      const result = await DoFetch<CurrentMemberApiModel>(
        "/api/account/register",
        {
          method: "POST",
          body: model,
        }
      );

      this.member = {
        id: result.id,
        name: result.displayName,
        likedDecks: result.likedDecks || [],
      };
    },
    async forgotPassword(email: string) {
      return await DoFetch("/api/account/forgotpassword", {
        method: "POST",
        body: {
          email: email,
        },
      });
    },
    toggleDeckLike(deckId: number) {
      if (this.member?.likedDecks?.includes(deckId)) {
        this.member.likedDecks = this.member.likedDecks.filter(
          (id) => id !== deckId
        );
      } else {
        this.member?.likedDecks?.push(deckId);
      }
    },
  },
});
