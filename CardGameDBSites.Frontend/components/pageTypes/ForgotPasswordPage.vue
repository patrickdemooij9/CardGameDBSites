<script setup lang="ts">
import type { ForgotPasswordContentModel } from "~/api/umbraco";
import Button from "../shared/Button.vue";

defineProps<{
  content: ForgotPasswordContentModel;
}>();

const model = ref({
  email: "",
});
const success = ref(false);
const errorMessage = ref("");

async function submit() {
  useAccountStore()
    .forgotPassword(model.value.email)
    .then(() => {
      success.value = true;
    })
    .catch((error) => {
      // Handle error, e.g., show a notification
      console.log(error);
      errorMessage.value = error || "Register failed. Please try again.";
    });
}
</script>

<template>
  <div
    class="flex items-center justify-center container px-4 md:px-8 full-without-header"
  >
    <form class="h-min md:w-1/3" @submit.prevent="submit">
      <h1 class="text-base mb-4">Forgot your password?</h1>
      <div class="flex flex-col gap-2 mb-4" v-if="!success">
        <label for="email">Email</label>
        <input
          type="email"
          name="email"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          required
          v-model="model.email"
        />
      </div>
      <div v-else>
        <p>An email has been sent to the email specified. Use this email to reset your password.</p>
      </div>

      <div v-if="errorMessage" class="text-red-500 mb-4">
        {{ errorMessage }}
      </div>

      <div class="flex justify-between items-center">
        <Button type="submit">Submit</Button>

        <div class="flex gap-2">
          <NuxtLink to="/login" class="no-underline hover:text-gray-500"> To login </NuxtLink>
          <NuxtLink to="/register" class="no-underline hover:text-gray-500">
            To register
          </NuxtLink>
        </div>
      </div>
    </form>
  </div>
</template>
