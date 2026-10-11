<script setup lang="ts">
import { PhMinus, PhPlus, PhX } from "@phosphor-icons/vue";
import type { CardDetailApiModel, CardVariantTypeApiModel } from "~/api/default";
import { useAppToast } from "~/composables/useAppToast";
import { useCards } from "~/composables/useCards";
import { useCollection } from "~/composables/useCollection";

const emit = defineEmits<{
  (e: "close"): void;
}>();

const props = defineProps<{
  card: CardDetailApiModel;
}>();

const cards = useCards();
const collectionService = useCollection();
const toast = useAppToast();

const open = ref(true);
const isLoading = ref(true);
const isSaving = ref(false);
const variantTypes = ref<CardVariantTypeApiModel[]>([]);
const variantAmounts = ref<Record<number, number>>({});

const variants = computed(() =>
  (props.card.variants ?? [])
    .filter((variant) => variant.setId == props.card.setId)
    .sort((a, b) => (a.variantTypeId ?? 0) - (b.variantTypeId ?? 0))
);

onMounted(async () => {
  try {
    const [, types] = await Promise.all([
      collectionService.loadCards([props.card.baseId!]),
      cards.loadVariantTypes(),
    ]);
    variantTypes.value = types;
    collectionService.getCards(props.card.baseId!).forEach((card) => {
      variantAmounts.value[card.variantId!] = card.amount!;
    });
    isLoading.value = false;
  } catch {
    toast.error("Could not load your collection.");
    close();
  }
});

function variantName(variantTypeId?: number | null) {
  if (!variantTypeId) {
    return "Normal";
  }
  return variantTypes.value.find((it) => it.id === variantTypeId)?.displayName ?? "Unknown";
}

function amountOf(variantId: number) {
  return variantAmounts.value[variantId] ?? 0;
}

function setAmount(variantId: number, amount: number) {
  variantAmounts.value[variantId] = Math.max(0, Number.isNaN(amount) ? 0 : amount);
}

function close() {
  open.value = false;
}

async function save() {
  isSaving.value = true;
  try {
    await collectionService.saveCards(props.card.baseId!, variantAmounts.value);
    close();
  } catch {
    toast.error("Could not save your collection.");
  } finally {
    isSaving.value = false;
  }
}
</script>

<template>
  <Teleport to="#root">
    <Transition name="drawer-fade" appear>
      <div v-if="open" class="fixed inset-0 z-50 bg-black/60" @click="close" />
    </Transition>
    <Transition name="sheet-slide" appear @after-leave="emit('close')">
      <div
        v-if="open"
        class="fixed inset-x-0 bottom-0 z-50 flex max-h-[85vh] flex-col rounded-t-2xl bg-white shadow-xl md:inset-x-auto md:bottom-auto md:left-1/2 md:top-1/2 md:w-[28rem] md:-translate-x-1/2 md:-translate-y-1/2 md:rounded-lg"
        role="dialog"
        aria-modal="true"
        aria-labelledby="collection-sheet-title"
      >
        <div class="mx-auto mt-2 h-1 w-10 shrink-0 rounded-full bg-gray-300 md:hidden" />
        <div class="flex items-start gap-3 px-4 pt-3 pb-2">
          <div class="min-w-0 flex-1">
            <h3 id="collection-sheet-title" class="text-base font-semibold text-gray-900">
              Manage collection
            </h3>
            <p class="truncate text-sm text-gray-500">{{ card.displayName }}</p>
          </div>
          <button class="-mr-2 p-2 text-gray-400 hover:text-gray-700" aria-label="Close" @click="close">
            <PhX :size="22" />
          </button>
        </div>

        <div class="flex-1 overflow-y-auto px-4">
          <div v-if="isLoading" class="flex justify-center py-8">
            <div class="h-8 w-8 animate-spin rounded-full border-b-2 border-main-color"></div>
          </div>
          <ul v-else class="m-0 list-none divide-y divide-gray-100 p-0">
            <li
              v-for="variant in variants"
              :key="variant.cardVariantId"
              class="flex items-center justify-between gap-3 py-3"
            >
              <span class="font-medium">{{ variantName(variant.variantTypeId) }}</span>
              <div class="flex items-center rounded-md border border-gray-300">
                <button
                  class="p-3 disabled:opacity-30"
                  :disabled="amountOf(variant.cardVariantId!) <= 0"
                  aria-label="Decrease count"
                  @click="setAmount(variant.cardVariantId!, amountOf(variant.cardVariantId!) - 1)"
                >
                  <PhMinus :size="16" />
                </button>
                <input
                  type="number"
                  inputmode="numeric"
                  min="0"
                  class="w-12 border-x border-gray-300 py-2 text-center tabular-nums [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none"
                  :aria-label="`${variantName(variant.variantTypeId)} copies`"
                  :value="amountOf(variant.cardVariantId!)"
                  @input="setAmount(variant.cardVariantId!, Number.parseInt(($event.target as HTMLInputElement).value))"
                />
                <button
                  class="p-3"
                  aria-label="Increase count"
                  @click="setAmount(variant.cardVariantId!, amountOf(variant.cardVariantId!) + 1)"
                >
                  <PhPlus :size="16" />
                </button>
              </div>
            </li>
          </ul>
        </div>

        <div class="flex gap-2 border-t border-gray-200 px-4 pt-3 pb-safe-bottom">
          <button
            class="mb-3 flex-1 rounded-md border border-gray-300 px-3 py-2.5 font-semibold"
            @click="close"
          >
            Cancel
          </button>
          <button
            class="mb-3 flex-1 rounded-md bg-main-color px-3 py-2.5 font-semibold text-white disabled:opacity-50"
            :disabled="isLoading || isSaving"
            @click="save"
          >
            {{ isSaving ? "Saving…" : "Save" }}
          </button>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
