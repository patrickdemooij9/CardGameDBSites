import {
  type CreateSquadPostModel,
  type DeckApiModel,
  type DeckQueryPostModel,
  type PagedResultDeckApiModel,
} from "~/api/default";
import type { CreateDeckModel } from "~/components/decks/deckBuilder/models/CreateDeckModel";
import { DoOptionalServerFetch, DoServerFetch } from "~/helpers/RequestsHelper";

export default class DeckService {
  async get(id: number) {
    const result = await DoOptionalServerFetch<DeckApiModel>(
      "/api/decks/get?id=" + id,
      {
        query: {
          id,
        },
      }
    );
    return result!;
  }

  async query(model: DeckQueryPostModel) {
    const result = await DoOptionalServerFetch<PagedResultDeckApiModel>(
      "/api/decks/query",
      {
        method: "POST",
        body: model,
      }
    );
    return result!;
  }

  async post(model: CreateDeckModel, publish: boolean) {
    const modelToPost: CreateSquadPostModel = {
      id: model.id,
      name: model.name,
      description: model.description,
      typeId: model.typeId,
      publish: publish,
      squads: model.groups.map((group, index) => ({
        id: group.id,
        slots: [
          ...group.slots.map((slot) => ({
            id: slot.id,
            cards: slot.cardGroups
              .flatMap((card) => card.cards)
              .map((key) => ({
                cardId: key.card.baseId!,
                amount: key.amount,
                children: key.children.flatMap((childSlot) =>
                  childSlot.cardGroups
                    .flatMap((card) => card.cards)
                    .map((key) => key.card.baseId!)
                ),
              })),
          })),
          // Include sideboard cards in the first group with slotId 99
          ...(index === 0 && model.sideboardSlot && model.hasSideboard ? [{
            id: 99,
            cards: model.sideboardSlot.cardGroups
              .flatMap((cg) => cg.cards)
              .map((key) => ({
                cardId: key.card.baseId!,
                amount: key.amount,
                children: [],
              })),
          }] : []),
        ],
      })),
    };
    const result = DoOptionalServerFetch<number>(
      '/api/deckbuilder/submit',
      {
        method: "POST",
        body: modelToPost,
      }
    );
    return result;
  }

  async likeDeck(deckId: number) {
    const result = DoServerFetch<boolean>(
      "/api/decks/likeDeck",
      true,
      {
        method: "POST",
        body: JSON.stringify(deckId),
      },
    );
    return result!;
  }

  async viewDeck(deckId: number) {
    DoServerFetch<boolean>(
      "/api/decks/viewDeck",
      true,
      {
        method: "POST",
        body: JSON.stringify(deckId),
      },
    );
  }

  async deleteDeck(deckId: number) {
    await DoServerFetch(
      `/api/decks/deleteDeck?deckId=${deckId}`,
      true,
      {
        method: "DELETE",
      }
    );
  }
}
