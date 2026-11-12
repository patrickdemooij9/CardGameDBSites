<script setup lang="ts">
import { PhTrophy } from "@phosphor-icons/vue";
import type { CardDetailApiModel, DeckTypeSettingsApiModel } from "~/api/default";
import { GetValidCards } from "~/services/requirements/RequirementService";
import TournamentService, { type TournamentWinningDeckApiModel } from "~/services/TournamentService";
import CmsImage from "../shared/CmsImage.vue";
import SectionHeader from "../shared/SectionHeader.vue";

const props = defineProps<{
  content: {
    title?: string | null;
    amount?: number | null;
  };
}>();

const amount = props.content.amount ?? 4;

const { data: winners } = await useAsyncData(
  `winning-decks:${amount}`,
  () => new TournamentService().getWinningDecks(amount).catch(() => []),
  { default: () => [] as TournamentWinningDeckApiModel[] },
);

const { getDeckTypeSettings, getNavigationUrl } = useSite();

const typeIds = [...new Set(winners.value.map((winner) => winner.deck.typeId))];
const settingsByType: Record<number, DeckTypeSettingsApiModel> = {};
for (const typeId of typeIds) {
  const settings = await getDeckTypeSettings(typeId);
  if (settings) settingsByType[typeId] = settings;
}

const cardIds = [...new Set(winners.value.flatMap((winner) => winner.deck.cards.map((card) => card.cardId!)))];
const cardsById: Record<number, CardDetailApiModel> = {};
if (cardIds.length > 0) {
  for (const card of await useCards().loadCardsByIds(cardIds)) {
    cardsById[card.baseId!] = card;
  }
}

const items = computed(() =>
  winners.value
    .filter((winner) => settingsByType[winner.deck.typeId])
    .map((winner) => {
      const settings = settingsByType[winner.deck.typeId]!;
      const deckCards = winner.deck.cards
        .map((card) => cardsById[card.cardId!])
        .filter((card): card is CardDetailApiModel => !!card);
      return {
        ...winner,
        url: settings.overviewUrl + winner.deck.id,
        mainCards: GetValidCards(deckCards, settings.mainCardRequirements ?? []),
      };
    }),
);

const metaUrl = await getNavigationUrl("/meta");
</script>

<template>
  <div v-if="items.length">
    <SectionHeader :title="content.title || 'Tournament winners'" subtitle="latest results" />
    <div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
      <div v-for="item in items" :key="`${item.tournamentId}-${item.deck.id}`" class="flex min-w-0 flex-col">
        <NuxtLink
          :to="item.url"
          class="flex flex-1 flex-col rounded border bg-surface p-3 no-underline hover:border-main-color dark:border-line dark:hover:border-main-color"
        >
          <p class="flex min-h-8 items-start gap-1.5 text-xs font-semibold text-ink-muted">
            <PhTrophy class="shrink-0 text-amber-500" :size="16" weight="fill" />
            <span class="line-clamp-2" :title="item.tournamentName">{{ item.tournamentName }}</span>
          </p>
          <p class="mt-2 line-clamp-2 font-bold">{{ item.deck.name }}</p>
          <div class="mt-auto flex items-end justify-between gap-2 pt-4">
            <div class="flex gap-2">
              <CmsImage
                v-for="card in item.mainCards"
                :key="card.baseId"
                class="h-12 w-12 rounded-full"
                width="96"
                height="96"
                :src="card.imageUrl"
                crop="icon"
                :alt="card.displayName"
              />
            </div>
            <p v-if="item.deck.price" class="w-fit rounded-md bg-green-600 px-2.5 py-1.5 text-white">
              ${{ item.deck.price.marketPrice.toFixed(2) }}
            </p>
          </div>
        </NuxtLink>
      </div>
    </div>
    <div v-if="metaUrl" class="mt-6 flex justify-center">
      <NuxtLink :to="metaUrl" class="btn-secondary no-underline">View the meta</NuxtLink>
    </div>
  </div>
</template>
