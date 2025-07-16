import type { NitroFetchOptions } from "nitropack";

export function DoFetch<T>(
  url: string,
  options?: NitroFetchOptions<string>
): Promise<T> {
  options = options ?? {};
  options.headers = options.headers ?? {};

  const config = useRuntimeConfig();
  return $fetch<T>(`${config.public.API_BASE_URL}${url}`, options);
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

  if (import.meta.server){
    const { data, error } = await useFetch(url, options);
    return data.value as T;
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
