<script setup lang="ts">
import AppTabView from "~/components/navigation/AppTabView.vue";
import { useAppSection } from "~/composables/useAppSection";

definePageMeta({ middleware: "native-only" });

const { segments, title } = await useAppSection("/decks", "Decks");
useHead({ title });
</script>

<template>
  <AppTabView :segments="segments" />
</template>
