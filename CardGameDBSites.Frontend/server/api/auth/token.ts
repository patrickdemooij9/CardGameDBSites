import { defineEventHandler, getCookie } from "h3";

export default defineEventHandler((event) => {
  const jwt = getCookie(event, "cardgamesdb");
  return { token: jwt ?? null };
});
