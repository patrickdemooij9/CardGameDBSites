import { defineEventHandler, getCookie, proxyRequest, sendProxy } from "h3";
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

  try {
    // Use $fetch to call backend API and get raw response
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    };

    if (jwt) {
      headers.Authorization = `Bearer ${jwt}`;
    }
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
    const body =
      method !== "GET" && method !== "HEAD" ? await readBody(event) : undefined;

    const response = await $fetch(backendUrl, {
      method: method,
      headers: {
        ...headers,
        "Content-Type": "application/json",
      },
      body: body,
    });

    // Respond with backend data
    return response;
  } catch (error) {
    console.error("Proxy fetch error:", error);
    throw error;
  }
});
