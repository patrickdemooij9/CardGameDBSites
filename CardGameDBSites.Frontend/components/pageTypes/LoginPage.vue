<script setup lang="ts">
import type { LoginContentModel } from "~/api/umbraco";
import Button from "../shared/Button.vue";

defineProps<{
  content: LoginContentModel;
}>();

const loginModel = ref({
  email: "",
  password: "",
  rememberMe: false,
});
const errorMessage = ref("");

function submit() {
  useAccountStore()
    .login(
      loginModel.value.email,
      loginModel.value.password,
      loginModel.value.rememberMe
    )
    .then(() => {
      // Redirect to the homepage or another page after successful login
      useRouter().push("/");
    })
    .catch((error) => {
      // Handle error, e.g., show a notification
      console.log(error);
      errorMessage.value = error || "Login failed. Please try again.";
    });
}
</script>

<template>
  <div
    class="flex items-center justify-center container px-4 md:px-8 full-without-header"
  >
    <form class="h-min md:w-1/3" @submit.prevent="submit">
      <h1 class="text-base mb-4">Sign in</h1>
      <div class="flex flex-col gap-2 mb-4">
        <label for="email">Email</label>
        <input
          type="email"
          name="email"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          required
          v-model="loginModel.email"
        />
      </div>

      <div class="flex flex-col gap-2 mb-4">
        <label for="password">Password</label>
        <input
          type="password"
          name="password"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          required
          v-model="loginModel.password"
        />
      </div>

      <div class="mb-4">
        <label class="flex items-center gap-2">
          <input
            type="checkbox"
            name="rememberMe"
            v-model="loginModel.rememberMe"
          />
          <span>Remember me</span>
        </label>
      </div>

      <div v-if="errorMessage" class="text-red-500 mb-4">
        {{ errorMessage }}
      </div>

      <div class="flex justify-between items-center">
        <Button type="submit">Login</Button>

        <div class="flex gap-2">
          <NuxtLink to="/register" class="no-underline hover:text-gray-500"> Register </NuxtLink>
          <NuxtLink to="/forgot-password" class="no-underline hover:text-gray-500">
            Forgot password?
          </NuxtLink>
        </div>
      </div>
    </form>
  </div>
</template>
