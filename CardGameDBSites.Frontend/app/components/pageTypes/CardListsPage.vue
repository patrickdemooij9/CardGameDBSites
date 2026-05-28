<script setup lang="ts">
import { useCardLists } from "~/composables/useCardLists";
import { useAccountStore } from "~/stores/AccountStore";
import type { CardListModel } from "~/models/CardListModel";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";
import { PhTrash, PhPencil, PhEye, PhEyeSlash } from "@phosphor-icons/vue";

const router = useRouter();
const cardLists = useCardLists();
const isLoading = ref(true);
const lists = ref<CardListModel[]>([]);

const isCreating = ref(false);
const newListName = ref("");
const newListDescription = ref("");
const newListIsPublic = ref(false);

const editingListId = ref<number | null>(null);
const editName = ref("");
const editDescription = ref("");
const editIsPublic = ref(false);

onMounted(async () => {
  const isLoggedIn = await useAccountStore().checkLogin();
  if (!isLoggedIn) {
    router.push("/login");
    return;
  }
  lists.value = await cardLists.loadLists();
  isLoading.value = false;
});

async function createList() {
  if (!newListName.value.trim()) return;

  await cardLists.createList({
    name: newListName.value.trim(),
    description: newListDescription.value.trim() || null,
    isPublic: newListIsPublic.value,
  });
  lists.value = await cardLists.loadLists();
  isCreating.value = false;
  newListName.value = "";
  newListDescription.value = "";
  newListIsPublic.value = false;
}

function startEdit(list: CardListModel) {
  editingListId.value = list.id;
  editName.value = list.name;
  editDescription.value = list.description ?? "";
  editIsPublic.value = list.isPublic;
}

async function saveEdit() {
  if (!editingListId.value || !editName.value.trim()) return;

  await cardLists.updateList(editingListId.value, {
    name: editName.value.trim(),
    description: editDescription.value.trim() || null,
    isPublic: editIsPublic.value,
  });
  lists.value = await cardLists.loadLists();
  editingListId.value = null;
}

async function deleteList(id: number) {
  if (!confirm("Are you sure you want to delete this list?")) return;

  await cardLists.deleteList(id);
  lists.value = lists.value.filter((l) => l.id !== id);
}
</script>

<template>
  <div v-if="isLoading" class="flex justify-center items-center py-12">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
  </div>
  <div v-else class="container px-4 pt-8 md:px-8 mb-6">
    <div class="flex justify-between items-center mb-6">
      <h1>My Card Lists</h1>
      <Button :button-type="ButtonType.Primary" @click="isCreating = true" v-if="!isCreating">
        + New List
      </Button>
    </div>

    <!-- Create new list form -->
    <div v-if="isCreating" class="border rounded p-4 mb-6 bg-white">
      <h2 class="text-lg font-semibold mb-4">Create New List</h2>
      <div class="flex flex-col gap-3">
        <div>
          <label class="block text-sm font-medium mb-1">Name *</label>
          <input
            v-model="newListName"
            type="text"
            class="w-full px-3 py-2 rounded border border-gray-300"
            placeholder="e.g. Trade, Wishlist, Showcase"
            @keyup.enter="createList"
          />
        </div>
        <div>
          <label class="block text-sm font-medium mb-1">Description</label>
          <textarea
            v-model="newListDescription"
            class="w-full px-3 py-2 rounded border border-gray-300"
            placeholder="Optional description"
            rows="2"
          ></textarea>
        </div>
        <div class="flex items-center gap-2">
          <input type="checkbox" id="newListPublic" v-model="newListIsPublic" />
          <label for="newListPublic" class="text-sm">Make this list public</label>
        </div>
        <div class="flex gap-2">
          <Button :button-type="ButtonType.Primary" @click="createList" :disabled="!newListName.trim()">
            Create
          </Button>
          <Button :button-type="ButtonType.Outline" @click="isCreating = false">
            Cancel
          </Button>
        </div>
      </div>
    </div>

    <!-- Lists grid -->
    <div v-if="lists.length === 0" class="text-center py-8">
      <p class="text-gray-500">You have no card lists yet. Create one to get started!</p>
    </div>
    <div v-else class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      <div v-for="list in lists" :key="list.id" class="border rounded p-4 bg-white hover:shadow-md transition-shadow">
        <!-- Edit mode -->
        <div v-if="editingListId === list.id" class="flex flex-col gap-2">
          <input
            v-model="editName"
            type="text"
            class="px-3 py-2 rounded border border-gray-300"
          />
          <textarea
            v-model="editDescription"
            class="px-3 py-2 rounded border border-gray-300"
            rows="2"
          ></textarea>
          <div class="flex items-center gap-2">
            <input type="checkbox" :id="`editPublic-${list.id}`" v-model="editIsPublic" />
            <label :for="`editPublic-${list.id}`" class="text-sm">Public</label>
          </div>
          <div class="flex gap-2">
            <Button :button-type="ButtonType.Primary" @click="saveEdit">Save</Button>
            <Button :button-type="ButtonType.Outline" @click="editingListId = null">Cancel</Button>
          </div>
        </div>

        <!-- Display mode -->
        <div v-else>
          <div class="flex justify-between items-start mb-2">
            <h3 class="text-lg font-semibold cursor-pointer hover:text-blue-600" @click="router.push(`/card-lists/${list.id}`)">
              {{ list.name }}
            </h3>
            <div class="flex items-center gap-1">
              <PhEye v-if="list.isPublic" class="w-4 h-4 text-green-600" title="Public" />
              <PhEyeSlash v-else class="w-4 h-4 text-gray-400" title="Private" />
            </div>
          </div>
          <p v-if="list.description" class="text-sm text-gray-600 mb-2">{{ list.description }}</p>
          <p class="text-sm text-gray-500 mb-3">{{ list.itemCount }} cards</p>
          <div class="flex gap-2">
            <Button :button-type="ButtonType.Outline" @click="router.push(`/card-lists/${list.id}`)">
              View
            </Button>
            <button class="p-2 text-gray-500 hover:text-blue-600" @click="startEdit(list)">
              <PhPencil class="w-4 h-4" />
            </button>
            <button class="p-2 text-gray-500 hover:text-red-600" @click="deleteList(list.id)">
              <PhTrash class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
