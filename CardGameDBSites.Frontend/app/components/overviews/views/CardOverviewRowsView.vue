<script setup lang="ts">
import type { CardDetailApiModel } from "~/api/default";
import { PhBooks } from "@phosphor-icons/vue";
import Button from "~/components/shared/Button.vue";
import ButtonType from "~/components/shared/ButtonType";

type CardOverviewTableColumn = {
  alias: string;
  displayName: string;
};

defineProps<{
  cards: CardDetailApiModel[];
  tableColumns: CardOverviewTableColumn[];
  isLoggedIn: boolean;
  getAmountForSet: (card: CardDetailApiModel) => number;
}>();

const emit = defineEmits<{
  (e: "openCollection", card: CardDetailApiModel): void;
}>();
</script>

<template>
  <div class="container px-4 md:px-8 overflow-x-auto md:overflow-visible">
    <table class="w-full text-left border-collapse">
      <thead>
        <tr class="border-b-2 border-gray-300">
          <th class="py-2 pr-4 font-semibold sticky top-0 bg-white z-10">Name</th>
          <th
            v-for="col in tableColumns"
            :key="col.alias"
            class="py-2 pr-4 font-semibold sticky top-0 bg-white z-10"
          >
            {{ col.displayName }}
          </th>
          <th v-if="isLoggedIn" class="py-2 pr-4 font-semibold sticky top-0 bg-white z-10">Collection</th>
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
            v-for="col in tableColumns"
            :key="col.alias"
            class="py-2 pr-4 text-sm text-gray-700"
          >
            {{ card.attributes?.[col.alias]?.join(", ") ?? "-" }}
          </td>
          <td v-if="isLoggedIn" class="py-2 pr-4">
            <div class="flex items-center gap-2">
              <span :aria-label="`${getAmountForSet(card)} copies`">
                {{ getAmountForSet(card) }}
                <span class="md:inline hidden" aria-hidden="true">copies</span>
              </span>
              <Button
                :button-type="ButtonType.Outline"
                class="flex justify-center"
                @click="emit('openCollection', card)"
              >
                <PhBooks />
              </Button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
