import { defineEventHandler, getCookie, getQuery, createError } from "h3";

export default defineEventHandler(async (event) => {
  const jwt = getCookie(event, "cardgamesdb");
  const { deckId, exportId } = getQuery(event);

  if (!deckId || !exportId) {
    throw createError({ statusCode: 400, statusMessage: "Missing deckId or exportId" });
  }

  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/api/export/createToken?deckId=${deckId}&exportId=${exportId}`;

  const data = await $fetch<{ token: string }>(backendUrl, {
    headers: jwt ? { Authorization: `****** } : {},
  });

  const exportUrl = `${config.public.API_BASE_URL}/api/export/export?exportToken=${data.token}`;
  return { url: exportUrl };
});
