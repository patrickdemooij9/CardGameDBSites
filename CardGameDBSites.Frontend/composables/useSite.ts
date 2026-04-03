import type NavigationModel from "~/components/navigation/NavigationModel";
import { type SetOverviewSettingsApiModel, type DeckBuilderApiModel, type DeckBuilderSlotAmountApiModel, type DeckTypeSettingsApiModel, type SiteSettingsApiModel } from "~/api/default";
import type NavigationItem from "~/components/navigation/NavigationItemModel";
import CreateDeckGroup from "~/components/decks/deckBuilder/models/CreateDeckGroup";
import CreateDeckSlot from "~/components/decks/deckBuilder/models/CreateDeckSlot";
import { CreateDeckModel } from "~/components/decks/deckBuilder/models/CreateDeckModel";
import CreateDeckCardGroup from "~/components/decks/deckBuilder/models/CreateDeckCardGroup";
import { DynamicDeckAmountConfig, FixedDeckAmountConfig } from "~/components/decks/deckBuilder/models/CreateDeckSlotAmount";
import { DoFetch } from "~/helpers/RequestsHelper";
import DeckService from "~/services/DeckService";
import { useCards } from "~/composables/useCards";

export function useSite() {
  const getSettings = async () => {
    const { data } = await useAsyncData("site-settings", () =>
      DoFetch<SiteSettingsApiModel>("/api/settings/site")
    );
    return data.value!;
  };

  const getNavigation = async () => {
    const settings = await getSettings();
    return {
      textColorIsWhite: settings.textColorWhite,
      createDeckMode: false,
      homepageMode: false,
      navigationLogoUrl: settings.navigationLogoUrl,
      loginPageUrl: settings.loginPageUrl!,
      accountItems: settings.accountNavigation?.map((item) => ({
        name: item.name,
        url: item.url,
        children: []
      })) ?? [],
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
    } as NavigationModel;
  };

  const getDeckTypeSettings = async (typeId: number) => {
    const { data } = await useAsyncData(`deck-type-settings-${typeId}`, () =>
      DoFetch<DeckTypeSettingsApiModel>("/api/settings/deckType", {
        query: { typeId }
      })
    );
    return data.value!;
  };

  const getSetOverviewSettings = async () => {
    const { data } = await useAsyncData("set-overview-settings", () =>
      DoFetch<SetOverviewSettingsApiModel>("/api/settings/setOverview")
    );
    return data.value!;
  };

  const getDeckBuilderSettings = async (typeId: number, deckId?: number): Promise<CreateDeckModel> => {
    const { data } = await useAsyncData(`deck-builder-settings-${typeId}`, () =>
      DoFetch<DeckBuilderApiModel>("/api/settings/deckBuilder", {
        query: { typeId }
      })
    );

    const result = data.value;
    const model = new CreateDeckModel();
    model.typeId = typeId;
    model.groups = result?.groups?.map<CreateDeckGroup>((group) => {
      const deckGroup = new CreateDeckGroup();
      deckGroup.id = group.id;
      deckGroup.name = group.name!;
      deckGroup.requirements = group.requirements!;
      deckGroup.slots = group.slots?.map((slot) => {
        const deckSlot = new CreateDeckSlot(slot.id, slot.name);
        deckSlot.cardGroups = slot.cardGroups?.map((cardGroup) => {
          const deckCardGroup = new CreateDeckCardGroup(cardGroup.displayName);
          deckCardGroup.sortBy = cardGroup.sortBy!;
          deckCardGroup.cards = [];
          deckCardGroup.requirements = cardGroup.requirements ?? [];
          return deckCardGroup;
        }) ?? [];
        deckSlot.minCards = slot.minCards ?? 0;
        deckSlot.maxCardAmount = getDeckAmount(slot.maxCardAmount);
        deckSlot.disableRemoval = slot.disableRemoval!;
        deckSlot.numberMode = slot.numberMode!;
        deckSlot.showIfTargetSlotIsFilled = slot.showIfTargetSlotIsFilled!;
        deckSlot.requirements = slot.requirements ?? [];
        return deckSlot;
      }) ?? [];
      return deckGroup;
    }) ?? [];

    if (deckId) {
      const deck = await new DeckService().get(deckId);
      if (deck) {
        const cards = await useCards().loadCardsByIds([... deck.cards!.map((deckCard) => deckCard.cardId!)]);
        model.id = deckId;
        model.name = deck.name;
        model.description = deck.description ?? "";
        deck.cards?.forEach(async (deckCard) => {
          const card = cards.find((cardId) => cardId.baseId === deckCard.cardId);
          if (!card) {
            return;
          }

          const group = model.groups.find((group) => group.id === deckCard.groupId);
          const slot = group?.slots.find((slot) => slot.id === deckCard.slotId);

          for (var i = 0; i < deckCard.amount!; i++){
            slot?.addCard(card);
          }
        });
      }
    }
    return model;
  };

  const getDeckAmount = (model: DeckBuilderSlotAmountApiModel) => {
    if (model.type === "fixed") {
      return new FixedDeckAmountConfig(model.config!["amount"]);
    }
    if (model.type === "dynamic") {
      return new DynamicDeckAmountConfig(model.config!["requirements"]);
    }
    return new FixedDeckAmountConfig(0);
  };

  return {
    getSettings,
    getNavigation,
    getDeckTypeSettings,
    getSetOverviewSettings,
    getDeckBuilderSettings,
  };
}

export default useSite;