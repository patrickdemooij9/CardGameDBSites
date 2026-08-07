import type { CommentViewModel } from "~/api/default";
import { DoServerFetch } from "~/helpers/RequestsHelper";

export function useComments() {
  const { $api } = useNuxtApp();

  const loadCommentsByDeckId = async (deckId: number) => {
    const data = await $api<CommentViewModel[]>("/api/comments/getByDeck", {
      query: { deckId },
    });

    return data ?? [];
  };

  const saveCommentByDeckId = async (deckId: number, content: string) => {
    const comment = await DoServerFetch<CommentViewModel>(
      "/api/comments/addDeckComment",
      true,
      {
        method: "POST",
        body: JSON.stringify({
          deckId: deckId,
          comment: content,
        }),
      },
    );
    return comment;
  };

  const deleteDeckComment = async (commentId: number) => {
    await DoServerFetch(
      `/api/comments/deleteDeckComment?commentId=${commentId}`,
      true,
      {
        method: "DELETE"
      }
    );
  };

  const loadCommentsByCardId = async (cardId: number) => {
    const data = await $api<CommentViewModel[]>("/api/comments/getByCard", {
      query: { cardId },
    });

    return data ?? [];
  };

  const saveCommentByCardId = async (cardId: number, content: string) => {
    const comment = await DoServerFetch<CommentViewModel>(
      "/api/comments/addCardComment",
      true,
      {
        method: "POST",
        body: JSON.stringify({
          cardId: cardId,
          comment: content,
        }),
      },
    );
    return comment;
  };

  const deleteCardComment = async (commentId: number) => {
    await DoServerFetch(
      `/api/comments/deleteCardComment?commentId=${commentId}`,
      true,
      {
        method: "DELETE"
      }
    );
  };

  return {
    loadCommentsByDeckId,
    saveCommentByDeckId,
    deleteDeckComment,
    loadCommentsByCardId,
    saveCommentByCardId,
    deleteCardComment
  };
}
