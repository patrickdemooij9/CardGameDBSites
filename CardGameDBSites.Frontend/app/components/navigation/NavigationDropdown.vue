<script setup lang="ts">
import { PhCaretDown } from "@phosphor-icons/vue";
import type NavigationItem from "./NavigationItemModel";

const props = defineProps<{
  label: string;
  url?: string;
  items: NavigationItem[];
  alignRight?: boolean;
}>();

const emit = defineEmits<{
  navigate: [];
}>();

const slots = useSlots();
const hasMenu = computed(() => props.items.length > 0 || !!slots.append);
const dismissed = ref(false);

// The nav survives page changes, so CSS hover and focus would keep the panel open after a click.
function onNavigate() {
  dismissed.value = true;
  (document.activeElement as HTMLElement | null)?.blur();
  emit("navigate");
}

const triggerClass =
  "flex cursor-pointer items-center gap-1 rounded-md py-3 font-medium no-underline md:px-3 md:py-2 md:hover:bg-black/5 dark:md:hover:bg-white/10";
</script>

<template>
  <div class="group relative" @mouseleave="dismissed = false" @focusin="dismissed = false">
    <NuxtLink v-if="url" :to="url" :class="triggerClass" @click="onNavigate">
      {{ label }}
      <PhCaretDown v-if="hasMenu" class="hidden md:block" :size="12" weight="bold" />
    </NuxtLink>
    <span v-else :class="triggerClass" tabindex="0">
      {{ label }}
      <PhCaretDown v-if="hasMenu" class="hidden md:block" :size="12" weight="bold" />
    </span>
    <div
      v-if="hasMenu"
      :class="[
        alignRight ? 'md:right-0' : 'md:left-0',
        { 'md:group-focus-within:block md:group-hover:block': !dismissed },
      ]"
      class="z-30 pl-4 md:absolute md:top-full md:hidden md:pl-0 md:pt-2"
    >
      <div
        class="flex flex-col pb-2 md:min-w-52 md:rounded-lg md:border md:border-line md:bg-surface md:p-1.5 md:text-ink md:shadow-lg"
      >
        <NuxtLink
          v-for="child in items"
          :key="child.url"
          :to="child.url"
          class="rounded-md py-2 text-ink-muted no-underline hover:text-ink md:px-3 md:hover:bg-canvas"
          @click="onNavigate"
        >
          {{ child.name }}
        </NuxtLink>
        <slot name="append"></slot>
      </div>
    </div>
  </div>
</template>
