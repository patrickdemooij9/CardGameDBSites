import type { IApiMediaWithCropsModel } from "~/api/umbraco";
import { GetBaseApiUrl } from "./RequestsHelper";
import type { ImageCropsApiModel } from "~/api/default";

export function GetCropUrl(
  crops: Array<IApiMediaWithCropsModel>,
  cropAlias: string
) {
  if (crops.length === 0){
    return '#';
  }
  const crop = crops.flatMap((item) => item.crops).find((crop) => crop?.alias === cropAlias);
  return `${GetBaseApiUrl()}${crops[0]!.url}?width=${crop?.width}&height=${crop?.height}`;
}

export function GetCrop(image: ImageCropsApiModel | undefined, cropAlias: string | undefined) {
  if (!image) {
    return undefined;
  }
  const crops = image.crops!;
  if (crops.length === 0 || !cropAlias) {
    return image.url;
  }
  return crops.find((crop) => cropAlias === crop.cropAlias)?.url || image.url;
}

export function GetAbsoluteUrl(url: string)
{
  return `${GetBaseApiUrl()}${url}`;
}

/**
 * Appends ImageSharp WebP (and optional sizing) query params to any image URL.
 * Use for cases that can't render through <CmsImg>/<NuxtImg>, such as CSS
 * background-image styles and HTML-string-injected <img> tags. Existing query
 * params are preserved.
 */
export function GetWebpUrl(
  url: string | undefined,
  opts?: { width?: number; height?: number }
) {
  if (!url) {
    return url;
  }
  const separator = url.includes("?") ? "&" : "?";
  const params = [
    "format=webp",
    opts?.width ? `width=${opts.width}` : undefined,
    opts?.height ? `height=${opts.height}` : undefined,
  ]
    .filter(Boolean)
    .join("&");
  return `${url}${separator}${params}`;
}
