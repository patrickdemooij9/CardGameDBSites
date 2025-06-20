import {
  type CreateSquadPostModel,
  type DeckApiModel,
  type DeckQueryPostModel,
  type PagedResultDeckApiModel,
} from "~/api/default";
import type { CreateDeckModel } from "~/components/decks/deckBuilder/models/CreateDeckModel";
import { DoFetch } from "~/helpers/RequestsHelper";

export default class DeckService {
  async get(id: number) {
    const result = DoFetch<DeckApiModel>(
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
    const result = await DoFetch<PagedResultDeckApiModel>(
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
      squads: model.groups.map((group) => ({
        id: group.id,
        slots: group.slots.map((slot) => ({
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
      })),
    };
    const result = DoFetch<number>(
      '/api/deckbuilder/submit',
      {
        method: "POST",
        body: modelToPost,
      }
    );
    return result;
  }
}
