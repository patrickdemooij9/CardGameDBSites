<script setup lang="ts">
import { useAccountStore } from '~/stores/AccountStore';
import TournamentService, {
  type TournamentEvent,
  type TournamentEntrant,
  type TournamentMatch,
  type CreateTournamentDto,
  type AddEntrantDto,
  type UpdateEntrantDto,
  type AddTournamentMatchDto,
  type ParsedDeckCard,
  type UnmatchedDeckCard,
} from '~/services/TournamentService';
import { ParseToHumanReadableText } from '~/helpers/DateHelper';

useHead({ title: 'Admin – Tournaments' });

const accountStore = useAccountStore();
await accountStore.checkLogin();
if (!accountStore.member?.isAdmin) {
  throw createError({ statusCode: 403, statusMessage: 'Forbidden' });
}

const svc = new TournamentService();

const tournaments = ref<TournamentEvent[]>([]);
const loadingTournaments = ref(false);
const expandedId = ref<string | null>(null);

async function loadTournaments() {
  loadingTournaments.value = true;
  try {
    tournaments.value = await svc.list();
  } finally {
    loadingTournaments.value = false;
  }
}
await loadTournaments();

function toggleExpand(id: string) {
  expandedId.value = expandedId.value === id ? null : id;
}

async function deleteTournament(id: string) {
  if (!confirm('Delete this tournament and all entrants/matches?')) return;
  await svc.deleteTournament(id);
  tournaments.value = tournaments.value.filter(t => t.id !== id);
  if (expandedId.value === id) expandedId.value = null;
}

const showCreateForm = ref(false);
const createTournamentError = ref('');
const creating = ref(false);
const createForm = ref<CreateTournamentDto>({
  name: '',
  date: new Date().toISOString().slice(0, 10),
  formatId: 1,
  playerCount: null,
  sourceUrl: null,
  sourceType: 'Manual',
});

async function submitCreate() {
  createTournamentError.value = '';
  creating.value = true;
  try {
    await svc.create({
      name: createForm.value.name,
      date: new Date(createForm.value.date).toISOString(),
      formatId: Number(createForm.value.formatId),
      playerCount: createForm.value.playerCount ? Number(createForm.value.playerCount) : null,
      sourceUrl: createForm.value.sourceUrl || null,
      sourceType: createForm.value.sourceType || 'Manual',
    });
    showCreateForm.value = false;
    createForm.value = {
      name: '',
      date: new Date().toISOString().slice(0, 10),
      formatId: 1,
      playerCount: null,
      sourceUrl: null,
      sourceType: 'Manual',
    };
    await loadTournaments();
  } catch (e: any) {
    createTournamentError.value = e?.data?.error ?? 'Failed to create tournament.';
  } finally {
    creating.value = false;
  }
}

const addEntrantForId = ref<string | null>(null);
const addEntrantForm = ref<AddEntrantDto>({
  playerName: '',
  placement: null,
  deckId: null,
});
const addEntrantError = ref('');
const addingEntrant = ref(false);

function openAddEntrant(tournamentId: string) {
  addEntrantForId.value = tournamentId;
  addEntrantForm.value = { playerName: '', placement: null, deckId: null };
  addEntrantError.value = '';
}

async function submitAddEntrant() {
  if (!addEntrantForId.value) return;
  addEntrantError.value = '';
  addingEntrant.value = true;
  try {
    await svc.addEntrant(addEntrantForId.value, {
      playerName: addEntrantForm.value.playerName,
      placement: addEntrantForm.value.placement ? Number(addEntrantForm.value.placement) : null,
      deckId: addEntrantForm.value.deckId ? Number(addEntrantForm.value.deckId) : null,
    });
    addEntrantForId.value = null;
    await loadTournaments();
  } catch (e: any) {
    addEntrantError.value = e?.data?.error ?? 'Failed to add entrant.';
  } finally {
    addingEntrant.value = false;
  }
}

const editEntrantId = ref<string | null>(null);
const editEntrantForm = ref<UpdateEntrantDto>({});
const editEntrantError = ref('');
const editingEntrant = ref(false);

function openEditEntrant(entrant: TournamentEntrant) {
  editEntrantId.value = entrant.id;
  editEntrantForm.value = {
    playerName: entrant.playerName,
    placement: entrant.placement,
    deckId: entrant.deckId,
  };
  editEntrantError.value = '';
}

async function submitEditEntrant() {
  if (!editEntrantId.value) return;
  editEntrantError.value = '';
  editingEntrant.value = true;
  try {
    await svc.updateEntrant(editEntrantId.value, {
      playerName: editEntrantForm.value.playerName,
      placement: editEntrantForm.value.placement !== null && editEntrantForm.value.placement !== undefined
        ? Number(editEntrantForm.value.placement)
        : null,
      deckId: editEntrantForm.value.deckId ? Number(editEntrantForm.value.deckId) : null,
    });
    editEntrantId.value = null;
    await loadTournaments();
  } catch (e: any) {
    editEntrantError.value = e?.data?.error ?? 'Failed to update entrant.';
  } finally {
    editingEntrant.value = false;
  }
}

async function deleteEntrant(entrantId: string) {
  if (!confirm('Delete this entrant and all their matches?')) return;
  await svc.deleteEntrant(entrantId);
  await loadTournaments();
}

const matchForms = ref<Record<string, AddTournamentMatchDto>>({});
const matchError = ref<Record<string, string>>({});
const addingMatch = ref<Record<string, boolean>>({});

function getMatchForm(entrantId: string): AddTournamentMatchDto {
  if (!matchForms.value[entrantId]) {
    matchForms.value[entrantId] = {
      roundNumber: null,
      opponentName: null,
      wins: 0,
      losses: 0,
      draws: 0,
    };
  }
  return matchForms.value[entrantId];
}

async function addMatch(entrantId: string) {
  const form = getMatchForm(entrantId);
  matchError.value[entrantId] = '';
  addingMatch.value[entrantId] = true;
  try {
    await svc.addMatch(entrantId, {
      roundNumber: form.roundNumber ? Number(form.roundNumber) : null,
      opponentName: form.opponentName || null,
      wins: Number(form.wins || 0),
      losses: Number(form.losses || 0),
      draws: Number(form.draws || 0),
    });
    matchForms.value[entrantId] = { roundNumber: null, opponentName: null, wins: 0, losses: 0, draws: 0 };
    await loadTournaments();
  } catch (e: any) {
    matchError.value[entrantId] = e?.data?.error ?? 'Failed to add match.';
  } finally {
    addingMatch.value[entrantId] = false;
  }
}

async function editMatch(match: TournamentMatch) {
  const roundValue = prompt('Round number (blank for none):', match.roundNumber?.toString() ?? '');
  if (roundValue === null) return;
  const opponentName = prompt('Opponent name (optional):', match.opponentName ?? '');
  if (opponentName === null) return;
  const wins = prompt('Wins in this match:', String(match.wins));
  if (wins === null) return;
  const losses = prompt('Losses in this match:', String(match.losses));
  if (losses === null) return;
  const draws = prompt('Draws in this match:', String(match.draws));
  if (draws === null) return;

  await svc.updateMatch(match.id, {
    roundNumber: roundValue.trim() === '' ? null : Number(roundValue),
    opponentName: opponentName.trim() === '' ? null : opponentName.trim(),
    wins: Number(wins),
    losses: Number(losses),
    draws: Number(draws),
  });
  await loadTournaments();
}

async function deleteMatch(matchId: string) {
  if (!confirm('Delete this match?')) return;
  await svc.deleteMatch(matchId);
  await loadTournaments();
}

const deckImportEntrantId = ref<string | null>(null);
const deckImportText = ref('');
const deckImportResult = ref<{ matched: ParsedDeckCard[]; unmatched: UnmatchedDeckCard[] } | null>(null);
const deckImportLoading = ref(false);
const deckImportError = ref('');
const deckImportDeckId = ref<number | null>(null);

function openDeckImport(entrantId: string) {
  deckImportEntrantId.value = entrantId;
  deckImportText.value = '';
  deckImportResult.value = null;
  deckImportError.value = '';
  deckImportDeckId.value = null;
}

async function parseDeckText() {
  deckImportError.value = '';
  deckImportResult.value = null;
  deckImportLoading.value = true;
  try {
    deckImportResult.value = await svc.parseDeck(deckImportText.value);
  } catch (e: any) {
    deckImportError.value = e?.data?.error ?? 'Failed to parse deck text.';
  } finally {
    deckImportLoading.value = false;
  }
}

async function applyDeckId() {
  if (!deckImportEntrantId.value || !deckImportDeckId.value) return;
  await svc.updateEntrant(deckImportEntrantId.value, { deckId: Number(deckImportDeckId.value) });
  deckImportEntrantId.value = null;
  await loadTournaments();
}

const sectionLabels: Record<string, string> = {
  MainDeck: 'Main Deck',
  Leader: 'Leader',
  Base: 'Base',
  Sideboard: 'Sideboard',
};
</script>

<template>
  <div class="container px-4 py-6 md:px-8">
    <h1 class="text-2xl font-bold mb-6">Admin – Tournament Management</h1>

    <div class="mb-6">
      <button
        v-if="!showCreateForm"
        class="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 text-sm font-medium"
        @click="showCreateForm = true"
      >
        + New Tournament
      </button>

      <div v-else class="bg-white rounded p-4 shadow max-w-xl">
        <h2 class="font-semibold mb-3">Create Tournament</h2>
        <div class="space-y-3">
          <div>
            <label class="text-xs text-gray-600 block mb-1">Name *</label>
            <input v-model="createForm.name" class="w-full border rounded px-2 py-1 text-sm" />
          </div>
          <div>
            <label class="text-xs text-gray-600 block mb-1">Date *</label>
            <input v-model="createForm.date" type="date" class="border rounded px-2 py-1 text-sm" />
          </div>
          <div>
            <label class="text-xs text-gray-600 block mb-1">Format ID *</label>
            <input v-model="createForm.formatId" type="number" class="border rounded px-2 py-1 text-sm w-24" />
          </div>
          <div>
            <label class="text-xs text-gray-600 block mb-1">Player Count</label>
            <input v-model="createForm.playerCount" type="number" class="border rounded px-2 py-1 text-sm w-24" />
          </div>
          <div>
            <label class="text-xs text-gray-600 block mb-1">Source Type</label>
            <input v-model="createForm.sourceType" class="w-full border rounded px-2 py-1 text-sm" placeholder="Manual / Melee.gg / ..." />
          </div>
          <div>
            <label class="text-xs text-gray-600 block mb-1">Source URL</label>
            <input v-model="createForm.sourceUrl" class="w-full border rounded px-2 py-1 text-sm" placeholder="https://..." />
          </div>
          <p v-if="createTournamentError" class="text-red-600 text-xs">{{ createTournamentError }}</p>
          <div class="flex gap-2">
            <button
              class="bg-green-600 text-white px-3 py-1.5 rounded text-sm hover:bg-green-700 disabled:opacity-50"
              :disabled="creating || !createForm.name"
              @click="submitCreate"
            >
              {{ creating ? 'Creating…' : 'Create' }}
            </button>
            <button class="text-sm text-gray-500 hover:text-gray-800" @click="showCreateForm = false">Cancel</button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="loadingTournaments" class="text-gray-500">Loading…</div>
    <div v-else-if="tournaments.length === 0" class="text-gray-500">No tournaments yet.</div>

    <div v-for="t in tournaments" :key="t.id" class="mb-4 bg-white rounded shadow">
      <div class="flex items-center justify-between px-4 py-3 cursor-pointer" @click="toggleExpand(t.id)">
        <div>
          <span class="font-semibold">{{ t.name }}</span>
          <span class="ml-3 text-sm text-gray-500">{{ ParseToHumanReadableText(t.date) }}</span>
          <span v-if="t.formatDisplayName" class="ml-2 text-xs text-gray-400">{{ t.formatDisplayName }}</span>
          <span v-if="t.sourceType" class="ml-2 text-xs text-gray-500">[{{ t.sourceType }}]</span>
        </div>
        <div class="flex items-center gap-3">
          <span class="text-xs text-gray-400">{{ expandedId === t.id ? '▲' : '▼' }}</span>
          <button class="text-red-600 text-xs hover:underline" @click.stop="deleteTournament(t.id)">Delete</button>
        </div>
      </div>

      <div v-if="expandedId === t.id" class="border-t px-4 py-4">
        <button
          v-if="addEntrantForId !== t.id"
          class="mb-3 bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700"
          @click="openAddEntrant(t.id)"
        >
          + Add Entrant
        </button>

        <div v-if="addEntrantForId === t.id" class="mb-4 bg-gray-50 rounded p-3 max-w-lg">
          <h3 class="text-sm font-semibold mb-2">Add Entrant</h3>
          <div class="grid grid-cols-2 gap-2 text-xs">
            <div class="col-span-2">
              <label class="text-gray-600 block mb-0.5">Player Name *</label>
              <input v-model="addEntrantForm.playerName" class="w-full border rounded px-2 py-1" />
            </div>
            <div>
              <label class="text-gray-600 block mb-0.5">Placement</label>
              <input v-model="addEntrantForm.placement" type="number" class="w-full border rounded px-2 py-1" />
            </div>
            <div>
              <label class="text-gray-600 block mb-0.5">Deck ID</label>
              <input v-model="addEntrantForm.deckId" type="number" class="w-full border rounded px-2 py-1" />
            </div>
          </div>
          <p v-if="addEntrantError" class="text-red-600 text-xs mt-1">{{ addEntrantError }}</p>
          <div class="flex gap-2 mt-2">
            <button
              class="bg-blue-600 text-white px-3 py-1 rounded text-xs hover:bg-blue-700 disabled:opacity-50"
              :disabled="addingEntrant || !addEntrantForm.playerName"
              @click="submitAddEntrant"
            >
              {{ addingEntrant ? 'Adding…' : 'Add' }}
            </button>
            <button class="text-xs text-gray-500 hover:text-gray-800" @click="addEntrantForId = null">Cancel</button>
          </div>
        </div>

        <div v-if="t.entrants.length === 0 && addEntrantForId !== t.id" class="text-sm text-gray-400 mb-2">No entrants yet.</div>
        <div v-else class="space-y-4">
          <div
            v-for="entrant in [...t.entrants].sort((a, b) => (a.placement ?? 9999) - (b.placement ?? 9999))"
            :key="entrant.id"
            class="border rounded p-3"
          >
            <div class="flex flex-wrap items-center justify-between gap-2 mb-2">
              <div class="text-sm">
                <span class="font-semibold">#{{ entrant.placement ?? '—' }} {{ entrant.playerName }}</span>
                <span class="ml-3 text-gray-600">W/L/D: {{ entrant.wins ?? 0 }}/{{ entrant.losses ?? 0 }}/{{ entrant.draws ?? 0 }}</span>
                <a v-if="entrant.deckId" :href="`/decks/${entrant.deckId}`" class="ml-3 text-blue-600 hover:underline" target="_blank">Deck #{{ entrant.deckId }}</a>
              </div>
              <div class="flex gap-2 text-xs">
                <button class="text-blue-600 hover:underline" @click="openEditEntrant(entrant)">Edit</button>
                <button class="text-purple-600 hover:underline" @click="openDeckImport(entrant.id)">Import Deck</button>
                <button class="text-red-600 hover:underline" @click="deleteEntrant(entrant.id)">Delete</button>
              </div>
            </div>

            <div v-if="editEntrantId === entrant.id" class="bg-gray-50 rounded p-3 mb-3 max-w-lg">
              <h3 class="text-sm font-semibold mb-2">Edit Entrant</h3>
              <div class="grid grid-cols-2 gap-2 text-xs">
                <div class="col-span-2">
                  <label class="text-gray-600 block mb-0.5">Player Name</label>
                  <input v-model="editEntrantForm.playerName" class="w-full border rounded px-2 py-1" />
                </div>
                <div>
                  <label class="text-gray-600 block mb-0.5">Placement</label>
                  <input v-model="editEntrantForm.placement" type="number" class="w-full border rounded px-2 py-1" />
                </div>
                <div>
                  <label class="text-gray-600 block mb-0.5">Deck ID</label>
                  <input v-model="editEntrantForm.deckId" type="number" class="w-full border rounded px-2 py-1" />
                </div>
              </div>
              <p v-if="editEntrantError" class="text-red-600 text-xs mt-1">{{ editEntrantError }}</p>
              <div class="flex gap-2 mt-2">
                <button
                  class="bg-blue-600 text-white px-3 py-1 rounded text-xs hover:bg-blue-700 disabled:opacity-50"
                  :disabled="editingEntrant"
                  @click="submitEditEntrant"
                >
                  {{ editingEntrant ? 'Saving…' : 'Save' }}
                </button>
                <button class="text-xs text-gray-500 hover:text-gray-800" @click="editEntrantId = null">Cancel</button>
              </div>
            </div>

            <div class="overflow-x-auto">
              <table class="w-full text-xs border-collapse mb-2">
                <thead>
                  <tr class="bg-gray-100 text-left">
                    <th class="px-2 py-1 border border-gray-200">Round</th>
                    <th class="px-2 py-1 border border-gray-200">Opponent</th>
                    <th class="px-2 py-1 border border-gray-200 text-center">W</th>
                    <th class="px-2 py-1 border border-gray-200 text-center">L</th>
                    <th class="px-2 py-1 border border-gray-200 text-center">D</th>
                    <th class="px-2 py-1 border border-gray-200">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="!entrant.matches || entrant.matches.length === 0">
                    <td colspan="6" class="px-2 py-1 border border-gray-200 text-gray-400">No matches yet.</td>
                  </tr>
                  <tr v-for="m in entrant.matches ?? []" :key="m.id" class="hover:bg-gray-50">
                    <td class="px-2 py-1 border border-gray-200">{{ m.roundNumber ?? '—' }}</td>
                    <td class="px-2 py-1 border border-gray-200">{{ m.opponentName ?? '—' }}</td>
                    <td class="px-2 py-1 border border-gray-200 text-center">{{ m.wins }}</td>
                    <td class="px-2 py-1 border border-gray-200 text-center">{{ m.losses }}</td>
                    <td class="px-2 py-1 border border-gray-200 text-center">{{ m.draws }}</td>
                    <td class="px-2 py-1 border border-gray-200">
                      <button class="text-blue-600 hover:underline mr-2" @click="editMatch(m)">Edit</button>
                      <button class="text-red-600 hover:underline" @click="deleteMatch(m.id)">Delete</button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div class="bg-gray-50 rounded p-2 text-xs">
              <div class="font-semibold mb-1">Add Match / Round</div>
              <div class="grid md:grid-cols-6 gap-2 items-end">
                <div>
                  <label class="text-gray-600 block mb-0.5">Round</label>
                  <input v-model="getMatchForm(entrant.id).roundNumber" type="number" class="w-full border rounded px-2 py-1" />
                </div>
                <div class="md:col-span-2">
                  <label class="text-gray-600 block mb-0.5">Opponent</label>
                  <input v-model="getMatchForm(entrant.id).opponentName" class="w-full border rounded px-2 py-1" />
                </div>
                <div>
                  <label class="text-gray-600 block mb-0.5">W</label>
                  <input v-model="getMatchForm(entrant.id).wins" type="number" min="0" class="w-full border rounded px-2 py-1" />
                </div>
                <div>
                  <label class="text-gray-600 block mb-0.5">L</label>
                  <input v-model="getMatchForm(entrant.id).losses" type="number" min="0" class="w-full border rounded px-2 py-1" />
                </div>
                <div>
                  <label class="text-gray-600 block mb-0.5">D</label>
                  <input v-model="getMatchForm(entrant.id).draws" type="number" min="0" class="w-full border rounded px-2 py-1" />
                </div>
              </div>
              <p v-if="matchError[entrant.id]" class="text-red-600 mt-1">{{ matchError[entrant.id] }}</p>
              <button
                class="bg-blue-600 text-white px-3 py-1 rounded mt-2 hover:bg-blue-700 disabled:opacity-50"
                :disabled="addingMatch[entrant.id]"
                @click="addMatch(entrant.id)"
              >
                {{ addingMatch[entrant.id] ? 'Adding…' : 'Add Match' }}
              </button>
            </div>

            <div v-if="deckImportEntrantId === entrant.id" class="bg-yellow-50 border border-yellow-200 rounded p-3 mt-3">
              <h3 class="text-sm font-semibold mb-2">Import Deck from Text</h3>
              <textarea
                v-model="deckImportText"
                rows="10"
                class="w-full border rounded px-2 py-1 text-xs font-mono"
                placeholder="MainDeck&#10;3 The Axe Forgets&#10;..."
              />
              <div class="flex gap-2 mt-2 mb-3">
                <button
                  class="bg-yellow-600 text-white px-3 py-1 rounded text-xs hover:bg-yellow-700 disabled:opacity-50"
                  :disabled="deckImportLoading || !deckImportText.trim()"
                  @click="parseDeckText"
                >
                  {{ deckImportLoading ? 'Parsing…' : 'Parse' }}
                </button>
                <button class="text-xs text-gray-500 hover:text-gray-800" @click="deckImportEntrantId = null">Cancel</button>
              </div>
              <p v-if="deckImportError" class="text-red-600 text-xs mb-2">{{ deckImportError }}</p>

              <div v-if="deckImportResult" class="text-xs">
                <div class="mb-2 font-semibold text-green-700">Matched: {{ deckImportResult.matched.length }}</div>
                <table class="w-full border-collapse mb-2">
                  <thead>
                    <tr class="bg-green-50 text-left">
                      <th class="px-2 py-1 border border-green-200">Section</th>
                      <th class="px-2 py-1 border border-green-200">Qty</th>
                      <th class="px-2 py-1 border border-green-200">Card Name</th>
                      <th class="px-2 py-1 border border-green-200">Card ID</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(card, i) in deckImportResult.matched" :key="i">
                      <td class="px-2 py-1 border border-green-200">{{ sectionLabels[card.section] ?? card.section }}</td>
                      <td class="px-2 py-1 border border-green-200">{{ card.amount }}</td>
                      <td class="px-2 py-1 border border-green-200">{{ card.cardName }}</td>
                      <td class="px-2 py-1 border border-green-200">{{ card.cardId }}</td>
                    </tr>
                  </tbody>
                </table>

                <div v-if="deckImportResult.unmatched.length > 0" class="mb-2">
                  <div class="font-semibold text-red-700">Unmatched: {{ deckImportResult.unmatched.length }}</div>
                  <div v-for="(card, i) in deckImportResult.unmatched" :key="`u-${i}`" class="text-red-700">
                    - {{ sectionLabels[card.section] ?? card.section }} / {{ card.amount }}x {{ card.cardName }}
                  </div>
                </div>

                <div class="mt-3 flex items-center gap-2">
                  <label>Link Deck ID:</label>
                  <input v-model="deckImportDeckId" type="number" class="border rounded px-2 py-1 w-24" />
                  <button class="bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700" :disabled="!deckImportDeckId" @click="applyDeckId">Apply</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
