import type { CommentViewModel } from "~/api/default";
import { DoFetch, DoServerFetch } from "~/helpers/RequestsHelper";

export function useComments() {
  const loadCommentsByDeckId = async (deckId: number) => {
    const { data } = await useAsyncData(`comments-deck-${deckId}`, () =>
      DoFetch<CommentViewModel[]>(`/api/comments/getByDeck?deckId=${deckId}`),
    );

    return data.value ?? [];
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
    const { data } = await useAsyncData(`comments-card-${cardId}`, () =>
      DoFetch<CommentViewModel[]>(`/api/comments/getByCard?cardId=${cardId}`),
    );

    return data.value ?? [];
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
