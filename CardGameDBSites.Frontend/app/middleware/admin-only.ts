import { useAccountStore } from "~/stores/AccountStore";

export default defineNuxtRouteMiddleware(async () => {
  const accountStore = useAccountStore();
  await accountStore.checkLogin();

  if (!accountStore.member?.isAdmin) {
    return navigateTo("/");
  }
});
