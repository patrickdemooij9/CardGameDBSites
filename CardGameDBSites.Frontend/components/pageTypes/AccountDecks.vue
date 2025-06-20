<script setup lang="ts">
import type { AccountDecksContentModel } from '~/api/umbraco';
import BaseDeckOverview from '../overviews/BaseDeckOverview.vue';
import DeckCard from '../cards/deckCards/DeckCard.vue';
import Button from '../shared/Button.vue';
import ButtonType from '../shared/ButtonType';

defineProps<{
    content: AccountDecksContentModel
}>();

const router = useRouter();

onMounted(() => {
    if (!useMemberStore().isLoggedIn){
        router.push('/login');
    }
})
</script>

<template>
    <div class="container px-4 pt-8 md:px-8 mb-6">
        <h1>Your decks</h1>
        <BaseDeckOverview
            :decks-per-row="1"
            :user-id="useMemberStore().member?.id">
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
                        </div>
                    </div>
                </div>
            </template>
        </BaseDeckOverview>
    </div>
    
</template>