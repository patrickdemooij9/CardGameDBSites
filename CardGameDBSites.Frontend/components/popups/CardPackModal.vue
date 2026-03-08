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
          :placeholder="(index + 1).toString()"
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
      <Button 
        @click="handleVerify" 
        :disabled="isVerifying"
        :button-type="ButtonType.Success"
        class="btn bg-green-400 hover:bg-green-500 text-white"
      >
        {{ isVerifying ? 'Verifying...' : 'Confirm' }}
      </Button>
    </template>
  </PopupBase>

  <!-- Verify Modal -->
   <PopupBase v-if="showVerifyModal" :size="PopupSize.Medium" @close="showVerifyModal = false">
    <h3 class="text-lg font-bold mb-2">We found the following cards to import</h3>
    
    <div class="space-y-1 max-h-64 overflow-y-auto">
      <div 
        v-for="card in verifiedCards" 
        :key="card.baseId"
        class="w-full border py-2 px-3 rounded mb-1"
      >
        {{ card.displayName }} ({{ getVariantName(card.variantTypeId) }})
      </div>
    </div>

    <template #actions>
      <Button 
        @click="handleAddPack" 
        :disabled="isAdding"
        :button-type="ButtonType.Success"
        class="btn bg-green-400 hover:bg-green-500 text-white"
      >
        {{ isAdding ? 'Adding...' : 'Confirm' }}
      </Button>
    </template>

   </PopupBase>
</template>

<script setup lang="ts">
import PopupBase from "./PopupBase.vue";
import { PopupSize } from "./PopupTypes";
import type { SetViewModel, CardVariantTypeApiModel } from "~/api/default";
import type { PackPostApiModel, PackVerifyCardApiModel } from "~/models/PackApiModel";
import { useCollectionStore } from "~/stores/CollectionStore";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";

const emit = defineEmits<{
  (e: "close"): void;
  (e: "added"): void;
}>();

const collectionStore = useCollectionStore();

const props = defineProps<{
  sets: SetViewModel[];
  variantTypes: CardVariantTypeApiModel[];
}>();

const selectedSetId = ref(props.sets[props.sets.length - 1]?.id ?? 0);
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
    console.log(result);
    
    if ("errorMessage" in result) {
      errorMessage.value = result.errorMessage;
    } else {
      verifiedCards.value = result.cards;
      showVerifyModal.value = true;
    }
  } catch (error: any) {
    console.error(error);
    errorMessage.value = error.data.data.errorMessage || "An error occurred during verification";
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
