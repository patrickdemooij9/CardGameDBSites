import type { DirectiveBinding } from "vue";
import type { ImageCropsApiModel } from "~/api/default";
import { GetCrop } from "~/helpers/CropUrlHelper";

type CursorImageSource = ImageCropsApiModel | string | undefined | null;

// 2x the #cursor-image element's w-56 so it stays sharp on high-DPR screens.
const CURSOR_IMAGE_WIDTH = 448;

export default defineNuxtPlugin((nuxtApp) => {
  const img = useImage();
  const resolvedUrls = new WeakMap<HTMLElement, string | undefined>();

  function resolveUrl(binding: DirectiveBinding<CursorImageSource>) {
    const src = binding.value;
    const url = typeof src === "string" ? src : GetCrop(src ?? undefined, binding.arg);
    return url ? img(url, { width: CURSOR_IMAGE_WIDTH, format: "webp" }) : undefined;
  }

  nuxtApp.vueApp.directive<HTMLElement, CursorImageSource>("cursor-image", {
    updated(el, binding) {
      resolvedUrls.set(el, resolveUrl(binding));
    },
    async beforeMount(el, binding) {
      resolvedUrls.set(el, resolveUrl(binding));
      await nextTick();

      const cursorImageElem = document.querySelector(
        "#cursor-image"
      ) as HTMLElement;
      if (!cursorImageElem) {
        return;
      }

      el.addEventListener("mouseover", (_event: MouseEvent) => {
        const imageUrl = resolvedUrls.get(el);
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
