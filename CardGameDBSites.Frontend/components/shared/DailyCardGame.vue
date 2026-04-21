<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import type { DailyGameBootstrap } from "~/models/dailygame/DailyGameModels";
import { applyGuessResult, canGuess, shouldShowLeaderboard } from "~/services/dailygame/DailyGameStateService";

const { bootstrap, submitGuess } = useDailyGame();
const accountStore = useAccountStore();

const state = ref<DailyGameBootstrap | null>(null);
const isLoading = ref(false);
const isSubmitting = ref(false);
const error = ref<string | null>(null);
const elapsedSeconds = ref(0);
let timer: ReturnType<typeof setInterval> | null = null;

const imageUrl = computed(() => "/api/proxy/api/dailygame/image");
const canSubmit = computed(() => !!state.value && canGuess(state.value) && !isSubmitting.value);

function formatSeconds(totalSeconds: number): string {
  const minutes = Math.floor(totalSeconds / 60)
    .toString()
    .padStart(2, "0");
  const seconds = (totalSeconds % 60).toString().padStart(2, "0");
  return `${minutes}:${seconds}`;
}

function resetTimer(baseElapsed: number, finished: boolean) {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
  elapsedSeconds.value = baseElapsed;

  if (!finished) {
    timer = setInterval(() => {
      elapsedSeconds.value += 1;
    }, 1000);
  }
}

async function loadSession() {
  isLoading.value = true;
  error.value = null;
  try {
    const token = useCookie<string | null>("dailyGameGuestToken", {
      sameSite: "lax",
      secure: true,
    });
    const result = await bootstrap(token.value || undefined);
    state.value = result;
    token.value = result.guestSessionToken;
    resetTimer(result.elapsedSeconds, result.isFinished);
  } catch {
    error.value = "Could not load the daily game.";
  } finally {
    isLoading.value = false;
  }
}

async function onSelectCard(card: CardDetailApiModel) {
  if (!canSubmit.value || !card.variantId) {
    return;
  }

  isSubmitting.value = true;
  error.value = null;
  try {
    const result = await submitGuess({
      guessedCardId: card.variantId,
      guestSessionToken: state.value?.guestSessionToken,
    });
    if (state.value) {
      state.value = applyGuessResult(state.value, result);
    } else {
      state.value = result.state;
    }
    resetTimer(result.state.elapsedSeconds, result.state.isFinished);
  } catch {
    error.value = "Could not submit guess.";
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(async () => {
  await loadSession();
});

onBeforeUnmount(() => {
  if (timer) {
    clearInterval(timer);
  }
});
</script>

<template>
  <div class="border border-gray-200 rounded-lg p-4 bg-white shadow-sm max-w-4xl mx-auto">
    <div class="flex justify-between items-center mb-4">
      <h2 class="text-xl font-bold">Daily Card Guess</h2>
      <p class="text-sm text-gray-600">Time: {{ formatSeconds(elapsedSeconds) }}</p>
    </div>

    <p v-if="error" class="text-red-600 mb-3">{{ error }}</p>

    <div v-if="isLoading || !state" class="text-gray-600">Loading game...</div>

    <template v-else>
      <div class="mb-4 flex justify-between text-sm text-gray-700">
        <p>Attempts: {{ state.attemptsUsed }}/{{ state.maxAttempts }}</p>
        <p v-if="state.isFinished" class="font-semibold">
          {{ state.isSolved ? "Solved!" : "No attempts left" }}
        </p>
      </div>

      <div class="mb-4 flex justify-center">
        <img
          :src="imageUrl"
          alt="Daily card"
          class="max-h-[420px] rounded-md"
          :style="{ filter: `blur(${state.blurLevel}px)` }"
        />
      </div>

      <div v-if="canSubmit" class="mb-4">
        <CardSearchInput @select="onSelectCard" />
      </div>

      <div class="space-y-3 mb-4">
        <div
          v-for="attempt in state.attempts"
          :key="attempt.attemptNumber"
          class="border border-gray-100 rounded p-3"
        >
          <p class="font-semibold">
            Attempt {{ attempt.attemptNumber }}: {{ attempt.guessedCardName || `Card #${attempt.guessedCardId}` }}
          </p>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-2 mt-2 text-sm">
            <div
              v-for="attribute in attempt.feedback"
              :key="`${attempt.attemptNumber}-${attribute.name}`"
              class="rounded px-2 py-1"
              :class="{
                'bg-green-100': attribute.matchType === 'exact',
                'bg-yellow-100': attribute.matchType === 'partial',
                'bg-blue-100': attribute.matchType === 'higher' || attribute.matchType === 'lower',
                'bg-gray-100': ['none', 'unknown'].includes(attribute.matchType)
              }"
            >
              <span class="font-medium">{{ attribute.name }}:</span>
              <span class="ml-1">{{ attribute.guessValues.join(', ') || '—' }}</span>
              <span class="ml-1 text-xs uppercase">({{ attribute.matchType }})</span>
            </div>
          </div>
        </div>
      </div>

      <div v-if="shouldShowLeaderboard(state)" class="mt-6">
        <h3 class="text-lg font-semibold mb-2">Leaderboard</h3>
        <p v-if="state.currentPlacement" class="mb-2 text-sm text-gray-700">
          You are currently rank #{{ state.currentPlacement.rank }}.
        </p>
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="text-left border-b">
                <th class="py-2">Rank</th>
                <th class="py-2">Player</th>
                <th class="py-2">Attempts</th>
                <th class="py-2">Time</th>
                <th class="py-2">Result</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="entry in state.leaderboard"
                :key="`${entry.rank}-${entry.memberId ?? 'guest'}`"
                class="border-b"
                :class="{ 'bg-main-color/10': entry.isCurrentPlayer }"
              >
                <td class="py-2">#{{ entry.rank }}</td>
                <td class="py-2">{{ entry.memberName || 'Guest' }}</td>
                <td class="py-2">{{ entry.attemptsUsed }}</td>
                <td class="py-2">{{ formatSeconds(entry.elapsedSeconds) }}</td>
                <td class="py-2">{{ entry.solved ? 'Solved' : 'Failed' }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="!accountStore.isLoggedIn" class="mt-4 p-3 rounded bg-yellow-50 border border-yellow-200">
          <p class="text-sm mb-2">Want your result saved permanently?</p>
          <div class="flex gap-2">
            <NuxtLink to="/login" class="px-3 py-1 rounded bg-main-color text-white no-underline">Login</NuxtLink>
            <NuxtLink to="/register" class="px-3 py-1 rounded border border-main-color no-underline">Create account</NuxtLink>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
