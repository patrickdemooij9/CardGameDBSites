<script setup lang="ts">
import { PhChartBar } from "@phosphor-icons/vue";
import SetService from "~/services/SetService";
import ProgressBar from "../shared/ProgressBar.vue";
import { DoServerFetch } from "~/helpers/RequestsHelper";
import type { SetProgressApiModel } from "~/api/default";
import { useAccountStore } from "~/stores/AccountStore";

type CollectionSettingsResponse = {
  allowSetCollecting: boolean;
  allowCardCollecting: boolean;
  showProgressBar: boolean;
};

const sets = await new SetService().getAllSets();
const setsProgress = ref<SetProgressApiModel[]>([]);
const collectionSetIds = ref<number[]>([]);
const collectionSettings = ref({
  allowSetCollecting: false,
  allowCardCollecting: true,
  showProgressBar: true,
});
const processingSetIds = ref<number[]>([]);
const isLoading = ref(true);
const emit = defineEmits<{
  collectionUpdated: [];
}>();

const accountStore = useAccountStore();
const isLoggedIn = await accountStore.checkLogin();
const uncategorizedLabel = "";

const groupedSets = computed(() => {
  const groups = new Map<string, typeof sets>();
  sets.forEach((set) => {
    const category = set.category?.trim() || uncategorizedLabel;
    const current = groups.get(category) ?? [];
    current.push(set);
    groups.set(category, current);
  });

  return [...groups.entries()].map(([category, sets]) => ({
    category,
    sets,
  }));
});

function getProgress(setId: number): SetProgressApiModel | undefined {
  return setsProgress.value.find((sp) => sp.setId === setId);
}

function calculateProgressFilled(setId: number): number {
  const progress = getProgress(setId);
  if (!progress || progress.totalCards === 0) {
    return 0;
  }
  return (progress.ownedCards! / progress.totalCards!) * 100;
}

function isSetInCollection(setId: number): boolean {
  return collectionSetIds.value.includes(setId);
}

async function updateSetCollection(setId: number) {
  if (!isLoggedIn || processingSetIds.value.includes(setId)) {
    return;
  }

  processingSetIds.value = [...processingSetIds.value, setId];
  const isInCollection = isSetInCollection(setId);
  const endpoint = isInCollection ? "removeSet" : "addSet";

  try {
    await DoServerFetch(`/api/collection/${endpoint}?setId=${setId}`, true, {
      method: "POST",
    });

    collectionSetIds.value = isInCollection
      ? collectionSetIds.value.filter((id) => id !== setId)
      : [...collectionSetIds.value, setId];
    emit("collectionUpdated");
  } finally {
    processingSetIds.value = processingSetIds.value.filter((id) => id !== setId);
  }
}

onMounted(async () => {
  if (isLoggedIn) {
    collectionSettings.value = await DoServerFetch<CollectionSettingsResponse>(
      "/api/collection/settings"
    );
    if (collectionSettings.value.allowSetCollecting) {
      collectionSetIds.value = await DoServerFetch<number[]>("/api/collection/sets");
    } else if (
      collectionSettings.value.allowCardCollecting &&
      collectionSettings.value.showProgressBar
    ) {
      setsProgress.value = await DoServerFetch<SetProgressApiModel[]>(
        "/api/collection/setsProgress"
      );
    }
  }
  isLoading.value = false;
});
</script>

<template>
  <div v-if="isLoading" class="flex justify-center items-center py-12">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>
  <div v-else class="flex flex-col gap-8">
    <div v-for="group in groupedSets" :key="group.category" class="flex flex-col gap-4">
      <h3 class="text-lg font-bold">{{ group.category }}</h3>
      <div class="grid md:grid-cols-5 sm:grid-cols-2 grid-cols-1 gap-6">
        <div
          v-for="set in group.sets"
          :key="set.id"
          class="flex flex-col gap-2 rounded-md border-2 border-gray-300 bg-white p-4"
        >
          <NuxtLink
            :to="set.urlSegment"
            class="flex gap-2 items-baseline text-base no-underline"
          >
            <h2 class="text-base font-bold">{{ set.displayName }}</h2>
            <i v-if="set.code" class="text-xs">{{ set.code }}</i>
            <p
              v-if="
                accountStore.isLoggedIn &&
                collectionSettings.allowSetCollecting &&
                isSetInCollection(set.id)
              "
              class="ml-auto text-xs text-green-600 font-bold"
            >
              Owned
            </p>
          </NuxtLink>
          <div class="flex justify-between">
            <img class="h-16" v-if="set.imageUrl" :src="set.imageUrl" />
            <div class="flex flex-col justify-center">
              <span v-for="info in set.extraInformation" class="text-sm">{{
                info
              }}</span>
            </div>
          </div>
          <div class="flex justify-between gap-4">
            <div class="grow self-center">
              <div v-if="accountStore.isLoggedIn">
                <button
                  v-if="collectionSettings.allowSetCollecting"
                  type="button"
                  class="btn btn-outline px-2 py-1"
                  :disabled="processingSetIds.includes(set.id)"
                  @click="updateSetCollection(set.id)"
                >
                  {{
                    isSetInCollection(set.id)
                      ? "Remove from collection"
                      : "Add to collection"
                  }}
                </button>
                <ProgressBar
                  v-else-if="
                    collectionSettings.allowCardCollecting &&
                    collectionSettings.showProgressBar
                  "
                  :percent-filled="calculateProgressFilled(set.id)"
                  :description="`${getProgress(set.id)?.ownedCards}/${getProgress(set.id)?.totalCards}`"
                ></ProgressBar>
              </div>
            </div>
            <NuxtLink
              :to="set.urlSegment"
              class="border border-solid flex gap-2 rounded items-center px-2 py-1 bg-white"
            >
              <PhChartBar />
              <span>Details</span>
            </NuxtLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
