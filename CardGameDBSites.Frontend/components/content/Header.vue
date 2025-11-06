<script setup lang="ts">
import { PhArrowRight, PhMagnifyingGlass } from "@phosphor-icons/vue";
import type { HeaderPropertiesModel } from "~/api/umbraco";

const props = defineProps<{
  content: HeaderPropertiesModel;
}>();

const search = ref("");
const tagLines = [
  "Every Card. Every Set. One Place",
  "The Ultimate Card Game Database",
  "Your Complete Card Game Companion",
  "Card Data at Your Fingertips",
];
// Randomly select a tag line
const randomIndex = Math.floor(Math.random() * tagLines.length);
const selectedTagLine = tagLines[randomIndex];

function submitSearch(event: Event) {
  event.preventDefault();

  useRouter().push(`/cards?search=${search.value}`);
}
</script>

<template>
  <div
    class="relative mb-16 full-without-header bg-gradient-to-b from-gray-500 to-gray-100"
  >
    <div
      class="flex flex-col gap-4 items-center justify-center h-full z-10 relative"
    >
      <div class="pb-6 text-center">
        <h1 class="text-6xl pb-2">Aidalon-DB</h1>
        <h2 class="text-2xl">{{ selectedTagLine }}</h2>
      </div>
      <form
        class="flex overflow-hidden rounded border border-solid border-gray-300 sm:w-1/2 bg-white shadow-sm"
        @submit="submitSearch"
      >
        <input
          class="pl-4 py-4 grow"
          name="search"
          type="text"
          placeholder="Search cards..."
          v-model="search"
        />
        <button
          class="flex justify-center items-center w-8 text-lg px-1 bg-transparent"
          type="submit"
        >
          <PhMagnifyingGlass> </PhMagnifyingGlass>
        </button>
      </form>
      <div class="flex gap-2" v-if="props.content.searchLinks">
        <NuxtLink
          .to="link.url!"
          v-for="link in props.content.searchLinks"
          class="flex gap-1 items-center no-underline pointer"
        >
          <PhArrowRight/>
          <p class="no-underline">{{ link.title }}</p>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>
