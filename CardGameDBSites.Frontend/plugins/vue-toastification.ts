import Toast from "vue-toastification";
import * as vt from 'vue-toastification'
import "vue-toastification/dist/index.css";

export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.use(Toast, vt.default);
});
