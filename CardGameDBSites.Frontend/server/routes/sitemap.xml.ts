import { defineEventHandler, proxyRequest } from "h3";

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/sitemap.xml`;

  return await proxyRequest(event, backendUrl);
});
