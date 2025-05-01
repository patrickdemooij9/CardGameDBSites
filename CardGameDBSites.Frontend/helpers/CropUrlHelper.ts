import type { IApiMediaWithCropsModel } from "~/api/umbraco";

export function GetCropUrl(
  crops: Array<IApiMediaWithCropsModel>,
  cropAlias: string
) {
  if (crops.length === 0){
    return '#';
  }
  const crop = crops.flatMap((item) => item.crops).find((crop) => crop?.alias === cropAlias);
  return `https://localhost:44344${crops[0].url}?width=${crop?.width}&height=${crop?.height}`;
}

export function GetAbsoluteUrl(url: string)
{
  return `https://localhost:44344${url}`;
}
