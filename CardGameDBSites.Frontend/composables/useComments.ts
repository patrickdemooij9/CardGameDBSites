import type { CommentViewModel } from "~/api/default";
import { DoFetch } from "~/helpers/RequestsHelper";

export function useComments(){
    const loadCommentsByDeckId = async (deckId: number) => {
        const { data } = await useAsyncData(`comments-deck-${deckId}`, () =>
            DoFetch<CommentViewModel[]>(`/api/comments/getByDeck?deckId=${deckId}`),
        );

        return data.value ?? [];
    }

    return {
        loadCommentsByDeckId
    }
}