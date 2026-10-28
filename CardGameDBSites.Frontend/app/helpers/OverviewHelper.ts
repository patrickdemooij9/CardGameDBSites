import type {
  ApiBlockItemModel,
  IApiContentModel,
  OverviewFilterElementModel,
  OverviewFilterItemPropertiesModel,
  OverviewFilterPropertiesModel,
} from "~/api/umbraco";
import { GetAbsoluteUrl } from "./CropUrlHelper";
import { ToRangeFilterClauses } from "./RangeFilterHelper";
import { FormatKeywordValue, ToKeywordFilterClauses } from "./KeywordFilterHelper";
import {
  OverviewFilterType,
  type OverviewFilterItemModel,
  type OverviewFilterModel,
} from "~/components/overviews/OverviewFilterModel";
import { useSiteStore } from "~/stores/SiteStore";

const KEYWORD_FILTER_ALIAS = "keywords";

export function GetFilterType(isInline?: boolean | null, isRange?: boolean | null) {
  if (isRange) return OverviewFilterType.RANGE;
  return isInline ? OverviewFilterType.INLINE : OverviewFilterType.DROPDOWN;
}

export function GetKeywordAttributes() {
  return useSiteStore().siteSettings?.data.keywordAttributes ?? [];
}

export function ToOverviewModel(items: ApiBlockItemModel[]) {
  return (
    items.map<OverviewFilterModel>((item) => {
      const castedItem = (item.content as OverviewFilterElementModel)
        .properties!;
      const filterItems = ToFilterItems(castedItem);

      if (castedItem.isKeywordFilter) {
        return {
          Alias: KEYWORD_FILTER_ALIAS,
          DisplayName: castedItem.displayName ?? "",
          Type: OverviewFilterType.KEYWORD,
          AutoFillValues: castedItem.autoFillValues ?? false,
          Items: filterItems,
          ToFiltersHandler: (values, matchMode) =>
            ToKeywordFilterClauses(values, matchMode, GetKeywordAttributes()),
          FormatValue: (value) => FormatKeywordValue(value, filterItems),
        };
      }

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
        Items: filterItems,
      };
    }) ?? []
  );
}

function ToFilterItems(filter: OverviewFilterPropertiesModel) {
  return (
    filter.items?.items?.map<OverviewFilterItemModel>((child) => {
      const castedChild = child.content
        ?.properties as OverviewFilterItemPropertiesModel;
      return {
        DisplayName: castedChild?.displayName ?? "",
        Value: castedChild?.value ?? "",
        IconUrl: castedChild?.icon
          ? GetAbsoluteUrl(castedChild!.icon![0]!.url!)
          : "",
      };
    }) ?? []
  );
}
