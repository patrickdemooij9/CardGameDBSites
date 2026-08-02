import { RestrictionType, type RequirementApiModel } from "~/api/default";
import type { ApiBlockListModel } from "~/api/umbraco";
import RequirementType from "~/services/requirements/RequirementType";

// Maps a squad-requirement block's element-type alias (as delivered by the Delivery API) to the
// requirement alias understood by the requirement handlers (RequirementService). Only the
// attribute-based requirements are mapped; other requirement types don't translate to a card filter.
const contentTypeToAlias: Record<string, RequirementType> = {
  equalAbilitySquadRequirementConfig: RequirementType.EqualValue,
  notEqualAbilitySquadRequirementConfig: RequirementType.NotEqualValue,
};

function parseRestrictionType(value: string | null | undefined): RestrictionType {
  switch (value) {
    case "Filter":
      return RestrictionType.FILTER;
    case "Passive":
      return RestrictionType.PASSIVE;
    default:
      return RestrictionType.HARD;
  }
}

/**
 * Converts a "Squad Requirements" Block List (from a page's Delivery API properties, e.g.
 * MetaCardOverview.cardRequirement) into the RequirementApiModel[] shape consumed by
 * RequirementService (GetFilters / GetValidCards). Blocks whose type isn't a mappable
 * attribute-based requirement are skipped.
 */
export function ToRequirements(blockList: ApiBlockListModel | undefined | null): RequirementApiModel[] {
  const result: RequirementApiModel[] = [];
  for (const item of blockList?.items ?? []) {
    const content = item.content;
    const alias = contentTypeToAlias[content?.contentType ?? ""];
    if (!alias) continue;

    const properties = content?.properties as
      | { ability?: Array<{ name?: string }> | null; values?: string[] | null; restrictionType?: string | null }
      | undefined;

    const ability = properties?.ability?.[0]?.name;
    if (!ability) continue;

    result.push({
      alias,
      restrictionType: parseRestrictionType(properties?.restrictionType),
      config: {
        ability,
        values: properties?.values ?? [],
      },
    });
  }
  return result;
}
