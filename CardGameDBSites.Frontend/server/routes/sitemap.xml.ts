import { createError, defineEventHandler, proxyRequest } from "h3";

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/sitemap.xml`;

  try {
    return await proxyRequest(event, backendUrl);
  } catch (error) {
    console.error(`Failed proxy for /sitemap.xml -> ${backendUrl}`, error);
    throw createError({
      statusCode: 502,
      statusMessage: "Failed to fetch sitemap.xml from backend"
    });
  }
});
