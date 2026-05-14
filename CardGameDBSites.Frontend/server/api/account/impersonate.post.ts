import { createError, defineEventHandler, getCookie, setCookie } from "h3";

export default defineEventHandler(async (event) => {
  const adminJwt = getCookie(event, "cardgamesdb");

  if (!adminJwt) {
    throw createError({ statusCode: 401, statusMessage: "Not authenticated" });
  }

  const body = await readBody<{ memberId: number }>(event);
  if (!body?.memberId) {
    throw createError({ statusCode: 400, statusMessage: "memberId is required" });
  }

  const config = useRuntimeConfig();

  const impersonationJwt = await $fetch<string>(
    `${config.public.API_BASE_URL}/api/account/impersonate/${body.memberId}`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${adminJwt}` },
    }
  );

  const isProduction = process.env.NODE_ENV === "production";

  // Preserve the original admin JWT so impersonation can be reversed
  setCookie(event, "cardgamesdb_admin", adminJwt, {
    httpOnly: true,
    secure: isProduction,
    path: "/",
    sameSite: "lax",
  });

  // Replace the main session cookie with the short-lived impersonation token
  setCookie(event, "cardgamesdb", impersonationJwt, {
    httpOnly: true,
    secure: isProduction,
    path: "/",
    sameSite: "lax",
    maxAge: 3600,
  });

  return await $fetch(
    `${config.public.API_BASE_URL}/api/account/getCurrentMember`,
    {
      headers: { Authorization: `Bearer ${impersonationJwt}` },
    }
  );
});
