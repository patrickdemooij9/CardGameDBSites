import type { CurrentMemberApiModel, RegisterPostModel } from "~/api/default";
import { DoFetch, DoServerFetch } from "~/helpers/RequestsHelper";
import type MemberModel from "~/models/MemberModel";

let _loadingPromise: Promise<boolean> | null = null;

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
      try {
        const member = await DoServerFetch<CurrentMemberApiModel>(
          `/api/account/login`,
          false,
          {
            method: "POST",
            body: { email, password, rememberMe },
          }
        );
        this.member = {
          id: member.id,
          name: member.displayName,
          likedDecks: member.likedDecks || [],
        };
        this.validatedLogin = true;
      } catch (error) {
        throw "Incorrect email/password";
      }
    },
    async logout(){
      await DoServerFetch("/api/account/logout", false, {
        method: "POST",
      });
      this.member = undefined;
    },
    async checkLogin() {
      if (this.validatedLogin) {
        return this.member !== undefined;
      }

      if (_loadingPromise) return _loadingPromise;
      _loadingPromise = (async () => {
        try {
          const result = await DoServerFetch<CurrentMemberApiModel>(
            "/api/account/getCurrentMember"
          );

          this.member = {
            id: result.id,
            name: result.displayName,
            likedDecks: result.likedDecks || [],
          };
        } catch (error) {
          this.member = undefined;
          return false;
        }
        this.validatedLogin = true;
        return true;
      })();
      return _loadingPromise;
    },
    async getCurrentMember(){
      if (this.member) {
        return this.member;
      }
      if (_loadingPromise) return _loadingPromise.then(() => this.member);
      return undefined;
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
      return await DoServerFetch("/api/account/forgotpassword", true, {
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
