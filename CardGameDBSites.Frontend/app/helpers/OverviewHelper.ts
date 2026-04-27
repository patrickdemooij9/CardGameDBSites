import type {
  ApiBlockItemModel,
  IApiContentModel,
  OverviewFilterElementModel,
  OverviewFilterItemPropertiesModel,
} from "~/api/umbraco";
import { GetAbsoluteUrl } from "./CropUrlHelper";
import {
  OverviewFilterType,
  type OverviewFilterItemModel,
  type OverviewFilterModel,
} from "~/components/overviews/OverviewFilterModel";

export function ToOverviewModel(items: ApiBlockItemModel[]) {
  return (
    items.map<OverviewFilterModel>((item) => {
      const castedItem = (item.content as OverviewFilterElementModel)
        .properties!;
      return {
        Alias: castedItem.isExpansionFilter
          ? "Set Name"
          : ((castedItem.ability as IApiContentModel[])[0]?.name ?? ""),
        DisplayName: castedItem?.displayName ?? "",
        Type: castedItem?.isInline
          ? OverviewFilterType.INLINE
          : OverviewFilterType.DROPDOWN,
        AutoFillValues: castedItem?.autoFillValues ?? false,
        Items:
          castedItem?.items?.items?.map<OverviewFilterItemModel>((child) => {
            const castedChild = child.content
              ?.properties as OverviewFilterItemPropertiesModel;
            return {
              DisplayName: castedChild?.displayName ?? "",
              Value: castedChild?.value ?? "",
              IconUrl: castedChild?.icon
                ? GetAbsoluteUrl(castedChild!.icon![0]!.url!)
                : "",
            };
          }) ?? [],
      };
    }) ?? []
  );
}
