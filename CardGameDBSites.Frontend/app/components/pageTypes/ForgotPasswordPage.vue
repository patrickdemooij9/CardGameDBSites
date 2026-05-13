<script setup lang="ts">
import type { ForgotPasswordContentModel } from "~/api/umbraco";
import Button from "../shared/Button.vue";
import { useAccountStore } from "~/stores/AccountStore";

defineProps<{
  content: ForgotPasswordContentModel;
}>();

const model = ref({
  email: "",
  newPassword: "",
});
const route = useRoute();
const code = computed(() => {
  const queryCode = route.query.code;
  if (Array.isArray(queryCode)) {
    return queryCode[0] || "";
  }

  return queryCode || "";
});
const isResetMode = computed(() => !!code.value);
const successPostModel = ref(false);
const successResetModel = ref(false);
const errorMessage = ref("");

async function submitForgotPassword() {
  useAccountStore()
    .forgotPassword(model.value.email)
    .then(() => {
      successPostModel.value = true;
    })
    .catch((error) => {
      console.log(error);
      errorMessage.value = error?.data || "Could not send password reset email. Please try again.";
    });
}

async function submitResetPassword() {
  useAccountStore()
    .resetPassword(code.value, model.value.newPassword)
    .then(() => {
      successResetModel.value = true;
    })
    .catch((error) => {
      console.log(error);
      errorMessage.value = error?.data || "Could not reset password. Please try again.";
    });
}
</script>

<template>
  <div
    class="flex items-center justify-center container px-4 md:px-8 full-without-header"
  >
    <form
      v-if="!successResetModel && !successPostModel && !isResetMode"
      class="h-min md:w-1/3"
      @submit.prevent="submitForgotPassword"
    >
      <h1 class="text-base mb-4">Forgot your password?</h1>
      <div class="flex flex-col gap-2 mb-4">
        <label for="email">Email</label>
        <input
          type="email"
          name="email"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          required
          v-model="model.email"
        />
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

    <form
      v-else-if="!successResetModel && isResetMode"
      class="h-min md:w-1/3"
      @submit.prevent="submitResetPassword"
    >
      <h1 class="text-base mb-4">Reset your password</h1>
      <div class="flex flex-col gap-2 mb-4">
        <label for="newPassword">New password</label>
        <input
          type="password"
          name="newPassword"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          required
          v-model="model.newPassword"
        />
      </div>

      <div v-if="errorMessage" class="text-red-500 mb-4">
        {{ errorMessage }}
      </div>

      <div class="flex justify-between items-center">
        <Button type="submit">Reset password</Button>

        <div class="flex gap-2">
          <NuxtLink to="/login" class="no-underline hover:text-gray-500"> To login </NuxtLink>
          <NuxtLink to="/register" class="no-underline hover:text-gray-500">
            To register
          </NuxtLink>
        </div>
      </div>
    </form>

    <div v-else class="h-min md:w-1/3">
      <p v-if="successResetModel">Your password has successfully been reset!</p>
      <p v-else>An email has been sent to the email specified. Use this email to reset your password.</p>
    </div>
  </div>
</template>
