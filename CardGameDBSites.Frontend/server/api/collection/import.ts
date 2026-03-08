import { defineEventHandler, getCookie, setHeader, readMultipartFormData } from "h3";

export default defineEventHandler(async (event) => {
  const jwt = getCookie(event, "cardgamesdb");
  const query = getQuery(event);
  const overwrite = query.overwrite || "false";

  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/api/collection/import?overwrite=${overwrite}`;

  const formData = await readMultipartFormData(event);

  try {
    const headers: Record<string, string> = {};

    if (jwt) {
      headers.Authorization = `Bearer ${jwt}`;
    }

    const response = await $fetch(backendUrl, {
      method: "POST",
      headers,
      body: formData,
    });

    return response;
  } catch (error: any) {
    console.error("Import proxy error:", error);
    throw error;
  }
});
