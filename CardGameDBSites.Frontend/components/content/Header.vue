<script setup lang="ts">
import type { HeaderPropertiesModel } from '~/api/umbraco';
import { GetCropUrl } from '~/helpers/CropUrlHelper';

const props = defineProps<{
    content: HeaderPropertiesModel
}>();
</script>

<template>
    <div class="container relative mb-16 h-80">
        <figure
            class="absolute top-0 left-0 w-full h-full overflow-hidden after:bg-black/30 after:absolute after:w-full after:h-full after:top-0 after:left-0">
            <img class="h-full object-cover" .src="GetCropUrl(props.content.backgroundImage ?? [], 'Header')" />
        </figure>
        <div class="flex flex-col gap-4 items-center justify-center h-full z-10 relative">
            <form asp-controller="CardOverview" asp-action="search"
                class="flex overflow-hidden rounded border border-solid border-gray-300 bg-white sm:w-80">
                <input class="pl-4 py-2 grow" name="search" type="text" placeholder="Search cards..." />
                <button class="flex justify-center items-center w-8 text-lg px-3 bg-transparent" type="submit">
                    <i class="ph ph-magnifying-glass"></i>
                </button>
            </form>
            <div class="flex gap-2" v-if="props.content.searchLinks">
                <NuxtLink .to="link.url!" v-for="link in props.content.searchLinks"
                    class="rounded bg-main-color text-white py-2 px-4 no-underline shadow-lg hover:bg-main-color-hover">
                    {{ link.title }}
                </NuxtLink>
            </div>
        </div>
    </div>
</template>