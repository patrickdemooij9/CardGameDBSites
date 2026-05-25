export default defineNuxtPlugin((nuxtApp) => {
  nuxtApp.vueApp.directive("cursor-image", {
    async beforeMount(el: HTMLElement, binding) {
      await nextTick();

      const cursorImageElem = document.querySelector(
        "#cursor-image"
      ) as HTMLElement;
      if (!cursorImageElem) {
        return;
      }

      const cursorImageElemRect = cursorImageElem.getBoundingClientRect();

      el.addEventListener("mouseover", (_event: MouseEvent) => {
        const imageUrl = binding.value;
        if (imageUrl) {
          cursorImageElem.style.backgroundImage = `url(${imageUrl})`;
          cursorImageElem.style.display = "block";
        }

        window.addEventListener("mousemove", updateCursorImageLocation);
      });
      el.addEventListener("mouseleave", () => {
        window.removeEventListener("mousemove", updateCursorImageLocation);
        cursorImageElem.style.backgroundImage = "";
        cursorImageElem.style.display = "none";
      });

      function updateCursorImageLocation(event: MouseEvent) {
        const rect = cursorImageElem.getBoundingClientRect();

        if (rect.height + event.clientY >= window.innerHeight) {
          cursorImageElem.style.top = `${
            event.clientY - rect.height + 20
          }px`;
        } else {
          cursorImageElem.style.top = `${event.clientY - 20}px`;
        }

        if (
          rect.width + event.clientX + 40 >=
          window.innerWidth
        ) {
          cursorImageElem.style.left = `${
            event.clientX - rect.width - 20
          }px`;
        } else {
          cursorImageElem.style.left = `${event.clientX + 20}px`;
        }
      }
    },
  });
});
