<script setup lang="ts">
import {
  PhFolder,
  PhFolderOpen,
  PhStack,
  PhTray,
  PhPlus,
  PhPencilSimple,
  PhTrash,
} from "@phosphor-icons/vue";
import type { DeckFolderApiModel } from "~/api/default";
import type { FolderScope } from "./FolderScope";

const props = defineProps<{
  folders: DeckFolderApiModel[];
  activeScope: FolderScope;
}>();

const emit = defineEmits<{
  select: [scope: FolderScope];
  create: [];
  rename: [folder: DeckFolderApiModel];
  delete: [folder: DeckFolderApiModel];
}>();

function isActiveFolder(folder: DeckFolderApiModel) {
  return (
    props.activeScope.type === "folder" &&
    props.activeScope.folderId === folder.id
  );
}
</script>

<template>
  <aside class="md:w-64 shrink-0">
    <nav class="flex flex-col gap-1">
      <button
        type="button"
        class="flex items-center gap-2 rounded px-3 py-2 text-left hover:bg-gray-100"
        :class="activeScope.type === 'all' ? 'bg-gray-100 font-semibold' : ''"
        @click="emit('select', { type: 'all' })"
      >
        <PhStack :size="18" /> All decks
      </button>
      <button
        type="button"
        class="flex items-center gap-2 rounded px-3 py-2 text-left hover:bg-gray-100"
        :class="activeScope.type === 'unfiled' ? 'bg-gray-100 font-semibold' : ''"
        @click="emit('select', { type: 'unfiled' })"
      >
        <PhTray :size="18" /> Unfiled
      </button>

      <div class="mt-3 mb-1 flex items-center justify-between px-3">
        <span class="text-xs uppercase tracking-wide text-gray-500">Folders</span>
        <button
          type="button"
          class="text-gray-500 hover:text-main-color"
          aria-label="New folder"
          title="New folder"
          @click="emit('create')"
        >
          <PhPlus :size="18" />
        </button>
      </div>

      <p v-if="folders.length === 0" class="px-3 py-1 text-sm text-gray-500">
        No folders yet.
      </p>

      <div
        v-for="folder in folders"
        :key="folder.id"
        class="group flex items-center rounded hover:bg-gray-100"
        :class="isActiveFolder(folder) ? 'bg-gray-100 font-semibold' : ''"
      >
        <button
          type="button"
          class="flex flex-1 items-center gap-2 px-3 py-2 text-left min-w-0"
          @click="emit('select', { type: 'folder', folderId: folder.id! })"
        >
          <PhFolderOpen v-if="isActiveFolder(folder)" :size="18" class="shrink-0" />
          <PhFolder v-else :size="18" class="shrink-0" />
          <span class="truncate">{{ folder.name }}</span>
          <span class="ml-auto text-xs text-gray-500 shrink-0">{{
            folder.deckCount
          }}</span>
        </button>
        <div
          class="flex items-center gap-1 pr-2 opacity-0 group-hover:opacity-100 transition-opacity"
        >
          <button
            type="button"
            class="text-gray-500 hover:text-main-color"
            aria-label="Rename folder"
            title="Rename"
            @click.stop="emit('rename', folder)"
          >
            <PhPencilSimple :size="16" />
          </button>
          <button
            type="button"
            class="text-gray-500 hover:text-red-600"
            aria-label="Delete folder"
            title="Delete"
            @click.stop="emit('delete', folder)"
          >
            <PhTrash :size="16" />
          </button>
        </div>
      </div>
    </nav>
  </aside>
</template>
