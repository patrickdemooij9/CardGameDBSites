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
