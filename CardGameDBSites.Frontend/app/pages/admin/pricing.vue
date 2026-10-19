<script setup lang="ts">
import { useAccountStore } from "~/stores/AccountStore";
import { DoServerFetch } from "~/helpers/RequestsHelper";
import TcgPlayerIdEditor from "~/components/admin/TcgPlayerIdEditor.vue";

const accountStore = useAccountStore();

type MissingPriceReason = "NoTcgPlayerId" | "InvalidTcgPlayerId" | "NoPriceFound";

interface PricingCard {
  baseId: number;
  variantId: number;
  displayName: string;
  setId: number;
  setName: string;
  variantTypeName: string;
  tcgPlayerId: string | null;
}

interface MissingPrice {
  card: PricingCard;
  reason: MissingPriceReason;
}

interface TcgPlayerIdConflict {
  tcgPlayerId: string;
  cards: PricingCard[];
}

interface SyncStatus {
  isRunning: boolean;
  startedUtc: string | null;
  finishedUtc: string | null;
  syncedCards: number;
  error: string | null;
  latestPriceDate: string | null;
}

interface PricingOverview {
  priceableCardCount: number;
  missingPrices: MissingPrice[];
  conflicts: TcgPlayerIdConflict[];
  syncStatus: SyncStatus;
}

const reasonLabels: Record<MissingPriceReason, string> = {
  NoTcgPlayerId: "No TCGplayer ID",
  InvalidTcgPlayerId: "Invalid TCGplayer ID",
  NoPriceFound: "No price found",
};

const reasonClasses: Record<MissingPriceReason, string> = {
  NoTcgPlayerId: "bg-gray-100 text-gray-700",
  InvalidTcgPlayerId: "bg-red-100 text-red-800",
  NoPriceFound: "bg-yellow-100 text-yellow-800",
};

const pageSize = 200;

const overview = ref<PricingOverview | null>(null);
const syncStatus = ref<SyncStatus | null>(null);
const isLoading = ref(false);
const isStartingSync = ref(false);
const errorMessage = ref<string | null>(null);
const syncMessage = ref<string | null>(null);

const activeTab = ref<"missing" | "conflicts">("missing");
const search = ref("");
const setFilter = ref<number | null>(null);
const reasonFilter = ref<MissingPriceReason | null>(null);
const visibleCount = ref(pageSize);

let pollTimer: ReturnType<typeof setInterval> | null = null;

const sets = computed(() => {
  const byId = new Map<number, string>();
  for (const item of overview.value?.missingPrices ?? []) {
    byId.set(item.card.setId, item.card.setName);
  }
  return [...byId.entries()]
    .map(([id, name]) => ({ id, name }))
    .sort((a, b) => a.name.localeCompare(b.name));
});

const reasonCounts = computed(() => {
  const counts: Record<MissingPriceReason, number> = {
    NoTcgPlayerId: 0,
    InvalidTcgPlayerId: 0,
    NoPriceFound: 0,
  };
  for (const item of overview.value?.missingPrices ?? []) {
    counts[item.reason]++;
  }
  return counts;
});

const filteredMissingPrices = computed(() => {
  const term = search.value.trim().toLowerCase();
  return (overview.value?.missingPrices ?? []).filter(
    (item) =>
      (setFilter.value === null || item.card.setId === setFilter.value) &&
      (reasonFilter.value === null || item.reason === reasonFilter.value) &&
      (!term ||
        item.card.displayName.toLowerCase().includes(term) ||
        item.card.tcgPlayerId?.includes(term) ||
        String(item.card.variantId) === term)
  );
});

const visibleMissingPrices = computed(() =>
  filteredMissingPrices.value.slice(0, visibleCount.value)
);

watch([search, setFilter, reasonFilter], () => {
  visibleCount.value = pageSize;
});

async function loadOverview() {
  isLoading.value = true;
  errorMessage.value = null;
  try {
    overview.value = await DoServerFetch<PricingOverview>(
      "/api/admin/pricing/overview",
      true
    );
    syncStatus.value = overview.value.syncStatus;
    if (syncStatus.value.isRunning) startPolling();
  } catch {
    errorMessage.value = "Failed to load the pricing overview.";
  } finally {
    isLoading.value = false;
  }
}

async function startSync() {
  isStartingSync.value = true;
  syncMessage.value = null;
  try {
    syncStatus.value = await DoServerFetch<SyncStatus>(
      "/api/admin/pricing/sync",
      true,
      { method: "POST" }
    );
    startPolling();
  } catch (error: any) {
    syncMessage.value =
      error?.statusCode === 409
        ? "A price sync is already running."
        : "Failed to start the price sync.";
    await pollStatus();
  } finally {
    isStartingSync.value = false;
  }
}

async function pollStatus() {
  try {
    const wasRunning = syncStatus.value?.isRunning ?? false;
    syncStatus.value = await DoServerFetch<SyncStatus>(
      "/api/admin/pricing/sync",
      true
    );
    if (syncStatus.value.isRunning) {
      startPolling();
    } else {
      stopPolling();
      if (wasRunning) await loadOverview();
    }
  } catch {
    stopPolling();
  }
}

function startPolling() {
  if (pollTimer) return;
  pollTimer = setInterval(pollStatus, 5000);
}

function stopPolling() {
  if (!pollTimer) return;
  clearInterval(pollTimer);
  pollTimer = null;
}

function onMissingPriceIdSaved(item: MissingPrice, tcgPlayerId: string | null) {
  item.card.tcgPlayerId = tcgPlayerId;
  item.reason = tcgPlayerId ? "NoPriceFound" : "NoTcgPlayerId";
}

function onConflictIdSaved(
  conflict: TcgPlayerIdConflict,
  card: PricingCard,
  tcgPlayerId: string | null
) {
  card.tcgPlayerId = tcgPlayerId;
  conflict.cards = conflict.cards.filter((c) => c.tcgPlayerId === conflict.tcgPlayerId);
  if (overview.value && conflict.cards.length < 2) {
    overview.value.conflicts = overview.value.conflicts.filter((c) => c !== conflict);
  }
}

function tcgPlayerUrl(id: string) {
  return `https://www.tcgplayer.com/product/${encodeURIComponent(id)}`;
}

function formatDateTime(iso: string | null) {
  return iso ? new Date(iso).toLocaleString() : "—";
}

function formatDate(iso: string | null) {
  return iso ? new Date(iso).toLocaleDateString() : "—";
}

onMounted(async () => {
  await accountStore.checkLogin();
  if (!accountStore.member?.isAdmin) {
    navigateTo("/");
    return;
  }
  await loadOverview();
});

onBeforeUnmount(stopPolling);
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold">Card Pricing</h1>
      <button
        class="bg-gray-100 border border-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-200 text-sm"
        :disabled="isLoading"
        @click="loadOverview"
      >
        {{ isLoading ? "Loading…" : "Refresh" }}
      </button>
    </div>

    <p v-if="errorMessage" class="text-red-600 mb-4">{{ errorMessage }}</p>

    <div class="bg-white rounded shadow p-4 mb-6">
      <div class="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 class="text-lg font-semibold mb-2">Price sync</h2>
          <dl class="grid grid-cols-[auto_1fr] gap-x-4 gap-y-1 text-sm">
            <dt class="text-gray-500">Status</dt>
            <dd>
              <span
                v-if="syncStatus?.isRunning"
                class="inline-flex items-center gap-2 text-blue-700 font-medium"
              >
                <span class="animate-spin rounded-full h-3 w-3 border-b-2 border-current"></span>
                Running…
              </span>
              <span v-else-if="syncStatus?.error" class="text-red-600 font-medium">
                Failed: {{ syncStatus.error }}
              </span>
              <span v-else-if="syncStatus?.finishedUtc" class="text-green-700 font-medium">
                Idle
              </span>
              <span v-else class="text-gray-500">Not run since the server started</span>
            </dd>
            <dt class="text-gray-500">Last started</dt>
            <dd>{{ formatDateTime(syncStatus?.startedUtc ?? null) }}</dd>
            <dt class="text-gray-500">Last finished</dt>
            <dd>{{ formatDateTime(syncStatus?.finishedUtc ?? null) }}</dd>
            <dt class="text-gray-500">Cards synced</dt>
            <dd>{{ syncStatus?.syncedCards ?? 0 }}</dd>
            <dt class="text-gray-500">Latest price date</dt>
            <dd>{{ formatDate(syncStatus?.latestPriceDate ?? null) }}</dd>
          </dl>
        </div>
        <div class="flex flex-col items-end gap-2">
          <button
            class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 text-sm font-medium disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="isStartingSync || syncStatus?.isRunning"
            @click="startSync"
          >
            {{ syncStatus?.isRunning ? "Sync running…" : "Start price sync" }}
          </button>
          <span class="text-xs text-gray-400">Syncs this site's prices from TCGplayer</span>
          <span v-if="syncMessage" class="text-red-600 text-sm">{{ syncMessage }}</span>
        </div>
      </div>
    </div>

    <template v-if="overview">
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
        <div class="bg-white rounded shadow p-4">
          <div class="text-xs font-semibold text-gray-500 uppercase">Priceable cards</div>
          <div class="text-2xl font-bold">{{ overview.priceableCardCount }}</div>
        </div>
        <div class="bg-white rounded shadow p-4">
          <div class="text-xs font-semibold text-gray-500 uppercase">Without a price</div>
          <div class="text-2xl font-bold">{{ overview.missingPrices.length }}</div>
        </div>
        <div class="bg-white rounded shadow p-4">
          <div class="text-xs font-semibold text-gray-500 uppercase">Conflicting TCGplayer IDs</div>
          <div
            class="text-2xl font-bold"
            :class="{ 'text-red-600': overview.conflicts.length > 0 }"
          >
            {{ overview.conflicts.length }}
          </div>
        </div>
      </div>

      <div class="flex gap-2 mb-4 border-b border-gray-200">
        <button
          class="px-4 py-2 text-sm font-medium -mb-px border-b-2"
          :class="activeTab === 'missing' ? 'border-blue-600 text-blue-700' : 'border-transparent text-gray-500 hover:text-gray-700'"
          @click="activeTab = 'missing'"
        >
          Missing prices ({{ overview.missingPrices.length }})
        </button>
        <button
          class="px-4 py-2 text-sm font-medium -mb-px border-b-2"
          :class="activeTab === 'conflicts' ? 'border-blue-600 text-blue-700' : 'border-transparent text-gray-500 hover:text-gray-700'"
          @click="activeTab = 'conflicts'"
        >
          Conflicting IDs ({{ overview.conflicts.length }})
        </button>
      </div>

      <div v-if="activeTab === 'missing'" class="bg-white rounded shadow p-4">
        <div class="flex flex-wrap gap-4 mb-4">
          <div class="flex flex-col w-full max-w-xs">
            <label class="text-xs font-semibold text-gray-500 mb-0.5">Search</label>
            <input
              v-model="search"
              type="text"
              placeholder="Name, TCGplayer ID or variant ID"
              class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
          </div>
          <div class="flex flex-col w-full max-w-xs">
            <label class="text-xs font-semibold text-gray-500 mb-0.5">Set</label>
            <select
              v-model="setFilter"
              class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            >
              <option :value="null">All sets</option>
              <option v-for="set in sets" :key="set.id" :value="set.id">{{ set.name }}</option>
            </select>
          </div>
          <div class="flex flex-col w-full max-w-xs">
            <label class="text-xs font-semibold text-gray-500 mb-0.5">Reason</label>
            <select
              v-model="reasonFilter"
              class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            >
              <option :value="null">All reasons</option>
              <option v-for="(label, reason) in reasonLabels" :key="reason" :value="reason">
                {{ label }} ({{ reasonCounts[reason] }})
              </option>
            </select>
          </div>
        </div>

        <p v-if="filteredMissingPrices.length === 0" class="text-gray-500 text-sm">
          Every card matching these filters has a price.
        </p>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="text-left text-xs font-semibold text-gray-500 border-b border-gray-200">
                <th class="py-2 pr-4">Card</th>
                <th class="py-2 pr-4">Set</th>
                <th class="py-2 pr-4">Variant</th>
                <th class="py-2 pr-4">TCGplayer ID</th>
                <th class="py-2 pr-4">Reason</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="item in visibleMissingPrices"
                :key="item.card.variantId"
                class="border-b border-gray-100 last:border-0"
              >
                <td class="py-2 pr-4">
                  <div class="font-medium">{{ item.card.displayName }}</div>
                  <div class="text-xs text-gray-400">
                    Card #{{ item.card.baseId }} · Variant #{{ item.card.variantId }}
                  </div>
                </td>
                <td class="py-2 pr-4">{{ item.card.setName }}</td>
                <td class="py-2 pr-4">{{ item.card.variantTypeName }}</td>
                <td class="py-2 pr-4">
                  <TcgPlayerIdEditor
                    :variant-id="item.card.variantId"
                    :tcg-player-id="item.card.tcgPlayerId"
                    @saved="(id) => onMissingPriceIdSaved(item, id)"
                  >
                    <a
                      v-if="item.card.tcgPlayerId && item.reason === 'NoPriceFound'"
                      :href="tcgPlayerUrl(item.card.tcgPlayerId)"
                      target="_blank"
                      rel="noopener"
                      class="text-blue-500 hover:underline"
                    >
                      {{ item.card.tcgPlayerId }}
                    </a>
                    <span v-else-if="item.card.tcgPlayerId">{{ item.card.tcgPlayerId }}</span>
                    <span v-else class="text-gray-400">—</span>
                  </TcgPlayerIdEditor>
                </td>
                <td class="py-2 pr-4">
                  <span
                    class="text-xs font-semibold px-2 py-0.5 rounded whitespace-nowrap"
                    :class="reasonClasses[item.reason]"
                  >
                    {{ reasonLabels[item.reason] }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>

          <div
            v-if="filteredMissingPrices.length > visibleMissingPrices.length"
            class="flex items-center gap-3 mt-4"
          >
            <button
              class="bg-gray-100 border border-gray-300 text-gray-700 px-4 py-1.5 rounded hover:bg-gray-200 text-sm"
              @click="visibleCount += pageSize"
            >
              Show more
            </button>
            <span class="text-xs text-gray-400">
              Showing {{ visibleMissingPrices.length }} of {{ filteredMissingPrices.length }}
            </span>
          </div>
        </div>
      </div>

      <div v-else class="space-y-4">
        <p v-if="overview.conflicts.length === 0" class="text-gray-500 text-sm">
          No two cards share a TCGplayer ID.
        </p>
        <p v-else class="text-xs text-gray-500">
          The price sync only prices the first card it finds for each ID, so the others in a group stay
          unpriced. Give each card its own ID, or clear it where it doesn't belong.
        </p>
        <div
          v-for="conflict in overview.conflicts"
          :key="conflict.tcgPlayerId"
          class="bg-white rounded shadow p-4 border-l-4 border-red-400"
        >
          <div class="flex items-center gap-3 mb-2">
            <span class="text-sm font-semibold">TCGplayer ID {{ conflict.tcgPlayerId }}</span>
            <a
              :href="tcgPlayerUrl(conflict.tcgPlayerId)"
              target="_blank"
              rel="noopener"
              class="text-xs text-blue-500 hover:underline"
            >
              View on TCGplayer
            </a>
            <span class="text-xs text-gray-400 ml-auto">{{ conflict.cards.length }} cards</span>
          </div>
          <ul class="divide-y divide-gray-100 text-sm">
            <li
              v-for="card in conflict.cards"
              :key="card.variantId"
              class="py-1.5 flex flex-wrap items-center gap-x-4 gap-y-0.5"
            >
              <span class="font-medium">{{ card.displayName }}</span>
              <span class="text-gray-600">{{ card.setName }}</span>
              <span class="text-gray-600">{{ card.variantTypeName }}</span>
              <span class="text-xs text-gray-400 ml-auto">
                Card #{{ card.baseId }} · Variant #{{ card.variantId }}
              </span>
              <TcgPlayerIdEditor
                :variant-id="card.variantId"
                :tcg-player-id="card.tcgPlayerId"
                @saved="(id) => onConflictIdSaved(conflict, card, id)"
              >
                <span></span>
              </TcgPlayerIdEditor>
            </li>
          </ul>
        </div>
      </div>
    </template>
  </div>
</template>
