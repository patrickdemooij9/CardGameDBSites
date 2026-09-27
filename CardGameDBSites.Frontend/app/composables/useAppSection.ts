import type NavigationItem from "~/components/navigation/NavigationItemModel";
import { useSite } from "~/composables/useSite";

/** Url prefixes that already have their own bottom tab, so "More" can skip them. */
export const AppTabPrefixes = ["/cards", "/decks", "/collection"];

/**
 * Builds an app section from the CMS navigation: the matching top-level item's
 * children become the segmented-control tabs, so each site configures its own.
 */
export async function useAppSection(urlPrefix: string, title: string) {
  const navigation = await useSite().getNavigation();
  const item = navigation.items.find((navItem) => navItem.url.startsWith(urlPrefix));

  const segments: NavigationItem[] = item
    ? item.children.length > 0
      ? item.children
      : [item]
    : [];

  return { segments, title };
}
