import type NavigationModel from "~/components/navigation/NavigationModel";
import type { DeckTypeSettingsApiModel, SiteSettingsApiModel } from "../api/default";
import type NavigationItem from "~/components/navigation/NavigationItemModel";

export default class SiteService {
  static settings?: SiteSettingsApiModel;

  async getSettings() {
    if (SiteService.settings) {
      return SiteService.settings;
    }

    const result = await fetch("https://localhost:44344/api/settings/site");
    if (!result.ok) {
      throw new Error(result.statusText);
    }

    const data = (await result.json()) as SiteSettingsApiModel;
    SiteService.settings = data;
    return SiteService.settings;
  }

  async getNavigation(): Promise<NavigationModel> {
    const settings = await this.getSettings();
    return {
      textColorIsWhite: settings.textColorWhite,
      createDeckMode: false,
      navigationLogoUrl: settings.navigationLogoUrl,
      items:
        settings.navigation?.map<NavigationItem>((item) => ({
          name: item.name,
          url: item.url,
          children:
            item.children?.map<NavigationItem>((child) => ({
              name: child.name,
              url: child.url,
              children: [],
            })) ?? [],
        })) ?? [],
    };
  }

  async getDeckTypeSettings(typeId: number)
  {
    const result = await useFetch<DeckTypeSettingsApiModel>("https://localhost:44344/api/settings/deckType", {
      query: {
        typeId
      }
    });
    return result.data.value;
  }
}
