import { defineEventHandler, getCookie, deleteCookie, setResponseStatus, setResponseHeader } from "h3";
import { URLSearchParams } from "url";

//TODO: Clean this shit up
export default defineEventHandler(async (event) => {
  // Read JWT from cookies
  const jwt = getCookie(event, "cardgamesdb"); // 'token' is your cookie name

  // Get the slug array and join to form the path
  const slug = event.context.params?.slug;
  const path = Array.isArray(slug) ? slug.join("/") : slug || "";

  // Prepare the backend API URL
  const config = useRuntimeConfig();
  const query = getQuery(event);
  const searchParams = new URLSearchParams();

  for (const key in query) {
    const value = query[key];

    if (Array.isArray(value)) {
      value.forEach((v) => searchParams.append(key, String(v)));
    } else if (value !== undefined && value !== null) {
      searchParams.append(key, String(value));
    }
  }

  const queryString = searchParams.toString();
  const backendUrl = `${config.public.API_BASE_URL}/${path}${
    queryString ? `?${queryString}` : ""
  }`;

  const contentType = getRequestHeader(event, "Content-Type") || "application/json";

  const method = event.req.method as
    | "GET"
    | "HEAD"
    | "PATCH"
    | "POST"
    | "PUT"
    | "DELETE"
    | "CONNECT"
    | "OPTIONS"
    | "TRACE"
    | undefined;

  try {
    const headers: Record<string, string> = {
      "Content-Type": contentType,
    };

    if (jwt) {
      headers.Authorization = `Bearer ${jwt}`;
    }

    const body =
      method !== "GET" && method !== "HEAD" ? await readBody(event) : undefined;

    const response = await $fetch.raw(backendUrl, {
      method,
      headers,
      body,
      ignoreResponseError: true,
    });

    if (response.status === 401 && jwt) {
      deleteCookie(event, "cardgamesdb");
    }

    setResponseStatus(event, response.status);
    const excludedHeaders = new Set([
      "set-cookie",
      "content-encoding",
      "content-length",
      "transfer-encoding",
    ]);
    response.headers.forEach((value, key) => {
      if (!excludedHeaders.has(key.toLowerCase())) {
        setResponseHeader(event, key, value);
      }
    });
    return response._data;
  } catch (error) {
    console.error("Proxy fetch error:", error);
    throw error;
  }
});
