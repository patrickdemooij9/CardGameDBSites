<script setup lang="ts">
import type { AccountDecksContentModel } from '~/api/umbraco';
import BaseDeckOverview from '../overviews/BaseDeckOverview.vue';
import DeckCard from '../cards/deckCards/DeckCard.vue';
import Button from '../shared/Button.vue';
import ButtonType from '../shared/ButtonType';
import DeckService from '~/services/DeckService';

defineProps<{
    content: AccountDecksContentModel
}>();

const router = useRouter();
const memberId = ref<number | undefined>(undefined);
const isLoading = ref(true);

onMounted(async () => {
    const isLoggedIn = await useAccountStore().checkLogin();
    if (!isLoggedIn){
        router.push('/login');
    }

    memberId.value = useAccountStore().member?.id;
    isLoading.value = false;
})

async function deleteDeck(deckId: number) {
    if (!confirm("Are you sure you want to delete this deck?")) return;
    
    await new DeckService().deleteDeck(deckId);
    // Force refresh by navigating to same page
    router.go(0);
}
</script>

<template>
    <div v-if="isLoading" class="flex justify-center items-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
    </div>
    <div v-else class="container px-4 pt-8 md:px-8 mb-6" v-if="memberId">
        <h1>Your decks</h1>
        <BaseDeckOverview
            :decks-per-row="1"
            :user-id="memberId">
            <template #default="{ decks }">
                <div v-if="decks!.items!.length === 0" class="text-center">
                    <p>You have no decks yet. Create one to get started!</p>
                    <router-link to="/create-squad" class="btn btn-primary">Create Deck</router-link>
                </div>
                <div v-else>
                    <div v-for="deck in decks!.items" :key="deck.id" class="border rounded px-4 py-2 mb-2 bg-white">
                        <div class="flex justify-between items-center mb-2">
                            <h1 class="text-base">{{ deck.name }}</h1>
                        </div>
                        <div class="flex gap-4">
                            <Button :button-type="ButtonType.Primary" class="border rounded" @click="router.push(`/decks/${deck.id}`)">
                                View deck
                            </Button>
                            <Button :button-type="ButtonType.Primary" class="border rounded" @click="router.push(`/create-deck?id=${deck.id}`)">
                                Edit deck
                            </Button>
                            <Button :button-type="ButtonType.Danger" class="border rounded" @click="deck.id && deleteDeck(deck.id)">
                                Delete
                            </Button>
                        </div>
                    </div>
                </div>
            </template>
        </BaseDeckOverview>
    </div>
    
</template>