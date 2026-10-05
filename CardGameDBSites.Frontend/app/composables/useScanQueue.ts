import type { IndexCard } from "#card-scanner/matcher";
import { useCards } from "~/composables/useCards";
import { useCollection } from "~/composables/useCollection";
import {
  addScan,
  collectionAmounts,
  defaultVariantId,
  variantOptions,
  type ScanQueueEntry,
} from "~/services/scanner/ScanQueue";
import { useCardsStore } from "~/stores/CardStore";

export function useScanQueue() {
  const entries = useState<ScanQueueEntry[]>("scanner-queue", () => []);
  const cards = useCards();
  const cardStore = useCardsStore();
  const collection = useCollection();

  async function add(card: IndexCard, amount: number) {
    let variants = cardStore.cards[card.baseId]?.data.variants ?? [];
    try {
      const [details] = await Promise.all([cards.loadCardsByIds([card.baseId]), cards.loadVariantTypes()]);
      variants = details[0]?.variants ?? variants;
    } catch {
      // Offline: fall back to the matched image's own variant, which the user can still change later.
    }

    const variantId = defaultVariantId(card.variantIds, variants);
    if (variantId === undefined) {
      return undefined;
    }
    const scanned = {
      baseId: card.baseId,
      name: card.name,
      set: card.set,
      imageUrl: card.url,
      urlSegment: card.urlSegment,
      imageVariantIds: card.variantIds,
    };
    return addScan(entries.value, scanned, variantId, amount).id;
  }

  function remove(entry: ScanQueueEntry) {
    entries.value = entries.value.filter((it) => it.id !== entry.id);
  }

  function optionsFor(entry: ScanQueueEntry) {
    const variants = cardStore.cards[entry.baseId]?.data.variants ?? [];
    return variantOptions(entry.imageVariantIds, variants, cardStore.variantTypes.data);
  }

  async function importAll() {
    const baseIds = [...new Set(entries.value.map((entry) => entry.baseId))];
    const owned = await collection.loadCards(baseIds, { refresh: true });

    const items = baseIds
      .map((cardId) => ({
        cardId,
        values: collectionAmounts(
          entries.value.filter((entry) => entry.baseId === cardId),
          owned[cardId] ?? []
        ),
      }))
      .filter((item) => Object.keys(item.values).length);
    if (items.length) {
      await collection.saveCardsBatch(items);
    }

    const imported = entries.value.reduce((sum, entry) => sum + entry.amount, 0);
    entries.value = [];
    return imported;
  }

  return { entries, add, remove, optionsFor, importAll };
}
