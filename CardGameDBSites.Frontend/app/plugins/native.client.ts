import { App } from "@capacitor/app";
import { isNativeApp } from "~/helpers/NativeApp";
import { LoadNativeTokens } from "~/helpers/NativeAuth";

export default defineNuxtPlugin(async () => {
  if (!isNativeApp()) {
    return;
  }

  await LoadNativeTokens();

  const router = useRouter();
  App.addListener("backButton", ({ canGoBack }) => {
    if (canGoBack && router.currentRoute.value.fullPath !== "/") {
      router.back();
    } else {
      App.exitApp();
    }
  });
});
