import type {
  ApiBlockItemModel,
  IApiContentModel,
  OverviewFilterElementModel,
  OverviewFilterItemPropertiesModel,
} from "~/api/umbraco";
import { GetAbsoluteUrl } from "./CropUrlHelper";
import { ToRangeFilterClauses } from "./RangeFilterHelper";
import {
  OverviewFilterType,
  type OverviewFilterItemModel,
  type OverviewFilterModel,
} from "~/components/overviews/OverviewFilterModel";

export function GetFilterType(isInline?: boolean | null, isRange?: boolean | null) {
  if (isRange) return OverviewFilterType.RANGE;
  return isInline ? OverviewFilterType.INLINE : OverviewFilterType.DROPDOWN;
}

export function ToOverviewModel(items: ApiBlockItemModel[]) {
  return (
    items.map<OverviewFilterModel>((item) => {
      const castedItem = (item.content as OverviewFilterElementModel)
        .properties!;
      const alias = castedItem.isExpansionFilter
        ? "Set Name"
        : ((castedItem.ability as IApiContentModel[])[0]?.name ?? "");
      return {
        Alias: alias,
        DisplayName: castedItem?.displayName ?? "",
        Type: GetFilterType(castedItem.isInline, castedItem.isRange),
        AutoFillValues: (castedItem?.autoFillValues || castedItem?.isRange) ?? false,
        ToFiltersHandler: castedItem.isRange
          ? (values) => ToRangeFilterClauses(alias, values)
          : undefined,
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
