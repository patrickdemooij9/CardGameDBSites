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
        if (cursorImageElemRect.height + event.clientY >= window.innerHeight) {
          cursorImageElem.style.top = `${
            event.pageY - cursorImageElemRect.height
          }px`;
        } else {
          cursorImageElem.style.top = `${event.pageY - 20}px`;
        }

        if (
          cursorImageElemRect.width + event.clientX + 40 >=
          window.innerWidth
        ) {
          cursorImageElem.style.left = `${
            event.pageX - cursorImageElemRect.width - 20
          }px`;
        } else {
          cursorImageElem.style.left = `${event.pageX + 20}px`;
        }
      }
    },
  });
});
