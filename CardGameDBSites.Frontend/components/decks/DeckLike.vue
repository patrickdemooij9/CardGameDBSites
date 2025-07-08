<script setup lang="ts">
import { PhHeart } from "@phosphor-icons/vue";
import type { DeckApiModel } from "~/api/default";
import DeckService from "~/services/DeckService";

const props = defineProps<{
  deck: DeckApiModel;
}>();

const accountStore = useAccountStore();
const isHovered = ref(false);
const likeChange = ref(0);

function onMouseEnter() {
  isHovered.value = true;
}
function onMouseLeave() {
  isHovered.value = false;
}
async function likeDeck(event: MouseEvent) {
  event.preventDefault();
  if (!accountStore.isLoggedIn) {
    return;
  }

  const change = accountStore.member?.likedDecks?.includes(props.deck.id!)
    ? -1
    : 1;

  await new DeckService().likeDeck(props.deck.id!);
  accountStore.toggleDeckLike(props.deck.id!);
  likeChange.value += change;
}

const showHeartFilled = computed(
  () =>
    (props.deck.amountOfLikes ?? 0) > 0 ||
    accountStore.member?.likedDecks?.includes(props.deck.id!) ||
    (accountStore.isLoggedIn && isHovered.value)
);

//TODO: Add logic for whenever you are logged in
</script>

<template>
  <div
    @mouseenter="onMouseEnter"
    @mouseleave="onMouseLeave"
    @click="likeDeck"
    class="flex items-baseline gap-0.5"
  >
    <span class="shrink-0">
      {{ deck.amountOfLikes! + likeChange }}
    </span>
    <PhHeart
      :class="{ hidden: !showHeartFilled }"
      weight="fill"
      class="text-red-500 hover:block"
    />
    <PhHeart :class="{ hidden: showHeartFilled }" />
  </div>
</template>
