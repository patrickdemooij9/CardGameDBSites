import { joinURL } from "ufo";
import { createOperationsGenerator } from "@nuxt/image/runtime";
import { defineProvider } from "@nuxt/image/runtime";

/**
 * Custom @nuxt/image provider for the Umbraco backend.
 *
 * The Umbraco 13 backend runs SixLabors.ImageSharp.Web, which resizes and
 * re-encodes images on demand via query-string params
 * (`?width=&height=&format=webp&quality=&rmode=`). This provider does no image
 * processing itself — it only rewrites the URL to hit ImageSharp, so it works
 * anywhere (including Cloudflare Pages) with zero runtime cost.
 *
 * It handles the two URL shapes the API returns:
 *  - relative Umbraco Delivery media (`/media/...`) — prefixed with `baseURL`.
 *  - pre-built absolute crop URLs from the default API — left as-is, and any
 *    existing query params (crop coordinates, etc.) are preserved.
 */
const operationsGenerator = createOperationsGenerator({
  keyMap: {
    width: "width",
    height: "height",
    format: "format",
    quality: "quality",
    fit: "rmode",
  },
  joinWith: "&",
  formatter: (key: string, value: string) => `${key}=${value}`,
});

export default defineProvider<{ baseURL?: string }>({
  getImage: (src: string, { modifiers, baseURL = "" }) => {
    const operations = operationsGenerator(modifiers ?? {});
    const isAbsolute = /^https?:\/\//.test(src);
    const url = joinURL(isAbsolute ? "" : baseURL, src);
    const separator = url.includes("?") ? "&" : "?";
    return {
      url: operations ? `${url}${separator}${operations}` : url,
    };
  },
});
