import { ColorModeCookie, useColorMode } from "~/composables/useColorMode";

// SWR-cached pages are rendered without the visitor's cookie, so the class is decided here before first paint.
const applyColorModeScript = `(function(){try{var m=document.cookie.match(/(?:^|; )${ColorModeCookie}=([^;]*)/);var p=m?decodeURIComponent(m[1]):'system';document.documentElement.classList.toggle('dark',p==='dark'||(p!=='light'&&matchMedia('(prefers-color-scheme: dark)').matches))}catch(e){}})()`;

export default defineNuxtPlugin(() => {
  const { isDark, systemPrefersDark } = useColorMode();

  useHead({
    htmlAttrs: { class: computed(() => (isDark.value ? "dark" : "")) },
    script: [{ innerHTML: applyColorModeScript, tagPosition: "head" }],
  });

  if (import.meta.server) {
    return;
  }

  const query = window.matchMedia("(prefers-color-scheme: dark)");
  systemPrefersDark.value = query.matches;
  query.addEventListener("change", (event) => {
    systemPrefersDark.value = event.matches;
  });
});
