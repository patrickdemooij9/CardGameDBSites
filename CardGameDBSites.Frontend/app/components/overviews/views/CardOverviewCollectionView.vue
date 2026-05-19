<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import { PhMinus, PhPlus } from "@phosphor-icons/vue";

type CollectionColumn = {
  variantTypeId: number | null;
  displayName: string;
};

type CollectionCell = CollectionColumn & {
  key: string;
  amount: number | null;
  isUpdating: boolean;
};

defineProps<{
  cards: CardDetailApiModel[];
  columns: CollectionColumn[];
  getCollectionCells: (card: CardDetailApiModel) => CollectionCell[];
}>();

const emit = defineEmits<{
  (e: "updateCollectionAmount", card: CardDetailApiModel, variantTypeId: number | null, delta: number): void;
}>();
</script>

<template>
  <div class="container px-4 md:px-8 overflow-x-auto">
    <table class="w-full min-w-max text-left border-collapse">
      <thead>
        <tr class="border-b-2 border-gray-300">
          <th class="py-2 pr-4 font-semibold sticky top-0 bg-white z-10">Name</th>
          <th
            v-for="collectionColumn in columns"
            :key="collectionColumn.variantTypeId ?? 'normal'"
            class="py-2 pr-4 font-semibold sticky top-0 bg-white z-10"
          >
            {{ collectionColumn.displayName }}
          </th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="card in cards"
          :key="card.baseId"
          class="border-b border-gray-200 hover:bg-gray-50"
        >
          <td class="py-2 pr-4">
            <NuxtLink
              :href="card.urlSegment"
              class="no-underline font-medium hover:underline cursor-source"
              v-cursor-image="card.imageUrl?.url ?? ''"
            >
              {{ card.displayName }}
            </NuxtLink>
          </td>
          <td
            v-for="collectionCell in getCollectionCells(card)"
            :key="collectionCell.key"
            class="py-2 pr-4"
          >
            <div
              v-if="collectionCell.amount !== null"
              class="flex items-center gap-2"
            >
              <button
                type="button"
                class="border rounded px-1.5 py-0.5 hover:bg-gray-100 disabled:bg-gray-100 disabled:text-gray-400"
                :aria-label="`Decrease ${collectionCell.displayName} amount for ${card.displayName}`"
                :disabled="
                  collectionCell.isUpdating ||
                  collectionCell.amount === 0
                "
                @click="emit('updateCollectionAmount', card, collectionCell.variantTypeId, -1)"
              >
                <PhMinus :size="12" />
              </button>
              <span class="min-w-6 text-center text-sm font-semibold">
                {{ collectionCell.amount }}
              </span>
              <button
                type="button"
                class="border rounded px-1.5 py-0.5 hover:bg-gray-100 disabled:bg-gray-100 disabled:text-gray-400"
                :aria-label="`Increase ${collectionCell.displayName} amount for ${card.displayName}`"
                :disabled="collectionCell.isUpdating"
                @click="emit('updateCollectionAmount', card, collectionCell.variantTypeId, 1)"
              >
                <PhPlus :size="12" />
              </button>
            </div>
            <span v-else class="text-sm text-gray-400">-</span>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
