<template>
  <PopupBase :size="PopupSize.Medium" @close="emit('close')">
    <h3 class="text-lg font-bold mb-2">Add a card pack</h3>
    <p class="text-gray-600 mb-4">Add the number (bottom-left) of your card to each of the inputs and we'll add them to your collection.</p>

    <div v-if="errorMessage" class="text-red-600 mb-4">
      {{ errorMessage }}
    </div>

    <div class="form-field mb-4">
      <label for="setId" class="form-label font-bold">Choose your set</label>
      <select 
        id="setId" 
        v-model="selectedSetId" 
        class="form-input px-1 py-1 w-full border rounded"
      >
        <option v-for="set in sets" :key="set.id" :value="set.id">
          {{ set.displayName }}
        </option>
      </select>
    </div>

    <div class="space-y-2 max-h-64 overflow-y-auto">
      <div v-for="(item, index) in packItems" :key="index" class="flex items-center gap-2">
        <input 
          v-model="item.id" 
          class="form-input w-full px-2 py-1 border rounded" 
          :placeholder="(index + 1).toString().padStart(3, '0')"
        />
        <select 
          v-model="item.variantTypeId" 
          class="form-input px-1 py-1 border rounded"
        >
          <option :value="null">Normal</option>
          <option v-for="variantType in variantTypes" :key="variantType.id" :value="variantType.id">
            {{ variantType.displayName }}
          </option>
        </select>
      </div>
    </div>

    <template #actions>
      <button 
        @click="handleVerify" 
        :disabled="isVerifying"
        class="btn bg-green-400 hover:bg-green-500 text-white"
      >
        {{ isVerifying ? 'Verifying...' : 'Confirm' }}
      </button>
    </template>
  </PopupBase>

  <!-- Verify Modal -->
  <Teleport to="#root">
    <div
      v-if="showVerifyModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60"
    >
      <div class="relative bg-white rounded-lg shadow-lg w-screen mx-4 max-h-screen overflow-auto md:w-[50vw]">
        <button
          class="absolute top-3 right-3 text-gray-400 hover:text-gray-700 transition-colors"
          @click.prevent="showVerifyModal = false"
        >
          <PhX class="h-6 w-6" />
        </button>
        <div class="p-6">
          <h3 class="text-lg font-bold mb-2">Add a card pack</h3>
          <p class="text-gray-600 mb-4">We found the following cards to import</p>
          
          <div class="space-y-1 max-h-64 overflow-y-auto">
            <div 
              v-for="card in verifiedCards" 
              :key="card.baseId"
              class="w-full border py-2 px-3 rounded mb-1"
            >
              {{ card.displayName }} ({{ getVariantName(card.variantTypeId) }})
            </div>
          </div>
        </div>
        <div class="bg-gray-50 px-4 py-3 gap-2 sm:flex sm:flex-row-reverse sm:px-6">
          <button 
            @click="handleAddPack" 
            :disabled="isAdding"
            class="btn bg-green-400 hover:bg-green-500 text-white"
          >
            {{ isAdding ? 'Adding...' : 'Confirm' }}
          </button>
          <button 
            @click="showVerifyModal = false"
            class="btn bg-white hover:bg-gray-50 text-gray-900"
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { PhX } from "@phosphor-icons/vue";
import PopupBase from "./PopupBase.vue";
import { PopupSize } from "./PopupTypes";
import type { SetViewModel, CardVariantTypeApiModel } from "~/api/default";
import type { PackPostApiModel, PackVerifyCardApiModel } from "~/models/PackApiModel";
import { useCollectionStore } from "~/stores/CollectionStore";

const emit = defineEmits<{
  (e: "close"): void;
  (e: "added"): void;
}>();

const collectionStore = useCollectionStore();

const props = defineProps<{
  sets: SetViewModel[];
  variantTypes: CardVariantTypeApiModel[];
}>();

const selectedSetId = ref(props.sets[0]?.id ?? 0);
const packItems = ref<{ id: string; variantTypeId: number | null }[]>(
  Array.from({ length: 16 }, (_, i) => ({ id: "", variantTypeId: null }))
);
const errorMessage = ref("");
const isVerifying = ref(false);
const isAdding = ref(false);

const showVerifyModal = ref(false);
const verifiedCards = ref<PackVerifyCardApiModel[]>([]);

function getVariantName(variantTypeId: number | null | undefined): string {
  if (!variantTypeId) return "Normal";
  return props.variantTypes.find(v => v.id === variantTypeId)?.displayName ?? "Normal";
}

async function handleVerify() {
  errorMessage.value = "";
  isVerifying.value = true;

  try {
    const postModel: PackPostApiModel = {
      setId: selectedSetId.value,
      items: packItems.value.map(item => ({
        id: item.id,
        variantTypeId: item.variantTypeId
      }))
    };

    const result = await collectionStore.verifyPack(postModel);
    
    if ("errorMessage" in result) {
      errorMessage.value = result.errorMessage;
    } else {
      verifiedCards.value = result.cards;
      showVerifyModal.value = true;
    }
  } catch (error: any) {
    errorMessage.value = error.message || "An error occurred during verification";
  } finally {
    isVerifying.value = false;
  }
}

async function handleAddPack() {
  isAdding.value = true;

  try {
    const postModel: PackPostApiModel = {
      setId: selectedSetId.value,
      items: verifiedCards.value.map(card => ({
        id: card.baseId.toString(),
        variantTypeId: card.variantTypeId ?? null
      }))
    };

    await collectionStore.addPack(postModel);
    
    showVerifyModal.value = false;
    emit("added");
    emit("close");
  } catch (error: any) {
    errorMessage.value = error.message || "An error occurred while adding the pack";
    showVerifyModal.value = false;
  } finally {
    isAdding.value = false;
  }
}
</script>
