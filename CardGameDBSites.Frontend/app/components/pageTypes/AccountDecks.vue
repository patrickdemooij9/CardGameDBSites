<script setup lang="ts">
import { PhPlus, PhFolder, PhTray, PhCheckSquare } from "@phosphor-icons/vue";
import type { AccountDecksContentModel } from "~/api/umbraco";
import type { DeckFolderApiModel } from "~/api/default";
import BaseDeckOverview from "../overviews/BaseDeckOverview.vue";
import DeckCardCollection from "../cards/deckCards/DeckCardCollection.vue";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";
import Modal from "../shared/Modal.vue";
import AccountDeckFolderSidebar from "./accountDecks/AccountDeckFolderSidebar.vue";
import type { FolderScope } from "./accountDecks/FolderScope";
import DeckService from "~/services/DeckService";
import DeckFolderService from "~/services/DeckFolderService";
import { useAccountStore } from "~/stores/AccountStore";
import { useSite } from "~/composables/useSite";
import { useAppToast } from "~/composables/useAppToast";
import { getEditDeckUrl } from "~/helpers/DeckUrlHelper";

defineProps<{
  content: AccountDecksContentModel;
}>();

const router = useRouter();
const toast = useAppToast();
const deckService = new DeckService();
const folderService = new DeckFolderService();

const memberId = ref<number | undefined>(undefined);
const isLoading = ref(true);
const siteSettings = await useSite().getSettings();
const createDeckUrl = siteSettings.createDeckUrl ?? "/create-deck";

// Bumped to force the deck overview to refetch after a delete/move (the query itself is unchanged).
const refreshKey = ref(0);

const folders = ref<DeckFolderApiModel[]>([]);

const scope = ref<FolderScope>({ type: "all" });

const scopeFolderId = computed(() =>
  scope.value.type === "folder" ? scope.value.folderId : null,
);
const scopeUnfiled = computed(() => scope.value.type === "unfiled");

async function loadFolders() {
  if (!memberId.value) return;
  folders.value = await folderService.getByUser();
}

function selectScope(next: FolderScope) {
  scope.value = next;
  exitSelectionMode();
  // Page reset is handled inside BaseDeckOverview when the folder scope changes.
}

function refreshDecks() {
  refreshKey.value++;
}

// ---- Selection ------------------------------------------------------------
const selectionMode = ref(false);
const selectedDeckIds = ref<number[]>([]);

function toggleSelectionMode() {
  selectionMode.value = !selectionMode.value;
  if (!selectionMode.value) selectedDeckIds.value = [];
}

function exitSelectionMode() {
  selectionMode.value = false;
  selectedDeckIds.value = [];
}

function onToggleSelect(deckId: number) {
  const i = selectedDeckIds.value.indexOf(deckId);
  if (i >= 0) selectedDeckIds.value.splice(i, 1);
  else selectedDeckIds.value.push(deckId);
}

// ---- Deck actions ---------------------------------------------------------
function onEdit(deckId: number) {
  router.push(getEditDeckUrl(siteSettings.createDeckUrl, deckId));
}

async function onDelete(deckId: number) {
  if (!confirm("Are you sure you want to delete this deck?")) return;
  await deckService.deleteDeck(deckId);
  toast.success("Deck deleted");
  refreshDecks();
  await loadFolders();
}

// ---- Move to folder -------------------------------------------------------
const showMoveModal = ref(false);
// Deck ids currently being moved (single deck, or the current multi-selection).
const decksToMove = ref<number[]>([]);

function onMoveSingle(deckId: number) {
  decksToMove.value = [deckId];
  showMoveModal.value = true;
}

function onMoveSelected() {
  if (selectedDeckIds.value.length === 0) return;
  decksToMove.value = [...selectedDeckIds.value];
  showMoveModal.value = true;
}

async function moveTo(folderId: number | null) {
  await folderService.moveDecks(folderId, decksToMove.value);
  showMoveModal.value = false;
  toast.success(folderId === null ? "Removed from folder" : "Moved to folder");
  exitSelectionMode();
  refreshDecks();
  await loadFolders();
}

// ---- Folder create / rename / delete --------------------------------------
const showFolderModal = ref(false);
const folderModalMode = ref<"create" | "rename">("create");
const folderModalName = ref("");
const folderModalTargetId = ref<number | null>(null);
const folderModalSaving = ref(false);

function openCreateFolder() {
  folderModalMode.value = "create";
  folderModalName.value = "";
  folderModalTargetId.value = null;
  showFolderModal.value = true;
}

function openRenameFolder(folder: DeckFolderApiModel) {
  folderModalMode.value = "rename";
  folderModalName.value = folder.name ?? "";
  folderModalTargetId.value = folder.id ?? null;
  showFolderModal.value = true;
}

async function saveFolder() {
  const name = folderModalName.value.trim();
  if (!name) return;

  folderModalSaving.value = true;
  try {
    if (folderModalMode.value === "create") {
      await folderService.create(name);
      toast.success("Folder created");
    } else if (folderModalTargetId.value != null) {
      await folderService.update(folderModalTargetId.value, name);
      toast.success("Folder renamed");
    }
    showFolderModal.value = false;
    await loadFolders();
  } finally {
    folderModalSaving.value = false;
  }
}

async function deleteFolder(folder: DeckFolderApiModel) {
  if (folder.id == null) return;
  if (
    !confirm(
      `Delete folder "${folder.name}"? The decks inside will be kept and moved to "Unfiled".`,
    )
  )
    return;

  await folderService.delete(folder.id);
  toast.success("Folder deleted");
  if (scope.value.type === "folder" && scope.value.folderId === folder.id) {
    selectScope({ type: "all" });
  }
  await loadFolders();
  refreshDecks();
}

// ---- Init -----------------------------------------------------------------
onMounted(async () => {
  const isLoggedIn = await useAccountStore().checkLogin();
  if (!isLoggedIn) {
    router.push("/login");
    return;
  }

  memberId.value = useAccountStore().member?.id;
  await loadFolders();
  isLoading.value = false;
});
</script>

<template>
  <div v-if="isLoading" class="flex justify-center items-center py-12">
    <div
      class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"
    ></div>
  </div>
  <div v-else-if="memberId" class="container px-4 pt-8 md:px-8 mb-6">
    <div class="flex items-center justify-between mb-6 gap-4 flex-wrap">
      <h1 class="text-2xl md:text-3xl font-extrabold">Your decks</h1>
    </div>

    <div class="flex flex-col md:flex-row gap-6">
      <AccountDeckFolderSidebar
        :folders="folders"
        :active-scope="scope"
        @select="selectScope"
        @create="openCreateFolder"
        @rename="openRenameFolder"
        @delete="deleteFolder"
      />

      <!-- Decks -->
      <section class="flex-1 min-w-0">
        <!-- Selection toolbar -->
        <div class="mb-4 flex items-center gap-3 flex-wrap">
          <Button :button-type="ButtonType.Outline" @click="toggleSelectionMode">
            <PhCheckSquare :size="18" />
            {{ selectionMode ? "Cancel selection" : "Select decks" }}
          </Button>

          <template v-if="selectionMode">
            <span class="text-sm text-gray-600">
              {{ selectedDeckIds.length }} selected
            </span>
            <Button
              :button-type="ButtonType.Success"
              :disabled="selectedDeckIds.length === 0"
              @click="onMoveSelected"
            >
              Move to folder
            </Button>
          </template>
        </div>

        <BaseDeckOverview
          :decks-per-row="3"
          :user-id="memberId"
          :show-format-filter="true"
          :folder-id="scopeFolderId"
          :unfiled="scopeUnfiled"
          :refresh-key="refreshKey"
        >
          <template #default="{ decks }">
            <div
              v-if="!decks || (decks.items?.length ?? 0) === 0"
              class="text-center py-12 text-gray-600"
            >
              <p v-if="scope.type === 'folder'">
                This folder is empty. Use "Select decks" to move decks here.
              </p>
              <p v-else-if="scope.type === 'unfiled'">
                You have no unfiled decks.
              </p>
              <template v-else>
                <p>You have no decks yet. Create one to get started!</p>
                <router-link :to="createDeckUrl" class="btn btn-primary"
                  >Create deck</router-link
                >
              </template>
            </div>
            <DeckCardCollection
              v-else
              :decks="decks.items ?? []"
              :decks-per-row="3"
              :show-owner-actions="true"
              :selection-mode="selectionMode"
              :selected-deck-ids="selectedDeckIds"
              @edit="onEdit"
              @delete="onDelete"
              @move="onMoveSingle"
              @toggle-select="onToggleSelect"
            />
          </template>
        </BaseDeckOverview>
      </section>
    </div>
  </div>

  <!-- Create / rename folder modal (sits above the move modal when both are open). -->
  <Modal
    v-if="showFolderModal"
    :title="folderModalMode === 'create' ? 'New folder' : 'Rename folder'"
    z-class="z-[60]"
    @close="showFolderModal = false"
  >
    <input
      v-model="folderModalName"
      type="text"
      placeholder="Folder name"
      class="w-full border rounded px-3 py-2 mb-4"
      @keyup.enter="saveFolder"
    />
    <div class="flex justify-end gap-3">
      <Button :button-type="ButtonType.Outline" @click="showFolderModal = false">
        Cancel
      </Button>
      <Button
        :button-type="ButtonType.Success"
        :disabled="!folderModalName.trim()"
        :loading="folderModalSaving"
        @click="saveFolder"
      >
        Save
      </Button>
    </div>
  </Modal>

  <!-- Move to folder modal -->
  <Modal
    v-if="showMoveModal"
    :title="`Move ${decksToMove.length} ${decksToMove.length === 1 ? 'deck' : 'decks'} to…`"
    @close="showMoveModal = false"
  >
    <div class="flex flex-col gap-1 max-h-72 overflow-auto">
      <button
        v-for="folder in folders"
        :key="folder.id"
        type="button"
        class="flex items-center gap-2 rounded px-3 py-2 text-left hover:bg-gray-100"
        @click="moveTo(folder.id!)"
      >
        <PhFolder :size="18" /> {{ folder.name }}
      </button>
      <button
        type="button"
        class="flex items-center gap-2 rounded px-3 py-2 text-left hover:bg-gray-100 text-gray-600"
        @click="moveTo(null)"
      >
        <PhTray :size="18" /> Remove from folder
      </button>
    </div>
    <div class="mt-4 border-t pt-4">
      <Button :button-type="ButtonType.Outline" @click="openCreateFolder">
        <PhPlus :size="16" /> New folder
      </Button>
    </div>
  </Modal>
</template>
