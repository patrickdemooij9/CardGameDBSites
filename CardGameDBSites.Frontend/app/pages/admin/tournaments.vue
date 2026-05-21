<script setup lang="ts">
import { useAccountStore } from '~/stores/AccountStore';
import TournamentService, {
    type TournamentEvent,
    type TournamentEntrant,
    type CreateTournamentDto,
    type AddEntrantDto,
    type UpdateEntrantDto,
    type ParsedDeckCard,
    type UnmatchedDeckCard,
} from '~/services/TournamentService';
import { ParseToHumanReadableText } from '~/helpers/DateHelper';

useHead({ title: 'Admin – Tournaments' });

const createError = ref('');

const accountStore = useAccountStore();
await accountStore.checkLogin();

if (!accountStore.member?.isAdmin) {
    // throw createError({ statusCode: 403, statusMessage: 'Forbidden' });
}

const svc = new TournamentService();

// ── Tournaments list ────────────────────────────────────────────────────────
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

async function deleteTournament(id: string) {
    if (!confirm('Delete this tournament and all its entrants?')) return;
    await svc.deleteTournament(id);
    tournaments.value = tournaments.value.filter(t => t.id !== id);
    if (expandedId.value === id) expandedId.value = null;
}

function toggleExpand(id: string) {
    expandedId.value = expandedId.value === id ? null : id;
}

// ── Create tournament form ──────────────────────────────────────────────────
const showCreateForm = ref(false);
const createForm = ref<CreateTournamentDto>({
    name: '',
    date: new Date().toISOString().slice(0, 10),
    formatId: 1,
    playerCount: null,
    sourceUrl: null,
});
const creating = ref(false);

async function submitCreate() {
    createError.value = '';
    creating.value = true;
    try {
        const dto: CreateTournamentDto = {
            name: createForm.value.name,
            date: new Date(createForm.value.date).toISOString(),
            formatId: Number(createForm.value.formatId),
            playerCount: createForm.value.playerCount ? Number(createForm.value.playerCount) : null,
            sourceUrl: createForm.value.sourceUrl || null,
        };
        await svc.create(dto);
        showCreateForm.value = false;
        createForm.value = { name: '', date: new Date().toISOString().slice(0, 10), formatId: 1, playerCount: null, sourceUrl: null };
        await loadTournaments();
    } catch (e: any) {
        createError.value = e?.data?.error ?? 'Failed to create tournament.';
    } finally {
        creating.value = false;
    }
}

// ── Add entrant ─────────────────────────────────────────────────────────────
const addEntrantForId = ref<string | null>(null);
const addEntrantForm = ref<AddEntrantDto>({
    playerName: '',
    placement: null,
    wins: null,
    losses: null,
    draws: null,
    deckId: null,
});
const addingEntrant = ref(false);
const addEntrantError = ref('');

function openAddEntrant(tournamentId: string) {
    addEntrantForId.value = tournamentId;
    addEntrantForm.value = { playerName: '', placement: null, wins: null, losses: null, draws: null, deckId: null };
    addEntrantError.value = '';
}

async function submitAddEntrant() {
    if (!addEntrantForId.value) return;
    addEntrantError.value = '';
    addingEntrant.value = true;
    try {
        const dto: AddEntrantDto = {
            playerName: addEntrantForm.value.playerName,
            placement: addEntrantForm.value.placement ? Number(addEntrantForm.value.placement) : null,
            wins: addEntrantForm.value.wins !== null && addEntrantForm.value.wins !== undefined ? Number(addEntrantForm.value.wins) : null,
            losses: addEntrantForm.value.losses !== null && addEntrantForm.value.losses !== undefined ? Number(addEntrantForm.value.losses) : null,
            draws: addEntrantForm.value.draws !== null && addEntrantForm.value.draws !== undefined ? Number(addEntrantForm.value.draws) : null,
            deckId: addEntrantForm.value.deckId ? Number(addEntrantForm.value.deckId) : null,
        };
        await svc.addEntrant(addEntrantForId.value, dto);
        addEntrantForId.value = null;
        await loadTournaments();
    } catch (e: any) {
        addEntrantError.value = e?.data?.error ?? 'Failed to add entrant.';
    } finally {
        addingEntrant.value = false;
    }
}

// ── Edit entrant ────────────────────────────────────────────────────────────
const editEntrantId = ref<string | null>(null);
const editEntrantForm = ref<UpdateEntrantDto>({});
const editingEntrant = ref(false);
const editEntrantError = ref('');

function openEditEntrant(entrant: TournamentEntrant) {
    editEntrantId.value = entrant.id;
    editEntrantForm.value = {
        playerName: entrant.playerName,
        placement: entrant.placement,
        wins: entrant.wins,
        losses: entrant.losses,
        draws: entrant.draws,
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
            placement: editEntrantForm.value.placement !== null && editEntrantForm.value.placement !== undefined ? Number(editEntrantForm.value.placement) : null,
            wins: editEntrantForm.value.wins !== null && editEntrantForm.value.wins !== undefined ? Number(editEntrantForm.value.wins) : null,
            losses: editEntrantForm.value.losses !== null && editEntrantForm.value.losses !== undefined ? Number(editEntrantForm.value.losses) : null,
            draws: editEntrantForm.value.draws !== null && editEntrantForm.value.draws !== undefined ? Number(editEntrantForm.value.draws) : null,
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
    if (!confirm('Delete this entrant?')) return;
    await svc.deleteEntrant(entrantId);
    await loadTournaments();
}

// ── Deck text import ────────────────────────────────────────────────────────
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

        <!-- Create tournament button / form -->
        <div class="mb-6">
            <button
                v-if="!showCreateForm"
                class="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 text-sm font-medium"
                @click="showCreateForm = true"
            >
                + New Tournament
            </button>

            <div v-else class="bg-white rounded p-4 shadow max-w-lg">
                <h2 class="font-semibold mb-3">Create Tournament</h2>
                <div class="space-y-3">
                    <div>
                        <label class="text-xs text-gray-600 block mb-1">Name *</label>
                        <input v-model="createForm.name" class="w-full border rounded px-2 py-1 text-sm" placeholder="Tournament name" />
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
                        <label class="text-xs text-gray-600 block mb-1">Source URL</label>
                        <input v-model="createForm.sourceUrl" class="w-full border rounded px-2 py-1 text-sm" placeholder="https://..." />
                    </div>
                    <p v-if="createError" class="text-red-600 text-xs">{{ createError }}</p>
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

        <!-- Tournaments list -->
        <div v-if="loadingTournaments" class="text-gray-500">Loading…</div>
        <div v-else-if="tournaments.length === 0" class="text-gray-500">No tournaments yet.</div>

        <div v-for="t in tournaments" :key="t.id" class="mb-4 bg-white rounded shadow">
            <!-- Tournament header row -->
            <div class="flex items-center justify-between px-4 py-3 cursor-pointer" @click="toggleExpand(t.id)">
                <div>
                    <span class="font-semibold">{{ t.name }}</span>
                    <span class="ml-3 text-sm text-gray-500">{{ ParseToHumanReadableText(t.date) }}</span>
                    <span v-if="t.formatDisplayName" class="ml-2 text-xs text-gray-400">{{ t.formatDisplayName }}</span>
                    <span v-if="t.playerCount" class="ml-2 text-xs text-gray-400">{{ t.playerCount }} players</span>
                </div>
                <div class="flex items-center gap-3">
                    <span class="text-xs text-gray-400">{{ expandedId === t.id ? '▲' : '▼' }}</span>
                    <button
                        class="text-red-600 text-xs hover:underline"
                        @click.stop="deleteTournament(t.id)"
                    >
                        Delete
                    </button>
                </div>
            </div>

            <!-- Expanded: entrant management -->
            <div v-if="expandedId === t.id" class="border-t px-4 py-4">
                <!-- Add entrant form trigger -->
                <button
                    v-if="addEntrantForId !== t.id"
                    class="mb-3 bg-blue-600 text-white px-3 py-1.5 rounded text-sm hover:bg-blue-700"
                    @click="openAddEntrant(t.id)"
                >
                    + Add Entrant
                </button>

                <!-- Add entrant inline form -->
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
                        <div>
                            <label class="text-gray-600 block mb-0.5">Wins</label>
                            <input v-model="addEntrantForm.wins" type="number" class="w-full border rounded px-2 py-1" />
                        </div>
                        <div>
                            <label class="text-gray-600 block mb-0.5">Losses</label>
                            <input v-model="addEntrantForm.losses" type="number" class="w-full border rounded px-2 py-1" />
                        </div>
                        <div>
                            <label class="text-gray-600 block mb-0.5">Draws</label>
                            <input v-model="addEntrantForm.draws" type="number" class="w-full border rounded px-2 py-1" />
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

                <!-- Entrants table -->
                <div v-if="t.entrants.length === 0 && addEntrantForId !== t.id" class="text-sm text-gray-400 mb-2">No entrants yet.</div>
                <table v-else-if="t.entrants.length > 0" class="w-full text-xs border-collapse mb-4">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="px-2 py-1.5 border border-gray-200">#</th>
                            <th class="px-2 py-1.5 border border-gray-200">Player</th>
                            <th class="px-2 py-1.5 border border-gray-200 text-center">W</th>
                            <th class="px-2 py-1.5 border border-gray-200 text-center">L</th>
                            <th class="px-2 py-1.5 border border-gray-200 text-center">D</th>
                            <th class="px-2 py-1.5 border border-gray-200">Deck</th>
                            <th class="px-2 py-1.5 border border-gray-200">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr
                            v-for="entrant in [...t.entrants].sort((a, b) => (a.placement ?? 9999) - (b.placement ?? 9999))"
                            :key="entrant.id"
                            class="hover:bg-gray-50"
                        >
                            <td class="px-2 py-1.5 border border-gray-200">{{ entrant.placement ?? '—' }}</td>
                            <td class="px-2 py-1.5 border border-gray-200">{{ entrant.playerName }}</td>
                            <td class="px-2 py-1.5 border border-gray-200 text-center">{{ entrant.wins ?? '—' }}</td>
                            <td class="px-2 py-1.5 border border-gray-200 text-center">{{ entrant.losses ?? '—' }}</td>
                            <td class="px-2 py-1.5 border border-gray-200 text-center">{{ entrant.draws ?? '—' }}</td>
                            <td class="px-2 py-1.5 border border-gray-200">
                                <a v-if="entrant.deckId" :href="`/decks/${entrant.deckId}`" class="text-blue-600 hover:underline" target="_blank">#{{ entrant.deckId }}</a>
                                <span v-else class="text-gray-400">—</span>
                            </td>
                            <td class="px-2 py-1.5 border border-gray-200">
                                <div class="flex gap-2">
                                    <button class="text-blue-600 hover:underline" @click="openEditEntrant(entrant)">Edit</button>
                                    <button class="text-purple-600 hover:underline" @click="openDeckImport(entrant.id)">Import Deck</button>
                                    <button class="text-red-600 hover:underline" @click="deleteEntrant(entrant.id)">Delete</button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>

                <!-- Edit entrant modal -->
                <div v-if="editEntrantId && t.entrants.some(e => e.id === editEntrantId)" class="bg-gray-50 rounded p-3 max-w-lg mb-4">
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
                        <div>
                            <label class="text-gray-600 block mb-0.5">Wins</label>
                            <input v-model="editEntrantForm.wins" type="number" class="w-full border rounded px-2 py-1" />
                        </div>
                        <div>
                            <label class="text-gray-600 block mb-0.5">Losses</label>
                            <input v-model="editEntrantForm.losses" type="number" class="w-full border rounded px-2 py-1" />
                        </div>
                        <div>
                            <label class="text-gray-600 block mb-0.5">Draws</label>
                            <input v-model="editEntrantForm.draws" type="number" class="w-full border rounded px-2 py-1" />
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

                <!-- Deck import modal -->
                <div v-if="deckImportEntrantId && t.entrants.some(e => e.id === deckImportEntrantId)" class="bg-yellow-50 border border-yellow-200 rounded p-3 max-w-2xl mb-4">
                    <h3 class="text-sm font-semibold mb-2">Import Deck from Text</h3>
                    <p class="text-xs text-gray-600 mb-2">
                        Paste a deck list below (MainDeck / Leader / Base / Sideboard sections).
                        Card names will be matched against the card database.
                    </p>

                    <textarea
                        v-model="deckImportText"
                        rows="12"
                        class="w-full border rounded px-2 py-1 text-xs font-mono"
                        placeholder="MainDeck&#10;3 The Axe Forgets&#10;3 Rose Tico | Now It's Worth It&#10;...&#10;Leader&#10;1 Lando Calrissian | Full Sabacc&#10;Base&#10;1 Lake Country"
                    />

                    <div class="flex gap-2 mt-2 mb-4">
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

                    <!-- Parse results -->
                    <div v-if="deckImportResult">
                        <div class="mb-3">
                            <h4 class="text-xs font-semibold text-green-700 mb-1">✓ Matched ({{ deckImportResult.matched.length }} lines)</h4>
                            <table class="w-full text-xs border-collapse">
                                <thead>
                                    <tr class="bg-green-50 text-left">
                                        <th class="px-2 py-1 border border-green-200">Section</th>
                                        <th class="px-2 py-1 border border-green-200">Qty</th>
                                        <th class="px-2 py-1 border border-green-200">Card Name</th>
                                        <th class="px-2 py-1 border border-green-200">Card ID</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="(card, i) in deckImportResult.matched" :key="i" class="hover:bg-green-50">
                                        <td class="px-2 py-1 border border-green-200 text-gray-500">{{ sectionLabels[card.section] ?? card.section }}</td>
                                        <td class="px-2 py-1 border border-green-200">{{ card.amount }}</td>
                                        <td class="px-2 py-1 border border-green-200">{{ card.cardName }}</td>
                                        <td class="px-2 py-1 border border-green-200 font-mono">{{ card.cardId }}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <div v-if="deckImportResult.unmatched.length > 0" class="mb-3">
                            <h4 class="text-xs font-semibold text-red-700 mb-1">✗ Unmatched ({{ deckImportResult.unmatched.length }} lines)</h4>
                            <table class="w-full text-xs border-collapse">
                                <thead>
                                    <tr class="bg-red-50 text-left">
                                        <th class="px-2 py-1 border border-red-200">Section</th>
                                        <th class="px-2 py-1 border border-red-200">Qty</th>
                                        <th class="px-2 py-1 border border-red-200">Card Name (not found)</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="(card, i) in deckImportResult.unmatched" :key="i" class="hover:bg-red-50">
                                        <td class="px-2 py-1 border border-red-200 text-gray-500">{{ sectionLabels[card.section] ?? card.section }}</td>
                                        <td class="px-2 py-1 border border-red-200">{{ card.amount }}</td>
                                        <td class="px-2 py-1 border border-red-200 text-red-700">{{ card.cardName }}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <!-- Link an existing deck ID -->
                        <div class="mt-3 flex items-center gap-2">
                            <label class="text-xs text-gray-700">Link Deck ID to this entrant:</label>
                            <input v-model="deckImportDeckId" type="number" class="border rounded px-2 py-1 text-xs w-24" placeholder="Deck ID" />
                            <button
                                class="bg-blue-600 text-white px-3 py-1 rounded text-xs hover:bg-blue-700 disabled:opacity-50"
                                :disabled="!deckImportDeckId"
                                @click="applyDeckId"
                            >
                                Apply
                            </button>
                        </div>
                        <p class="text-xs text-gray-500 mt-1">
                            Use the matched card IDs above to build the deck in the <a href="/deck-builder" class="underline text-blue-600" target="_blank">Deck Builder</a>, then paste the resulting deck ID here.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
