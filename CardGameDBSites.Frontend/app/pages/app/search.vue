<script setup lang="ts">
import AppTabView from "~/components/navigation/AppTabView.vue";
import { useAppSection } from "~/composables/useAppSection";

definePageMeta({ middleware: "native-only" });

const { segments, title } = await useAppSection("/cards", "Search");
useHead({ title });
</script>

<template>
  <AppTabView :segments="segments" />
</template>
