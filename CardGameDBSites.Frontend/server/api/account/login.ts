import { defineEventHandler, proxyRequest, sendProxy } from "h3";
import type { LoginPostModel } from "~/api/default";

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/api/account/login`;
  const body = await readBody<LoginPostModel>(event);

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
      maxAge: body.rememberMe ? 2.592e+6 : undefined,
    });

    return await $fetch(
      `${config.public.API_BASE_URL}/api/account/getCurrentMember`,
      {
        headers: jwtToken ? { Authorization: `Bearer ${jwtToken}` } : {},
      }
    );
  }
});
