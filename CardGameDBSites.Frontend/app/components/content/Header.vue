<script setup lang="ts">
import { PhMagnifyingGlass } from "@phosphor-icons/vue";
import type { HeaderPropertiesModel } from "~/api/umbraco";

const props = defineProps<{
  content: HeaderPropertiesModel;
}>();

const siteSettings = await useSite().getSettings();

const search = ref("");
const tagLines = props.content.subtitles ?? [
  "Every Card. Every Set. One Place",
  "The Ultimate Card Game Database",
  "Your Complete Card Game Companion",
  "Card Data at Your Fingertips",
];

// useState carries the server's pick to the client, so hydration keeps the same tagline.
const selectedTagLine = useState(
  `header-tagline-${props.content.title}`,
  () => tagLines[Math.floor(Math.random() * tagLines.length)],
);

function submitSearch(event: Event) {
  event.preventDefault();

  useRouter().push(`/cards?search=${encodeURIComponent(search.value)}`);
}
</script>

<template>
  <section class="flex min-h-[80svh] items-center justify-center bg-canvas px-4 py-10 text-ink">
    <div class="flex w-full flex-col items-center gap-6 text-center">
      <div>
        <h1 class="mb-2 text-4xl tracking-tight md:text-5xl">{{ content.title }}</h1>
        <p class="text-base text-ink-muted">{{ selectedTagLine }}</p>
      </div>
      <form
        class="flex w-full max-w-xl items-center gap-2 rounded-lg border border-line bg-surface p-1.5 pl-4 shadow-sm focus-within:border-ink-muted"
        role="search"
        @submit="submitSearch"
      >
        <input
          v-model="search"
          class="min-w-0 grow bg-transparent py-1.5 text-ink outline-none placeholder:text-ink-muted"
          name="search"
          type="text"
          enterkeyhint="search"
          aria-label="Search cards"
          placeholder="Search cards..."
        />
        <button
          :class="siteSettings.textColorWhite ? 'text-white' : 'text-black'"
          class="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-main-color hover:bg-main-color-hover"
          type="submit"
          aria-label="Search"
        >
          <PhMagnifyingGlass :size="18" weight="bold" />
        </button>
      </form>
      <div v-if="content.searchLinks?.length" class="flex flex-wrap justify-center gap-2">
        <NuxtLink
          v-for="link in content.searchLinks"
          :key="link.route?.path ?? link.url!"
          :to="link.route?.path ?? link.url!"
          class="btn-secondary no-underline text-sm"
        >
          {{ link.title }}
        </NuxtLink>
      </div>
    </div>
  </section>
</template>
