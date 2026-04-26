import { createError, defineEventHandler, proxyRequest } from "h3";

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig();
  const path = event.context.params?.path ?? "";
  const query = getRequestURL(event).search;
  const backendUrl = `${config.public.API_BASE_URL}/media/${path}${query}`;

  try {
    return await proxyRequest(event, backendUrl);
  } catch (error) {
    console.error(`Failed proxy for /media/${path} -> ${backendUrl}`, error);
    throw createError({
      statusCode: 502,
      statusMessage: "Failed to fetch media from backend"
    });
  }
});
