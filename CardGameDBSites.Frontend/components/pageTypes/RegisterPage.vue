<script setup lang="ts">
import type { RegisterContentModel } from "~/api/umbraco";
import Button from "../shared/Button.vue";

defineProps<{
  content: RegisterContentModel;
}>();

const registerModel = ref({
  email: "",
  displayName: "",
  password: "",
  termsAndConditions: false,
});
const errorMessage = ref("");

async function submit() {
  const recaptcha = await getRecaptchaToken();
  useAccountStore()
    .register({
      email: registerModel.value.email,
      userName: registerModel.value.displayName,
      password: registerModel.value.password,
      recaptcha: recaptcha,
      termAndConditions: registerModel.value.termsAndConditions,
    })
    .then(() => {
      // Redirect to the homepage or another page after successful login
      useRouter().push("/");
    })
    .catch((error) => {
      // Handle error, e.g., show a notification
      console.log(error);
      errorMessage.value = error || "Register failed. Please try again.";
    });
}

const recaptchaSiteKey = "6LfoAPIjAAAAAO0AyBgnvhpCHgEWTnTneKbGcyk0";

function loadRecaptchaScript() {
  if (document.getElementById("recaptcha-script")) return;
  const script = document.createElement("script");
  script.id = "recaptcha-script";
  script.src = `https://www.google.com/recaptcha/api.js?render=${recaptchaSiteKey}`;
  script.async = true;
  document.head.appendChild(script);
}

async function getRecaptchaToken() {
  // @ts-ignore
  await new Promise((resolve) => {
    // @ts-ignore
    if (window.grecaptcha && window.grecaptcha.ready) {
      // @ts-ignore
      window.grecaptcha.ready(resolve);
    } else {
      // Wait for script to load
      const check = setInterval(() => {
        // @ts-ignore
        if (window.grecaptcha && window.grecaptcha.ready) {
          clearInterval(check);
          // @ts-ignore
          window.grecaptcha.ready(resolve);
        }
      }, 100);
    }
  });
  // @ts-ignore
  return window.grecaptcha.execute(recaptchaSiteKey, { action: "submit" });
}

onMounted(() => {
  loadRecaptchaScript();
});
</script>

<template>
  <div
    class="flex items-center justify-center container px-4 md:px-8 full-without-header"
  >
    <form class="h-min md:w-1/3" @submit.prevent="submit">
      <h1 class="text-base mb-4">Create an account</h1>
      <div class="flex flex-col gap-2 mb-4">
        <label for="email">Email</label>
        <input
          type="email"
          name="email"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          required
          v-model="registerModel.email"
        />
      </div>

      <div class="flex flex-col gap-2 mb-4">
        <label for="displayName">Display name</label>
        <input
          type="text"
          name="displayName"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          required
          v-model="registerModel.displayName"
        />
      </div>

      <div class="flex flex-col gap-2 mb-4">
        <label for="password">Password</label>
        <input
          type="password"
          name="password"
          class="px-3 py-1 rounded border border-gray-300 w-full"
          required
          v-model="registerModel.password"
        />
      </div>

      <div class="mb-4">
        <label class="flex items-center gap-2">
          <input
            type="checkbox"
            name="termsAndConditions"
            required
            v-model="registerModel.termsAndConditions"
          />
          <span>I agree with the terms and conditions</span>
        </label>
      </div>

      <div v-if="errorMessage" class="text-red-500 mb-4">
        {{ errorMessage }}
      </div>

      <div class="flex justify-between items-center">
        <Button type="submit">Register</Button>

        <div class="flex gap-2">
          <NuxtLink to="/login" class="no-underline hover:text-gray-500"> To login </NuxtLink>
          <NuxtLink to="/forgot-password" class="no-underline hover:text-gray-500">
            Forgot password?
          </NuxtLink>
        </div>
      </div>
    </form>
  </div>
</template>
