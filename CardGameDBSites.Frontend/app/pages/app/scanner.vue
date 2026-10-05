<script setup lang="ts">
import CardScanner from "~/components/scanner/CardScanner.vue";

definePageMeta({ middleware: "native-only" });

useHead({ title: "Card scanner" });
</script>

<template>
  <CardScanner />
</template>
