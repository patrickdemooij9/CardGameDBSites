<template>
  <h2 class="text-base my-2">{{ comments.length }} comments</h2>

  <div v-for="comment in comments" class="py-2 px-2 group hover:bg-gray-50">
    <div class="flex gap-4 items-baseline mb-0.5">
      <h3 class="text-base">{{ comment.createdByName }}</h3>
      <p>{{ ParseToHumanReadableText(comment.createdAt) }}</p>

      <button
        v-if="comment.createdById === accountStore.member?.id"
        class="invisible group-hover:visible text-red-500 hover:text-red-700"
        @click="openDeleteModal(comment)"
      >
        <PhTrash />
      </button>
    </div>

    <p>{{ comment.comment }}</p>
  </div>

  <div v-if="accountStore.isLoggedIn" class="mt-4">
    <textarea
      v-model="newComment"
      placeholder="Write a comment..."
      class="w-full border rounded p-2 text-sm resize-y min-h-[80px]"
    ></textarea>
    <Button
      :disabled="!newComment.trim()"
      @click="submitComment"
      :button-type="ButtonType.Success"
      class="mt-2"
    >
      Post comment
    </Button>
  </div>

  <PopupBase
    v-if="showDeleteModal"
    :size="PopupSize.Small"
    @close="closeDeleteModal"
  >
    <h3 class="text-lg font-bold mb-2">Delete comment</h3>
    <p class="text-gray-600">Are you sure you want to delete this comment?</p>

    <template #actions>
      <Button
        @click="confirmDelete"
        :button-type="ButtonType.Danger"
        class="btn bg-red-500 hover:bg-red-600 text-white"
      >
        Delete
      </Button>
    </template>
  </PopupBase>
</template>

<script setup lang="ts">
import type { CommentViewModel } from "~/api/default";
import { ParseToHumanReadableText } from "~/helpers/DateHelper";
import { useAccountStore } from "~/stores/AccountStore";
import { PhTrash } from "@phosphor-icons/vue";
import PopupBase from "~/components/popups/PopupBase.vue";
import { PopupSize } from "~/components/popups/PopupTypes";
import Button from "~/components/shared/Button.vue";
import ButtonType from "~/components/shared/ButtonType";

const accountStore = useAccountStore();

const props = defineProps<{
  comments: CommentViewModel[];
}>();

const emit = defineEmits<{
  (e: "deleteComment", comment: CommentViewModel): void;
  (e: "addComment", comment: string): void;
}>();

const newComment = ref("");
const commentToDelete = ref<CommentViewModel | null>(null);
const showDeleteModal = ref(false);

function submitComment() {
  const text = newComment.value.trim();
  if (!text || !accountStore.member) return;

  emit("addComment", text);
  newComment.value = "";
}

function openDeleteModal(comment: CommentViewModel) {
  commentToDelete.value = comment;
  showDeleteModal.value = true;
}

function closeDeleteModal() {
  commentToDelete.value = null;
  showDeleteModal.value = false;
}

function confirmDelete() {
  if (!commentToDelete.value) return;

  // TODO: call backend to delete comment with id: ${commentToDelete.value.id}

  emit("deleteComment", commentToDelete.value);
  closeDeleteModal();
}
</script>
