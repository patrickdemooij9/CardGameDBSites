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
import type { DisplaySize } from "~/components/decks/deckBuilder/DeckBuilderModels";
import { useSiteStore } from "~/stores/SiteStore";

export function useSite() {
  const store = useSiteStore();

  const getSettings = async () => {
    if (!store.isExpired(store.siteSettings)) {
      return store.siteSettings!.data;
    }

    const { data } = await useAsyncData("site-settings", () =>
      DoFetch<SiteSettingsApiModel>("/api/settings/site")
    );
    if (data.value) {
      store.setSiteSettings(data.value);
    }
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
    const cached = store.deckTypeSettings[typeId] ?? null;
    if (!store.isExpired(cached)) {
      return cached!.data;
    }

    const { data } = await useAsyncData(`deck-type-settings-${typeId}`, () =>
      DoFetch<DeckTypeSettingsApiModel>("/api/settings/deckType", {
        query: { typeId }
      })
    );
    if (data.value) {
      store.setDeckTypeSettings(typeId, data.value);
    }
    return data.value!;
  };

  const getSetOverviewSettings = async () => {
    if (!store.isExpired(store.setOverviewSettings)) {
      return store.setOverviewSettings!.data;
    }

    const { data } = await useAsyncData("set-overview-settings", () =>
      DoFetch<SetOverviewSettingsApiModel>("/api/settings/setOverview")
    );
    if (data.value) {
      store.setSetOverviewSettings(data.value);
    }
    return data.value!;
  };

  const getDeckBuilderSettings = async (typeId: string, deckId?: number): Promise<CreateDeckModel> => {
    const { data } = await useAsyncData(`deck-builder-settings-${typeId}`, () =>
      DoFetch<DeckBuilderApiModel>("/api/settings/deckBuilderByGuid", {
        query: { typeGuid: typeId }
      })
    );

    const result = data.value;
    const model = new CreateDeckModel();
    model.typeId = result?.id;
    model.pickDefaultName(result?.defaultNames);
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
        deckSlot.displaySize = slot.displaySize! as DisplaySize;
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
        // Collect all card IDs: parent cards and their children
        const allCardIds = [
          ...deck.cards!.map((deckCard) => deckCard.cardId!),
          ...deck.cards!.flatMap((deckCard) => deckCard.children ?? []),
        ];
        const cards = await useCards().loadCardsByIds([...new Set(allCardIds)]);
        model.id = deckId;
        model.name = deck.name;
        model.description = deck.description ?? "";
        deck.cards?.forEach((deckCard) => {
          const card = cards.find((c) => c.baseId === deckCard.cardId);
          if (!card) {
            return;
          }

          const group = model.groups.find((group) => group.id === deckCard.groupId);
          const slot = group?.slots.find((slot) => slot.id === deckCard.slotId);

          for (var i = 0; i < deckCard.amount!; i++){
            slot?.addCard(card);
          }

          // Populate child slots with saved child cards
          const childIds = deckCard.children ?? [];
          if (childIds.length > 0 && slot) {
            const deckCardEntry = slot.cardGroups
              .flatMap((cg) => cg.cards)
              .find((c) => c.card.baseId === card.baseId);
            if (deckCardEntry && deckCardEntry.children.length > 0) {
              const childSlot = deckCardEntry.children[0];
              if (childSlot) {
                childIds.forEach((childId) => {
                  const childCard = cards.find((c) => c.baseId === childId);
                  if (childCard) {
                    childSlot.addCard(childCard);
                  }
                });
              }
            }
          }
        });
      }
    }

    // Hardcoded sideboard settings (eventually from backend)
    /*model.hasSideboard = true;
    model.sideboardMaxCards = 15;
    const sideboardSlot = new CreateDeckSlot(99, "Sideboard");
    sideboardSlot.maxCardAmount = new FixedDeckAmountConfig(
      model.sideboardMaxCards,
    );
    sideboardSlot.numberMode = true;
    sideboardSlot.disableRemoval = false;
    const sideboardCardGroup = new CreateDeckCardGroup("");
    sideboardSlot.cardGroups.push(sideboardCardGroup);
    model.sideboardSlot = sideboardSlot;

    const sideboardGroup = new CreateDeckGroup();
    sideboardGroup.name = "Sideboard";
    sideboardGroup.slots = [sideboardSlot];
    model.sideboardGroup = sideboardGroup;*/

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
