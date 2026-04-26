<script setup lang="ts">
import {
  PhCardsThree,
  PhCrown,
  PhImage,
  PhPiggyBank,
} from "@phosphor-icons/vue";
import type { DeckActionApiModel, DeckApiModel } from "~/api/default";
import PopupBase from "../popups/PopupBase.vue";
import { PopupSize } from "../popups/PopupTypes";
import Button from "../shared/Button.vue";
import ButtonType from "../shared/ButtonType";
import { useAppToast } from "~/composables/useAppToast";

const props = defineProps<{
  deck: DeckApiModel;
  action: DeckActionApiModel;
  missingCardsString?: string;
}>();

const showModal = ref(false);
const toast = useAppToast();
const isLoading = ref(false);

const icons: { [key: string]: Component } = {
  crown: PhCrown,
  "cards-three": PhCardsThree,
  image: PhImage,
  "piggy-bank": PhPiggyBank,
};

async function copyToClipboard(action: DeckActionApiModel) {
  if (isLoading.value) return;
  
  isLoading.value = true;
  try {
    const url = `/api/proxy/umbraco/api/export/export?deckId=${props.deck.id}&exportId=${action.id}`;
    const response = await fetch(url);
    const text = await response.text();
    await navigator.clipboard.writeText(text);
    toast.success("Copied to clipboard!");
  } catch (error) {
    toast.error("Failed to copy to clipboard");
  } finally {
    isLoading.value = false;
  }
}

async function handleRedirectExport(action: DeckActionApiModel) {
  if (isLoading.value) return;

  isLoading.value = true;
  try {
    const url = `/api/proxy/umbraco/api/export/export?deckId=${props.deck.id}&exportId=${action.id}`;
    const response = await fetch(url);
    if (!response.ok) throw new Error("Export failed");
    const data = await response.json();
    if (typeof data.redirectUrl === "string" && data.redirectUrl.startsWith("https://")) {
      location.href = data.redirectUrl;
    }
  } catch (error) {
    toast.error("Failed to open redirect");
  } finally {
    isLoading.value = false;
  }
}

async function handleForceTable() {
  if (isLoading.value) return;

  isLoading.value = true;
  try {
    const url = `/api/proxy/umbraco/api/export/ExportForceTable?deckId=${props.deck.id}`;
    const response = await fetch(url);
    if (!response.ok) throw new Error("Export failed");
    const data = await response.json();
    if (typeof data.redirectUrl === "string" && data.redirectUrl.startsWith("https://")) {
      location.href = data.redirectUrl;
    }
  } catch (error) {
    toast.error("Failed to open ForceTable");
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <div v-if="action.type === 'ForceTable'">
    <button
      class="flex align-center gap-1 no-underline"
      :disabled="isLoading"
      @click="handleForceTable()"
    >
      <component :is="icons['crown']"></component>
      <p>{{ action.displayName }}</p>
    </button>
  </div>
  <div v-else-if="action.type === 'DeckMissingCardsExport'">
    <form id="buyCardsForm" method="post" action="https://api.tcgplayer.com/massentry?productline=Star Wars Unlimited">
      <input type="hidden" name="c" :value="missingCardsString" />
      <input type="hidden" name="affiliateurl" value="https://tcgplayer.pxf.io/c/4924415/1780961/21018" />
      <button type="submit" class="flex align-center gap-1">
        <component :is="icons[action.icon!]"></component>
        <p>{{ action.displayName }}</p>
      </button>
    </form>
  </div>
  <div v-else-if="action.type === 'DeckExportGroup'">
    <button
      class="flex align-center gap-1 no-underline"
      @click="showModal = true"
    >
      <component :is="icons[action.icon!]"></component>
      <p>{{ action.displayName }}</p>
    </button>
  </div>
  <div v-else-if="action.isCopyClipboard">
    <button
      class="flex align-center gap-1 no-underline"
      :disabled="isLoading"
      @click="copyToClipboard(action)"
    >
      <component :is="icons[action.icon!]"></component>
      <p>{{ action.displayName }}</p>
    </button>
  </div>
  <div v-else-if="action.type === 'DeckRedirectExport'">
    <button
      class="flex align-center gap-1 no-underline"
      :disabled="isLoading"
      @click="handleRedirectExport(action)"
    >
      <component :is="icons[action.icon!]"></component>
      <p>{{ action.displayName }}</p>
    </button>
  </div>
  <div v-else>
    <a
      class="flex align-center gap-1 no-underline"
      :href="`/api/proxy/umbraco/api/export/export?deckId=${deck.id}&exportId=${action.id}`"
      target="_blank"
    >
      <component :is="icons[action.icon!]"></component>
      <p>{{ action.displayName }}</p>
    </a>
  </div>

  <PopupBase
    v-if="showModal"
    :size="PopupSize.Small"
    @close="showModal = false"
  >
    <h3 class="text-lg font-bold mb-2">{{ action.popupTitle }}</h3>
    <p class="text-gray-600" v-html="action.popupDescription"></p>
    <div v-for="subAction in action.subActions ?? []" class="mt-4">
      <h3 class="text-base font-semibold leading-6 text-gray-900">
        {{ subAction.popupTitle }}
      </h3>
      <div class="mt-2 mb-2">
        <div v-html="subAction.popupDescription"></div>
        <button
          v-if="subAction.isCopyClipboard"
          class="no-underline w-full"
          @click="copyToClipboard(subAction)"
        >
          <Button :button-type="ButtonType.Outline" class="w-full mt-2">
            <div class="flex gap-2 align-center">
              <p>
                {{ subAction.displayName }}
              </p>
            </div>
          </Button>
        </button>
        <button
          v-else-if="subAction.type === 'DeckRedirectExport'"
          class="no-underline w-full"
          :disabled="isLoading"
          @click="handleRedirectExport(subAction)"
        >
          <Button :button-type="ButtonType.Outline" class="w-full mt-2">
            <div class="flex gap-2 align-center">
              <p>
                {{ subAction.displayName }}
              </p>
            </div>
          </Button>
        </button>
        <a
          v-else
          :href="`/api/proxy/umbraco/api/export/export?deckId=${deck.id}&exportId=${subAction.id}`"
          class="no-underline"
        >
          <Button :button-type="ButtonType.Outline" class="w-full mt-2">
            <div class="flex gap-2 align-center">
              <p>
                {{ subAction.displayName }}
              </p>
            </div>
          </Button>
        </a>
      </div>
    </div>
  </PopupBase>
</template>
