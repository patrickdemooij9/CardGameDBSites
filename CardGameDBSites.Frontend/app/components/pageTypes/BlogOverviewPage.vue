<script setup lang="ts">
import type { BlogOverviewContentModel } from "~/api/umbraco";
import type { CommunityBlogPostApiModel } from "~/api/default";
import CommunityService from "~/services/CommunityService";
import { ParseToHumanReadableText } from "~/helpers/DateHelper";
import { GetAbsoluteUrl } from "~/helpers/CropUrlHelper";

const props = defineProps<{
  content: BlogOverviewContentModel;
}>();

const pageSize = 30;
const page = ref(1);
const totalItems = ref(0);
const posts = ref<CommunityBlogPostApiModel[]>([]);
const isLoading = ref(false);

async function loadPosts() {
  isLoading.value = true;
  try {
    const result = await new CommunityService().getPosts(page.value, pageSize);
    posts.value = result.items;
    totalItems.value = result.totalItems ?? 0;
  } finally {
    isLoading.value = false;
  }
}

async function previousPage() {
  if (page.value > 1) {
    page.value--;
    await loadPosts();
  }
}

async function nextPage() {
  page.value++;
  await loadPosts();
}

const hasNextPage = computed(() => page.value * pageSize < totalItems.value);

await loadPosts();
</script>

<template>
  <div class="container px-4 pt-8 md:px-8">
    <div class="mb-6">
      <h1>Community posts</h1>
      <p v-if="content.properties?.description">
        {{ content.properties.description }}
      </p>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <a
        v-for="post in posts"
        :key="post.id"
        :href="post.url"
        class="bg-white rounded hover:shadow-md transition-shadow no-underline"
      >
        <img
          v-if="post.imageUrl"
          :src="
            post.imageUrl.startsWith('/')
              ? GetAbsoluteUrl(post.imageUrl)
              : post.imageUrl
          "
          :alt="post.title"
          class="w-full h-48 object-cover"
        />
        <div class="p-4">
          <span
            class="text-sm font-medium px-2 py-1 rounded bg-gray-100 mb-2 inline-block"
          >
            {{ post.tagType }}
          </span>
          <h2 class="text-lg font-semibold mt-2">{{ post.title }}</h2>
          <p class="text-sm text-gray-600 mt-1">
            {{ post.author }} &mdash;
            {{ ParseToHumanReadableText(post.publishedDate!) }}
          </p>
        </div>
      </a>
    </div>

    <div v-if="page > 1 || hasNextPage" class="mt-6 mb-6 flex justify-center gap-4">
      <button
        v-if="page > 1"
        class="btn"
        :disabled="isLoading"
        @click="previousPage"
      >
        Previous
      </button>
      <button
        v-if="hasNextPage"
        class="btn"
        :disabled="isLoading"
        @click="nextPage"
      >
        Next
      </button>
    </div>
  </div>
</template>
