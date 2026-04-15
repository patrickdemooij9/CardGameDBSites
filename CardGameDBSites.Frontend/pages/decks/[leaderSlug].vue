<script setup lang="ts">
import { useCards } from '~/composables/useCards';
import { useSite } from '~/composables/useSite';
import { GetCrop } from '~/helpers/CropUrlHelper';
import DeckOverview from '~/components/overviews/DeckOverview.vue';

const route = useRoute();
const leaderSlug = route.params.leaderSlug as string;

const card = await useCards().loadCardByUrlSegment(leaderSlug);

if (!card) {
  throw createError({
    statusCode: 404,
    statusMessage: 'Card Not Found',
  });
}

const siteSettings = await useSite().getSettings();
const config = useRuntimeConfig();

const siteName = siteSettings.siteName;
const pageTitle = `${card.displayName} Decks | ${siteName}`;
const metaDescription = `Browse the best ${card.displayName} decks for ${siteName}. Find top-rated and popular decks featuring ${card.displayName} as leader.`;
const canonicalUrl = `${config.public.API_BASE_URL}/decks/${leaderSlug}`;
const ogImage = GetCrop(card.imageUrl, undefined);

useHead({
  title: pageTitle,
  meta: [
    { name: 'description', content: metaDescription },
    { property: 'og:title', content: pageTitle },
    { property: 'og:description', content: metaDescription },
    { property: 'og:image', content: ogImage },
    { property: 'og:url', content: canonicalUrl },
  ],
  link: [
    { rel: 'canonical', href: canonicalUrl },
  ],
});
</script>

<template>
  <div class="container px-4 pt-8 md:px-8 mb-6">
    <div class="flex flex-col sm:flex-row gap-6 mb-6">
      <div v-if="card.imageUrl" class="shrink-0">
        <img class="h-48" :src="GetCrop(card.imageUrl, undefined)" :alt="card.displayName" />
      </div>
      <div>
        <h1 class="text-2xl font-bold mb-2">{{ card.displayName }} Decks</h1>
        <p class="text-gray-600">
          Browse the best {{ card.displayName }} decks. Find top-rated and popular decks featuring {{ card.displayName }} as leader.
        </p>
      </div>
    </div>
    <DeckOverview :decks-per-row="4" :leader-card-id="card.baseId"></DeckOverview>
  </div>
</template>
