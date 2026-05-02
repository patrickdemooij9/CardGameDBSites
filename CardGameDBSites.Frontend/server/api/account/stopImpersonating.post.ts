import { createError, defineEventHandler, deleteCookie, getCookie, setCookie } from "h3";

export default defineEventHandler(async (event) => {
  const adminJwt = getCookie(event, "cardgamesdb_admin");

  if (!adminJwt) {
    throw createError({ statusCode: 400, statusMessage: "Not currently impersonating" });
  }

  const isProduction = process.env.NODE_ENV === "production";

  // Restore the original admin session
  setCookie(event, "cardgamesdb", adminJwt, {
    httpOnly: true,
    secure: isProduction,
    path: "/",
    sameSite: "lax",
  });

  // Remove the preserved admin cookie now that it has been restored
  deleteCookie(event, "cardgamesdb_admin");

  const config = useRuntimeConfig();

  return await $fetch(
    `${config.public.API_BASE_URL}/api/account/getCurrentMember`,
    {
      headers: { Authorization: `Bearer ${adminJwt}` },
    }
  );
});
