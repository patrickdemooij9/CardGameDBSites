<script setup lang="ts">
import type { CardDetailContentModel } from '~/api/umbraco';
import CardService from '~/services/CardService';
import SiteService from '~/services/SiteService';
import CardDetailAbility from '../cards/CardDetailAbility.vue';

const props = defineProps<{
  content: CardDetailContentModel;
}>();

const card = await useCardsStore().loadCard(props.content.id!);
const siteSettings = await new SiteService().getSettings();
</script>

<template>
    <div class="pt-8">
      <div class="container px-4 md:px-8 mb-8">
        <div class="flex flex-col sm:flex-row gap-8">
          <div class="flex flex-col gap-4 shrink-0">
            <div>
              <img v-if="card.imageUrl" class="sm:h-80" :src="card.imageUrl" />
            </div>
  
            <!--<div v-if="card.backImage">
              <img class="sm:h-80" :src="card.backImage" />
            </div>-->
          </div>
          <div class="w-full">
            <div class="bg-white rounded">
              <div class="p-4">
                <h1 class="text-lg">{{ card.displayName }}</h1>
                <div v-for="(element, index) in siteSettings.cardSections" :key="index" class="mb-2">
                  <div v-if="!element.isDivider">
                    <CardDetailAbility :section="element" :card="card" />
                  </div>
                  <hr v-else="index !== displayItems.length - 1" class="mb-2" />
                </div>
              </div>
            </div>
  
            <!--<div v-if="card.faqLink" class="bg-green-300 rounded mt-8">
              <div class="p-4">
                <p class="mb-2">Looking for frequently asked questions about {{ card.displayName }}?</p>
                <a class="btn btn-outline" :href="card.faqLink" target="_blank">Frequently asked questions</a>
              </div>
            </div>-->
  
            <!--<div class="bg-white rounded mt-8">
              <div class="p-4">
                <CommentSection :model="commentModel" />
              </div>
            </div>-->
  
            <!--<div v-if="faqItems.length > 0" class="bg-white rounded mt-8">
              <div class="p-4">
                <h2 class="text-lg">Frequently asked questions</h2>
                <p>These answers are taken from official FAQ or responses from the discord.</p>
                <div v-for="(item, index) in faqItems" :key="index" class="row faq-item">
                  <div class="col-12">
                    <p><b>Question: </b>{{ item.question }}</p>
                    <p><b>Answer: </b>{{ item.answer }}</p>
                  </div>
                </div>
              </div>
            </div>-->
          </div>
        </div>
      </div>
  
      <!--<div v-if="sections.length > 0">
        <div v-for="(cardSection, index) in sections" :key="index" class="container mb-8 px-4 md:px-8">
          <div class="row justify-center">
            <div class="col-12">
              <h2>{{ cardSection.title }}</h2>
            </div>
          </div>
          <div class="row">
            <div v-for="(image, imgIndex) in cardSection.images" :key="imgIndex" class="col-12 col-md-6">
              <img :src="image.url" />
            </div>
          </div>
        </div>
      </div>-->
    </div>
  </template>
  