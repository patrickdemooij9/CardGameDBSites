import type { NitroFetchOptions } from "nitropack";
import { useAccountStore } from "~/stores/AccountStore";

export function DoFetch<T>(
  url: string,
  options?: NitroFetchOptions<string>
): Promise<T> {
  options = options ?? {};
  options.headers = options.headers ?? {};

  const config = useRuntimeConfig();
  return $fetch<T>(`${config.public.API_BASE_URL}${url}`, options);
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

  if (useProxy){
    url = `/api/proxy${url}`;
  }

  return $fetch<T>(url, options);
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
