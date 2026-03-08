import { defineEventHandler, getCookie, setHeader } from "h3";

export default defineEventHandler(async (event) => {
  const jwt = getCookie(event, "cardgamesdb");
  const query = getQuery(event);
  const exportType = query.exportType || "Grouped";

  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/api/collection/export?exportType=${exportType}`;

  try {
    const response = await $fetch(backendUrl, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        ...(jwt ? { Authorization: `Bearer ${jwt}` } : {}),
      },
      responseType: "blob",
    });

    setHeader(event, "Content-Type", "application/octet-stream");
    setHeader(
      event,
      "Content-Disposition",
      'attachment; filename="CollectionExport.xlsx"'
    );

    return response;
  } catch (error) {
    console.error("Export proxy error:", error);
    throw error;
  }
});
