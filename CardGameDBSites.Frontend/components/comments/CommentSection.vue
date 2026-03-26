<template>
<h2 class="text-base my-2">{{ comments.length }} comments</h2>

<div v-for="comment in comments" class="py-2 px-2 group hover:bg-gray-50">
        <div class="flex gap-4 items-baseline mb-0.5">
            <h3 class="text-base">{{ comment.createdByName }}</h3>
            <p>{{ ParseToHumanReadableText(comment.createdAt) }}</p>

            <!--@if (currentUserId == comment.CreatedBy)
            {
                <button class="group-hover:visible md:invisible js-open-modal" js-modal="modal-delete-@comment.Id">
                    <i class="ph-fill ph-trash"></i>
                </button>
            }-->
        </div>

        <p>{{ comment.comment }}</p>
    </div>
</template>

<script setup lang="ts">
import type { CommentViewModel } from '~/api/default';
import { ParseToHumanReadableText } from '~/helpers/DateHelper';


defineProps<{
    comments: CommentViewModel[]
}>();
</script>
