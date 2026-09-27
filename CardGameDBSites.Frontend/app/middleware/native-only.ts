import { isNativeApp } from "~/helpers/NativeApp";

export default defineNuxtRouteMiddleware(() => {
  if (!isNativeApp()) {
    return navigateTo("/");
  }
});
