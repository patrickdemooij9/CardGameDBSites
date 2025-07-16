import { defineEventHandler, proxyRequest, sendProxy } from "h3";

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/api/account/register`;
  const body = await readBody(event);

  const jwtToken = (await $fetch(backendUrl, {
    method: "POST",
    body,
  })) as string;

  // Set JWT cookie if present
  if (jwtToken) {
    setCookie(event, "cardgamesdb", jwtToken, {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      path: "/",
      sameSite: "lax",
    });

    return await $fetch(
      `${config.public.API_BASE_URL}/api/account/getCurrentMember`,
      {
        headers: jwtToken ? { Authorization: `Bearer ${jwtToken}` } : {},
      }
    );
  }
});
