import { defineEventHandler } from "h3";

export default defineEventHandler(async (event) => {
  deleteCookie(event, "cardgamesdb");
});
