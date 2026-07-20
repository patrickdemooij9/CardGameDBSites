<script setup lang="ts">
import type { ImageCropsApiModel } from "~/api/default";
import { GetCrop } from "~/helpers/CropUrlHelper";

/**
 * Thin wrapper around <NuxtImg> for CMS-sourced images.
 *
 * The backend exposes images in two shapes — an `ImageCropsApiModel` object
 * (default API) or a plain URL `string` — and this component normalizes both to
 * a URL before handing it to <NuxtImg>. The default `umbraco` image provider
 * (see nuxt.config.ts) then appends `?format=webp&...` so every image is served
 * as WebP.
 *
 * Drop-in behaviour: by default this renders like a plain <img> (single-density
 * srcset, so the browser uses the image's natural size and `max-width:100%`
 * layouts keep working). Pass `sizes` to opt into a responsive, DPR-aware
 * `srcset` — in that case give the element a width via CSS (e.g. `w-full`) so
 * the browser can pick the right candidate. Multi-density srcset with a fixed
 * natural size (the NuxtImg default) makes high-DPR browsers halve the intrinsic
 * width, which breaks `object-none`/fixed-size layouts — hence the guard below.
 *
 * Usage:
 *   <CmsImage :src="card.imageUrl" />                        (object shape)
 *   <CmsImage :src="card.imageUrl" crop="icon" width="80" /> (named crop)
 *   <CmsImage :src="logoUrl" alt="Logo" />                   (string shape)
 *   <CmsImage :src="card.imageUrl" sizes="50vw md:16vw" class="w-full" /> (responsive)
 *   <CmsImage :src="card.imageUrl">                           (with fallback)
 *     <template #fallback><div class="missing-card-image">…</div></template>
 *   </CmsImage>
 *
 * Other attrs (`width`, `height`, `class`, ...) fall through to <NuxtImg>. For
 * images that can't use this component (CSS background-image, HTML-string-
 * injected <img>), use GetWebpUrl() instead.
 */
const props = withDefaults(
  defineProps<{
    src: ImageCropsApiModel | string | undefined | null;
    crop?: string;
    alt?: string;
    loading?: "lazy" | "eager";
    /** Responsive sizes hint. When set, emits a DPR-aware width-based srcset. */
    sizes?: string;
  }>(),
  {
    crop: undefined,
    alt: "",
    loading: "lazy",
    sizes: undefined,
  }
);

const resolvedUrl = computed<string | undefined>(() => {
  const src = props.src;
  if (!src) {
    return undefined;
  }
  if (typeof src === "string") {
    return src || undefined;
  }
  return GetCrop(src, props.crop) ?? undefined;
});
</script>

<template>
  <NuxtImg
    v-if="resolvedUrl"
    :src="resolvedUrl"
    :alt="alt"
    :loading="loading"
    :sizes="sizes"
    :densities="sizes ? undefined : '1x'"
    format="webp"
  />
  <slot v-else name="fallback" />
</template>
