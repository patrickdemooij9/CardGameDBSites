import { defineEventHandler } from "h3";
import type { RegisterPostModel } from "~/api/default";

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig();
  const backendUrl = `${config.public.API_BASE_URL}/api/account/register`;
  const body = await readBody<RegisterPostModel>(event);

  let jwtToken: string;
  try {
    jwtToken = await $fetch<string>(backendUrl, {
      method: "POST",
      body,
    });
  } catch (error: any) {
    // Pass the backend's status and body through so the form can show the real reason.
    setResponseStatus(event, error?.statusCode ?? 500);
    return error?.data;
  }

  setCookie(event, "cardgamesdb", jwtToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === "production",
    path: "/",
    sameSite: "lax",
  });

  return await $fetch(`${config.public.API_BASE_URL}/api/account/getCurrentMember`, {
    headers: { Authorization: `Bearer ${jwtToken}` },
  });
});
