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
  return `${GetBaseApiUrl()}${crops[0].url}?width=${crop?.width}&height=${crop?.height}`;
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
