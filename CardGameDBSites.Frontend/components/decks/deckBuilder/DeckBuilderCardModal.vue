<script setup lang="ts">
import type { CardDetailApiModel } from '~/api/default';

const emit = defineEmits<{
    (e: "close"): void;
}>();

const props = defineProps<{
    selectedCard: CardDetailApiModel
}>();
</script>

<template>
    <Teleport :to="'body'" class="modal-overlay">
        <div class="modal-wrapper">
            <div class="modal card-modal">
                <div class="close-button" v-on:click.prevent="emit('close')">
                    <i class="ph ph-x"></i>
                </div>

                <div class="image-switcher flex flex-col">
                    <div class="card-image grow">
                        <img :src="selectedCard.imageUrl!" />
                    </div>
                    <!--<div v-if="selectedCharacter.images.length > 1" class="switcher mt-4">
                        <div v-for="image in selectedCharacter.images" class="switch-item" :class="selectedCharacterImage === image ? 'active' : ''" v-on:click="selectImage(image)">
                            <span>{{image.label}}</span>
                        </div>
                    </div>-->
                </div>

                <div class="modal-content">
                    <h2 class="text-lg">{{selectedCard.displayName}}</h2>
                    <!--@foreach (var display in Model.Details)
                    {
                        var ability = display.Ability as CardAttribute;
                        var namePosition = display.NamePosition.IfNullOrWhiteSpace("Inline");

                        <div v-if="selectedCharacter !== undefined && hasAbility(selectedCharacter, '@ability.Name')" class="mb-2">
                            @if (namePosition.Equals("Heading"))
                            {
                                <h3>@ability.DisplayName:</h3>
                            }
                            <p>
                                @if (namePosition.Equals("Inline"))
                                {
                                    <b>@ability.DisplayName: </b>
                                }
                                <span v-html="getAbilityDisplayByType(selectedCharacter, '@ability.Name')"></span>
                            </p>
                        </div>
                    }

                    <div class="modal-actions squad-actions">
                        <div v-for="location in selectedCharacter.validLocations" class="flex gap-2 justify-between">
                            <div v-if="selectedCharacter.presentInSlot?.slot === location.slot && isCardAllowedToRemove(location.slot, selectedCharacter)">
                                <button v-if="selectedCharacter.presentInSlot.slot.maxCards === 1" v-on:click.prevent.stop="removeCharacterFromSquad(selectedCharacter)" type="button">
                                    <span>Remove</span>
                                </button>
                                <button v-if="selectedCharacter.presentInSlot.slot.maxCards > 1 || selectedCharacter.presentInSlot.slot.minCards > 1" v-on:click.prevent.stop="removeCharacterFromSquad(selectedCharacter)" type="button">
                                    <span>-</span>
                                </button>
                            </div>
                            <div v-if="location.isAllowed && (selectedCharacter.presentInSlot?.slot !== location.slot || !isSlotFillForCard(location.slot, selectedCharacter))">
                                <button v-if="location.slot.maxCards === 1" v-on:click.prevent.stop="addToSquad(location.squad, location.slot, selectedCharacter)" type="button">
                                    <span>Add to {{location.squad?.name ?? location.slot.label}}</span>
                                </button>
                                <button v-if="(location.slot.maxCards > 1 || location.slot.minCards > 1) && !isSlotFillForCard(location.slot, selectedCharacter) && !isSlotFilled(location.slot)" v-on:click.prevent.stop="addToSquad(location.squad, location.slot, selectedCharacter)" type="button">
                                    <span>+</span>
                                </button>
                            </div>
                        </div>
                    </div>-->
                </div>
            </div>
        </div>
    </Teleport>
</template>