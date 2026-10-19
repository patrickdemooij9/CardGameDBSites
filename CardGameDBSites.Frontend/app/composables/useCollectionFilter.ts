import {
  OverviewFilterType,
  type OverviewFilterModel,
} from "~/components/overviews/OverviewFilterModel";
import { useAccountStore } from "~/stores/AccountStore";

export function useCollectionFilter(baseFilters: OverviewFilterModel[]) {
  const filters = reactive<OverviewFilterModel[]>([...baseFilters]);

  // Added client-side only: login state isn't known during SSR, so this avoids a hydration mismatch.
  onMounted(async () => {
    if (await useAccountStore().checkLogin()) {
      filters.push({
        Alias: "collection",
        DisplayName: "Collection",
        Type: OverviewFilterType.DROPDOWN,
        Items: [
          { DisplayName: "In collection", Value: "inCollection" },
          { DisplayName: "No copies", Value: "none" },
          { DisplayName: "Missing copies", Value: "missing" },
        ],
        AutoFillValues: false,
      });
    }
  });

  return filters;
}
