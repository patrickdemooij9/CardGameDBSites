<script setup lang="ts">
import { PhX } from "@phosphor-icons/vue";
import type { CardDetailApiModel } from "~/api/default";
import CardDetailAbility from "~/components/cards/CardDetailAbility.vue";
import { GetCrop } from "~/helpers/CropUrlHelper";
import SiteService from "~/services/SiteService";

const emit = defineEmits<{
  (e: "close"): void;
}>();

defineProps<{
  selectedCard: CardDetailApiModel;
}>();

const siteSettings = await new SiteService().getSettings();
</script>

<template>
  <Teleport to="#root">
    <div
      class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60"
    >
      <div
        class="relative bg-white rounded-lg shadow-lg md:w-[70vw] w-screen mx-4 max-h-screen overflow-auto"
      >
        <button
          class="absolute top-3 right-3 text-gray-400 hover:text-gray-700 transition-colors"
          @click.prevent="emit('close')"
        >
          <PhX class="h-6 w-6" />
        </button>

        <div class="flex md:flex-row flex-col gap-4 p-6 md:mt-0 mt-4">
          <div class="flex flex-col">
            <div
              class="md:w-72 mb-4 flex items-center justify-center bg-gray-100 rounded shadow-inner"
            >
              <img :src="GetCrop(selectedCard.imageUrl, undefined)!" />
            </div>
            <!--<div v-if="selectedCharacter.images.length > 1" class="switcher mt-4">
                        <div v-for="image in selectedCharacter.images" class="switch-item" :class="selectedCharacterImage === image ? 'active' : ''" v-on:click="selectImage(image)">
                            <span>{{image.label}}</span>
                        </div>
                    </div>-->
          </div>

          <div>
            <h2 class="text-xl font-semibold mb-2">
              {{ selectedCard.displayName }}
            </h2>
            <div
              v-for="(element, index) in siteSettings.cardSections"
              :key="index"
              class="mb-2"
            >
              <div v-if="!element.isDivider">
                <CardDetailAbility :section="element" :card="selectedCard" />
              </div>
              <hr v-else="index !== displayItems.length - 1" class="mb-2" />
            </div>
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
    </div>
  </Teleport>
</template>
