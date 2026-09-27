<script setup lang="ts">
import type NavigationItem from "./NavigationItemModel";
import { ToContentSlug, useCmsPage } from "~/composables/useCmsPage";
import type { ApiContentModel } from "~/models/ApiContentModel";

const props = defineProps<{
  segments: NavigationItem[];
}>();

const route = useRoute();
const router = useRouter();
const { loadContent, resolveComponent } = useCmsPage();

// Switching segments is cheap after the first visit; apps are expected to feel instant here.
const cache = new Map<string, ApiContentModel>();
const content = ref<ApiContentModel>();
const isLoading = ref(false);
const failed = ref(false);

const active = computed(() => {
  const requested = route.query.view;
  return (
    props.segments.find((segment) => ToContentSlug(segment.url) === requested) ??
    props.segments[0]
  );
});

function select(segment: NavigationItem) {
  router.replace({ query: { view: ToContentSlug(segment.url) } });
}

watch(
  active,
  async (segment) => {
    if (!segment) {
      return;
    }

    const cached = cache.get(segment.url);
    if (cached) {
      content.value = cached;
      failed.value = false;
      return;
    }

    isLoading.value = true;
    failed.value = false;
    try {
      const data = await loadContent(segment.url);
      cache.set(segment.url, data);
      content.value = data;
    } catch {
      content.value = undefined;
      failed.value = true;
    } finally {
      isLoading.value = false;
    }
  },
  { immediate: true }
);
</script>

<template>
  <div>
    <div
      v-if="segments.length > 1"
      class="sticky top-safe-top z-10 flex overflow-x-auto bg-white border-b border-gray-200"
      role="tablist"
    >
      <button
        v-for="segment in segments"
        :key="segment.url"
        role="tab"
        :aria-selected="segment.url === active?.url"
        class="whitespace-nowrap px-4 py-3 text-sm border-b-2"
        :class="
          segment.url === active?.url
            ? 'border-main-color text-main-color font-semibold'
            : 'border-transparent text-gray-500'
        "
        @click="select(segment)"
      >
        {{ segment.name }}
      </button>
    </div>

    <component
      :is="resolveComponent(content.contentType)"
      v-if="content && resolveComponent(content.contentType)"
      :key="active?.url"
      :content="content"
    />
    <p v-else-if="isLoading" class="p-8 text-center text-gray-500">Loading…</p>
    <p v-else-if="failed" class="p-8 text-center text-gray-500">
      Could not load this page.
    </p>
  </div>
</template>
