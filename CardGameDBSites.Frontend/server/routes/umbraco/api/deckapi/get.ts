export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig();
  const path = event.context.matchedRoute?.path ?? "";
  const query = getRequestURL(event).search;
  const backendUrl = `${config.public.API_BASE_URL}${path}${query}`;

  try {
    return await proxyRequest(event, backendUrl);
  } catch (error) {
    console.error(`Failed proxy for ${path} -> ${backendUrl}`, error);
    throw createError({
      statusCode: 502,
      statusMessage: "Failed to fetch from backend",
    });
  }
});
