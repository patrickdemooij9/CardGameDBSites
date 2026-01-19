<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import PopupBase from "~/components/popups/PopupBase.vue";
import { PopupSize } from "./PopupTypes";

const emit = defineEmits<{
  (e: "close"): void;
}>();

const props = defineProps<{
  card: CardDetailApiModel;
}>();

const cardStore = useCardsStore();
const collectionStore = useCollectionStore();
await collectionStore.loadCards([props.card.baseId!]);
await cardStore.loadVariantTypes();

const variantTypes = computed(() => cardStore.getVariants);
const variantAmounts = ref<{ [key: number]: number }>({});
collectionStore.getCards(props.card.baseId!).forEach((card) => {
  variantAmounts.value[card.variantId!] = card.amount!;
});

function getVariantType(variantTypeId: number) {
  return variantTypes.value.find((it) => it.id === variantTypeId);
}

function updateVariantAmount(variantId: number, amount: number) {
  variantAmounts.value[variantId] = amount;
}

async function saveVariants() {
  await collectionStore.save(variantAmounts.value);
  emit("close");
}
</script>

<template>
    <PopupBase :size="PopupSize.Medium" @close="emit('close')">
        <h3 class="text-base font-semibold leading-6 text-gray-900">
          Manage collection
        </h3>
        <div class="mt-2 mb-2">
          <p class="text-sm">
            Update the amount of cards that you have for <span class="font-semibold">{{ card.displayName }}</span>
          </p>
        </div>
        <div class="flex gap-1 flex-col mt-4" v-for="variant in card.variants?.filter(v => v.setId == card.setId)">
          <label class="font-bold">
            {{ variant.variantTypeId ? getVariantType(variant.variantTypeId)?.displayName : "Normal" }}
          </label>
          <input type="number"
            class="px-4 py-2 rounded border border-solid border-gray-300"
            min="0"
            :value="variantAmounts[variant.cardVariantId!] ?? 0"
            @input="updateVariantAmount(variant.cardVariantId!, Number.parseInt(($event.target as HTMLInputElement).value))">
          </input>
        </div>
        <template #actions>
          <button
            type="button"
            @click="saveVariants"
            class="mt-3 inline-flex w-full justify-center rounded-md bg-green-400 px-3 py-2 text-sm font-semibold text-white shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-green-500 sm:mt-0 sm:w-auto"
          >
            Confirm
          </button>
        </template>
    </PopupBase>
</template>
