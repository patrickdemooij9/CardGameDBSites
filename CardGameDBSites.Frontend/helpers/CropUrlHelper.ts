import type { IApiMediaWithCropsModel } from "~/api/umbraco";
import { GetBaseApiUrl } from "./RequestsHelper";

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

export function GetAbsoluteUrl(url: string)
{
  return `${GetBaseApiUrl()}${url}`;
}
