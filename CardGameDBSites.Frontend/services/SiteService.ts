import type NavigationModel from "~/components/navigation/NavigationModel";
import type { DeckBuilderApiModel, DeckBuilderSlotAmountApiModel, DeckTypeSettingsApiModel, SiteSettingsApiModel } from "../api/default";
import type NavigationItem from "~/components/navigation/NavigationItemModel";
import CreateDeckGroup from "~/components/decks/deckBuilder/models/CreateDeckGroup";
import CreateDeckSlot from "~/components/decks/deckBuilder/models/CreateDeckSlot";
import { CreateDeckModel } from "~/components/decks/deckBuilder/models/CreateDeckModel";
import CreateDeckCardGroup from "~/components/decks/deckBuilder/models/CreateDeckCardGroup";
import { DynamicDeckAmountConfig, FixedDeckAmountConfig } from "~/components/decks/deckBuilder/models/CreateDeckSlotAmount";
import { DoFetch } from "~/helpers/RequestsHelper";
import DeckService from "./DeckService";
import CardService from "./CardService";

export default class SiteService {
  static settings?: SiteSettingsApiModel;

  async getSettings() {
    if (SiteService.settings) {
      return SiteService.settings;
    }

    const result = await DoFetch<SiteSettingsApiModel>("/api/settings/site");
    SiteService.settings = result;
    return SiteService.settings;
  }

  async getNavigation(): Promise<NavigationModel> {
    const settings = await this.getSettings();
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
    };
  }

  async getDeckTypeSettings(typeId: number)
  {
    const result = await DoFetch<DeckTypeSettingsApiModel>("/api/settings/deckType", {
      query: {
        typeId
      }
    });
    return result;
  }

  async getDeckBuilderSettings(typeId: number, deckId?: number): Promise<CreateDeckModel>{
    const result = await DoFetch<DeckBuilderApiModel>("/api/settings/deckBuilder", {
      query: {
        typeId
      }
    });

    const model = new CreateDeckModel();
    model.typeId = typeId;
    model.groups = result.groups?.map<CreateDeckGroup>((group) => {
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
          deckSlot.maxCardAmount = this.getDeckAmount(slot.maxCardAmount);
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
        if (deck) { //TODO: Also check if user is allowed to edit this deck
          const cardStore = useCardsStore();
          const cards = await cardStore.loadCards([... deck.cards!.map((deckCard) => deckCard.cardId!)]);
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
  }

  private getDeckAmount(model: DeckBuilderSlotAmountApiModel){
    if (model.type === "fixed") {
      return new FixedDeckAmountConfig(model.config!["amount"]);
    }
    if (model.type === "dynamic") {
      return new DynamicDeckAmountConfig(model.config!["requirements"]);
    }
    return new FixedDeckAmountConfig(0);
  }
}
