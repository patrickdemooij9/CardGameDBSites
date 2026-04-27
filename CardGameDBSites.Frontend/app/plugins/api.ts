// plugins/api.ts
export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig();

  // preconfigured $fetch instance with base URL
  const api = $fetch.create({
    baseURL: config.public.API_BASE_URL,
  });

  return {
    provide: {
      api,
    },
  };
});