<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import PopupBase from "~/components/popups/PopupBase.vue";
import { PopupSize } from "./PopupTypes";
import { useCardLists } from "~/composables/useCardLists";
import { useCards } from "~/composables/useCards";
import type { CardInListModel, CardListModel } from "~/models/CardListModel";

const emit = defineEmits<{
  (e: "close"): void;
}>();

const props = defineProps<{
  card: CardDetailApiModel;
}>();

const cardLists = useCardLists();
const cards = useCards();
const variantTypes = await cards.loadVariantTypes();

const lists = ref<CardListModel[]>([]);
const cardStatus = ref<CardInListModel[]>([]);
const selectedListId = ref<number | null>(null);
const selectedVariantId = ref<number | null>(null);
const amount = ref(1);
const isCreatingNew = ref(false);
const newListName = ref("");
const isSaving = ref(false);

async function loadData() {
  lists.value = await cardLists.loadLists();
  cardStatus.value = await cardLists.getCardStatus(props.card.baseId!);
}

await loadData();

function getVariantName(variantId: number | null | undefined): string {
  if (!variantId) return "Normal";
  const vt = variantTypes.find((v) => v.id === variantId);
  return vt?.displayName ?? "Unknown";
}

function isCardInList(listId: number): boolean {
  return cardStatus.value.some((s) => s.listId === listId);
}

function getCardVariantInList(listId: number, variantId: number | null): CardInListModel | undefined {
  return cardStatus.value.find((s) => s.listId === listId && s.variantId === variantId);
}

async function addToList() {
  if (!selectedListId.value || amount.value < 1) return;

  isSaving.value = true;
  try {
    await cardLists.addItem(selectedListId.value, {
      cardId: props.card.baseId!,
      variantId: selectedVariantId.value,
      amount: amount.value
    });
    cardStatus.value = await cardLists.getCardStatus(props.card.baseId!);
    selectedListId.value = null;
    selectedVariantId.value = null;
    amount.value = 1;
  } finally {
    isSaving.value = false;
  }
}

async function removeFromList(listId: number, itemId: number) {
  await cardLists.removeItem(listId, itemId);
  cardStatus.value = await cardLists.getCardStatus(props.card.baseId!);
}

async function createNewList() {
  if (!newListName.value.trim()) return;

  isSaving.value = true;
  try {
    await cardLists.createList({
      name: newListName.value.trim(),
      isPublic: false
    });
    lists.value = await cardLists.loadLists();
    isCreatingNew.value = false;
    newListName.value = "";
  } finally {
    isSaving.value = false;
  }
}
</script>

<template>
  <PopupBase :size="PopupSize.Medium" @close="emit('close')">
    <h3 class="text-base font-semibold leading-6 text-gray-900">
      Add to list
    </h3>
    <div class="mt-2 mb-2">
      <p class="text-sm">
        Add <span class="font-semibold">{{ card.displayName }}</span> to one of your lists.
      </p>
    </div>

    <!-- Current lists with status indicators -->
    <div class="mt-4 space-y-2">
      <div v-for="list in lists" :key="list.id" class="flex items-center justify-between p-2 border rounded">
        <div class="flex items-center gap-2">
          <span v-if="isCardInList(list.id)" class="text-green-600 font-bold">✓</span>
          <span v-else class="text-gray-300">○</span>
          <span>{{ list.name }}</span>
          <span class="text-xs text-gray-500">({{ list.itemCount }} items)</span>
        </div>
        <button
          v-if="!isCardInList(list.id)"
          class="text-sm text-blue-600 hover:text-blue-800"
          @click="selectedListId = list.id"
        >
          Add
        </button>
      </div>
    </div>

    <!-- Card entries in lists (showing variants) -->
    <div v-if="cardStatus.length > 0" class="mt-4">
      <h4 class="text-sm font-semibold text-gray-700 mb-2">Card in lists:</h4>
      <div v-for="status in cardStatus" :key="status.itemId" class="flex items-center justify-between p-2 bg-gray-50 rounded mb-1">
        <div class="text-sm">
          <span class="font-medium">{{ lists.find(l => l.id === status.listId)?.name }}</span>
          <span class="text-gray-500 ml-2">{{ getVariantName(status.variantId) }} × {{ status.amount }}</span>
        </div>
        <button
          class="text-sm text-red-600 hover:text-red-800"
          @click="removeFromList(status.listId, status.itemId)"
        >
          Remove
        </button>
      </div>
    </div>

    <!-- Add to selected list form -->
    <div v-if="selectedListId" class="mt-4 p-3 bg-blue-50 rounded border border-blue-200">
      <h4 class="text-sm font-semibold mb-2">
        Adding to: {{ lists.find(l => l.id === selectedListId)?.name }}
      </h4>
      <div class="flex flex-col gap-2">
        <div>
          <label class="text-sm font-medium">Variant</label>
          <select
            v-model="selectedVariantId"
            class="w-full px-3 py-2 rounded border border-gray-300 text-sm"
          >
            <option :value="null">Normal</option>
            <option
              v-for="variant in card.variants?.filter(v => v.setId === card.setId)"
              :key="variant.cardVariantId"
              :value="variant.cardVariantId"
            >
              {{ variant.variantTypeId ? getVariantName(variant.variantTypeId) : 'Normal' }}
            </option>
          </select>
        </div>
        <div>
          <label class="text-sm font-medium">Amount</label>
          <input
            type="number"
            v-model="amount"
            min="1"
            class="w-full px-3 py-2 rounded border border-gray-300 text-sm"
          />
        </div>
        <div class="flex gap-2">
          <button
            class="px-3 py-1 bg-blue-600 text-white rounded text-sm hover:bg-blue-700 disabled:opacity-50"
            :disabled="isSaving"
            @click="addToList"
          >
            Add
          </button>
          <button
            class="px-3 py-1 bg-gray-200 text-gray-700 rounded text-sm hover:bg-gray-300"
            @click="selectedListId = null"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>

    <!-- Create new list -->
    <div class="mt-4 pt-4 border-t">
      <button
        v-if="!isCreatingNew"
        class="text-sm text-blue-600 hover:text-blue-800"
        @click="isCreatingNew = true"
      >
        + Create new list
      </button>
      <div v-else class="flex gap-2">
        <input
          v-model="newListName"
          type="text"
          placeholder="List name"
          class="flex-1 px-3 py-2 rounded border border-gray-300 text-sm"
          @keyup.enter="createNewList"
        />
        <button
          class="px-3 py-1 bg-green-600 text-white rounded text-sm hover:bg-green-700 disabled:opacity-50"
          :disabled="isSaving || !newListName.trim()"
          @click="createNewList"
        >
          Create
        </button>
        <button
          class="px-3 py-1 bg-gray-200 text-gray-700 rounded text-sm hover:bg-gray-300"
          @click="isCreatingNew = false; newListName = ''"
        >
          Cancel
        </button>
      </div>
    </div>

    <template #actions>
      <button
        type="button"
        class="inline-flex w-full justify-center rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 sm:ml-3 sm:w-auto"
        @click="emit('close')"
      >
        Done
      </button>
    </template>
  </PopupBase>
</template>
