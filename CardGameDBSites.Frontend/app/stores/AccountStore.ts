import type { CurrentMemberApiModel, RegisterPostModel } from "~/api/default";
import { isNativeApp } from "~/helpers/NativeApp";
import {
  GetNativeAdminToken,
  GetNativeToken,
  LoadNativeTokens,
  SetNativeAdminToken,
  SetNativeToken,
} from "~/helpers/NativeAuth";
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
    applyMember(result: CurrentMemberApiModel) {
      this.member = {
        id: result.id,
        name: result.displayName,
        likedDecks: result.likedDecks || [],
        isAdmin: result.isAdmin ?? false,
        impersonatedBy: result.impersonatedBy,
      };
    },
    async login(email: string, password: string, rememberMe: boolean) {
      try {
        if (isNativeApp()) {
          const token = await DoFetch<string>("/api/account/login", {
            method: "POST",
            body: { email, password, rememberMe },
          });
          await SetNativeToken(token);
          this.applyMember(await DoFetch<CurrentMemberApiModel>("/api/account/getCurrentMember"));
        } else {
          this.applyMember(
            await DoServerFetch<CurrentMemberApiModel>(`/api/account/login`, false, {
              method: "POST",
              body: { email, password, rememberMe },
            })
          );
        }
        this.validatedLogin = true;
      } catch (error) {
        throw "Incorrect email/password";
      }
    },
    async logout(){
      if (isNativeApp()) {
        await SetNativeToken(undefined);
        await SetNativeAdminToken(undefined);
      } else {
        await DoServerFetch("/api/account/logout", false, {
          method: "POST",
        });
      }
      this.member = undefined;
    },
    async checkLogin() {
      if (this.validatedLogin) {
        return this.member !== undefined;
      }

      if (_loadingPromise) return _loadingPromise;
      _loadingPromise = (async () => {
        try {
          if (isNativeApp()) {
            await LoadNativeTokens();
          }
          const result = await DoServerFetch<CurrentMemberApiModel>(
            "/api/account/getCurrentMember"
          );

          this.applyMember(result);
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
      if (isNativeApp()) {
        const token = await DoFetch<string>("/api/account/register", {
          method: "POST",
          body: model,
        });
        await SetNativeToken(token);
        this.applyMember(await DoFetch<CurrentMemberApiModel>("/api/account/getCurrentMember"));
        this.validatedLogin = true;
        return;
      }

      this.applyMember(
        await DoFetch<CurrentMemberApiModel>("/api/account/register", {
          method: "POST",
          body: model,
        })
      );
    },
    async forgotPassword(email: string) {
      return await DoServerFetch("/api/account/forgotpassword", true, {
        method: "POST",
        body: {
          email: email,
        },
      });
    },
    async resetPassword(code: string, newPassword: string) {
      return await DoServerFetch("/api/account/resetpassword", true, {
        method: "POST",
        body: {
          code,
          newPassword,
        },
      });
    },
    async impersonate(memberId: number) {
      if (isNativeApp()) {
        const impersonationToken = await DoFetch<string>(
          `/api/account/impersonate/${memberId}`,
          { method: "POST" }
        );
        await SetNativeAdminToken(GetNativeToken());
        await SetNativeToken(impersonationToken);
        this.applyMember(await DoFetch<CurrentMemberApiModel>("/api/account/getCurrentMember"));
        this.validatedLogin = true;
        return;
      }

      this.applyMember(
        await DoServerFetch<CurrentMemberApiModel>("/api/account/impersonate", false, {
          method: "POST",
          body: { memberId },
        })
      );
      this.validatedLogin = true;
    },
    async stopImpersonating() {
      if (isNativeApp()) {
        const adminToken = GetNativeAdminToken();
        if (!adminToken) {
          throw new Error("Not currently impersonating");
        }
        await SetNativeToken(adminToken);
        await SetNativeAdminToken(undefined);
        this.applyMember(await DoFetch<CurrentMemberApiModel>("/api/account/getCurrentMember"));
        this.validatedLogin = true;
        return;
      }

      const result = await DoServerFetch<CurrentMemberApiModel>(
        "/api/account/stopImpersonating",
        false,
        {
          method: "POST",
        }
      );
      this.applyMember(result);
      this.member!.impersonatedBy = undefined;
      this.validatedLogin = true;
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
