import type { NitroFetchOptions } from "nitropack";
import { useAppToast } from "~/composables/useAppToast";
import { useAccountStore } from "~/stores/AccountStore";
import { Capacitor } from "@capacitor/core";

type FetchStatusError = {
  statusCode?: number;
  status?: number;
  response?: {
    status?: number;
  };
};

/**
 * Clears client-side auth state when an API request reports the session as unauthorized.
 */
function handleUnauthorized(error: unknown) {
  if (!import.meta.client) {
    return;
  }

  const fetchError = error as FetchStatusError;
  const status =
    fetchError.response?.status ?? fetchError.statusCode ?? fetchError.status;

  if (status !== 401) {
    return;
  }

  const accountStore = useAccountStore();
  const wasLoggedIn = accountStore.isLoggedIn;
  accountStore.member = undefined;
  accountStore.validatedLogin = false;

  if (wasLoggedIn) {
    useAppToast().info("Your session expired, so you were logged out.");
  }
}

/**
 * Returns true when running inside a Capacitor native shell.
 * In that context there is no Nitro server, so the /api/proxy route is unavailable.
 */
function isNativeApp(): boolean {
  if (!import.meta.client) return false;
  return Capacitor.isNativePlatform();
}

export function DoFetch<T>(
  url: string,
  options?: NitroFetchOptions<string>
): Promise<T> {
  options = options ?? {};
  options.headers = options.headers ?? {};

  const config = useRuntimeConfig();
  return $fetch<T>(`${config.public.API_BASE_URL}${url}`, options).catch((error) => {
    handleUnauthorized(error);
    throw error;
  });
}

export async function DoFetch2<T>(
  api: typeof $fetch,
  url: string,
  options?: NitroFetchOptions<string>
): Promise<T> {
  return api<T>(url, options);
}

export async function DoServerFetch<T>(
  url: string,
  useProxy: boolean = true,
  options?: NitroFetchOptions<string>
): Promise<T> {
  options = options ?? {};
  options.headers = options.headers ?? {};

  // In a Capacitor native app there is no Nitro server, so skip the proxy
  // and call the API directly.
  if (useProxy && isNativeApp()) {
    return DoFetch<T>(url, options);
  }

  if (useProxy){
    url = `/api/proxy${url}`;
  }

  if (import.meta.server){
    const { data, error } = await useFetch(url, options);
    return data.value as T;
  }

  return $fetch<T>(url, options).catch((error) => {
    handleUnauthorized(error);
    throw error;
  });
}

export async function DoOptionalServerFetch<T>(
  url: string,
  options?: NitroFetchOptions<string>
): Promise<T> {
  if (import.meta.client && !(await useAccountStore().checkLogin())){
    return DoFetch<T>(url, options);
  }
  return DoServerFetch<T>(url, true, options);
}

export function GetBaseApiUrl(){
  return useRuntimeConfig().public.API_BASE_URL;
}
