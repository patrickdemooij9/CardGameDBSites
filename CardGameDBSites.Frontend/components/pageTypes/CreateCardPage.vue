<script setup lang="ts">
import { ref, watch, onMounted } from "vue";
import { DoServerFetch } from "~/helpers/RequestsHelper";

const mana = ref<string>("");
const name = ref<string>("");
const faction = ref<string>("Liothan");
const grouping = ref<string>("Ally");
const subtype = ref<string>("");
const ability = ref<string>("");
const attack = ref<string>("");
const health = ref<string>("");
const rarity = ref<number>(3);
const madeBy = ref<string>("");
const imageUrl = ref<string>("");

const previewBase64 = ref<string | null>(null);
const isRendering = ref(false);
const downloadError = ref<string | null>(null);

let debounceTimer: ReturnType<typeof setTimeout> | null = null;

function buildModel() {
  return {
    mana: mana.value !== "" ? parseInt(mana.value) : null,
    name: name.value || null,
    faction: faction.value || null,
    grouping: grouping.value || null,
    subtype: subtype.value || null,
    ability: ability.value || null,
    attack: attack.value !== "" ? parseInt(attack.value) : null,
    health: health.value !== "" ? parseInt(health.value) : null,
    rarity: rarity.value,
    madeBy: madeBy.value || null,
    imageUrl: imageUrl.value || null,
  };
}

async function updatePreview() {
  isRendering.value = true;
  try {
    const base64 = await DoServerFetch<string>("/api/cardmaker/render", true, {
      method: "POST",
      body: buildModel(),
    });
    previewBase64.value = base64;
  } finally {
    isRendering.value = false;
  }
}

function schedulePreviewUpdate() {
  if (debounceTimer !== null) {
    clearTimeout(debounceTimer);
  }
  debounceTimer = setTimeout(() => {
    updatePreview();
  }, 400);
}

async function downloadCard() {
  downloadError.value = null;
  try {
    const response = await fetch("/api/proxy/api/cardmaker/download", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(buildModel()),
    });
    if (!response.ok) {
      downloadError.value = "Failed to generate card. Please try again.";
      return;
    }
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "YourCard.png";
    a.click();
    URL.revokeObjectURL(url);
  } catch {
    downloadError.value = "An error occurred while downloading the card.";
  }
}

watch(
  [mana, name, faction, grouping, subtype, ability, attack, health, rarity, madeBy, imageUrl],
  () => {
    schedulePreviewUpdate();
  }
);

onMounted(() => {
  updatePreview();
});
</script>

<template>
  <div class="container px-4 md:px-8 mb-8">
    <div class="grid grid-cols-1 md:grid-cols-12 md:gap-x-8 gap-y-2">
      <div class="flex justify-center col-span-5">
        <div
          v-if="isRendering || !previewBase64"
          class="flex justify-center items-center mt-16 h-96 w-64 rounded-md border border-gray-500"
        >
          <span v-if="isRendering">Rendering...</span>
          <span v-else>Here will be your card!</span>
        </div>
        <img
          v-else
          class="mt-16 h-96 rounded-md max-w-full"
          :src="`data:image/png;base64,${previewBase64}`"
          alt="Card preview"
        />
      </div>

      <div class="col-span-7">
        <div class="grid grid-cols-12 gap-x-4 gap-y-2">
          <div class="col-span-full">
            <h1 class="text-lg">Create card</h1>
            <p><span class="text-gray-400">*</span> indicates required fields</p>
          </div>

          <div class="col-span-2">
            <label class="block font-bold" for="Mana"
              >Mana <span class="text-gray-400">*</span></label
            >
            <input
              id="Mana"
              v-model="mana"
              class="form-input w-12 h-12 rounded-full text-center"
              type="text"
            />
          </div>
          <div class="col-span-10">
            <label class="block font-bold" for="Name"
              >Name <span class="text-gray-400">*</span></label
            >
            <input
              id="Name"
              v-model="name"
              class="h-12 form-input w-full"
              type="text"
            />
          </div>

          <div class="col-span-4">
            <label class="block font-bold" for="Faction"
              >Faction <span class="text-gray-400">*</span></label
            >
            <select id="Faction" v-model="faction" class="h-8 rounded">
              <option value="Liothan">Liothan</option>
              <option value="Taulot">Taulot</option>
              <option value="Kurumo">Kurumo</option>
              <option value="Nupten">Nupten</option>
            </select>
          </div>
          <div class="col-span-4">
            <label class="block font-bold" for="Grouping"
              >Type <span class="text-gray-400">*</span></label
            >
            <select id="Grouping" v-model="grouping" class="h-8">
              <option value="Ally">Ally</option>
              <option value="Spell">Spell</option>
              <option value="Tower">Tower</option>
              <option value="Attachment">Attachment</option>
            </select>
          </div>
          <div class="col-span-4">
            <label class="block font-bold" for="Subtype">Subtype</label>
            <input
              id="Subtype"
              v-model="subtype"
              class="max-w-full form-input block"
              type="text"
            />
          </div>

          <div class="col-span-8">
            <label class="block font-bold" for="Ability">Ability</label>
            <textarea
              id="Ability"
              v-model="ability"
              class="h-24 w-full form-input resize-none"
            ></textarea>
            <small
              >Use |text| to bold text. [h+], [h-], [a+], [a-], [ar+], [ar-],
              [ta+], [ta-], [tar+], [tar-], [l] will be replaced with
              icons.</small
            >
          </div>

          <div class="col-span-4"></div>

          <div class="col-span-6">
            <label class="block font-bold" for="Attack">Attack</label>
            <input
              id="Attack"
              v-model="attack"
              class="form-input w-12 h-12 rounded-full text-center"
              type="text"
            />
          </div>
          <div class="col-span-4">
            <label class="block font-bold" for="Health">Health</label>
            <input
              id="Health"
              v-model="health"
              class="form-input w-12 h-12 rounded-full text-center"
              type="text"
            />
          </div>

          <div class="col-span-4">
            <label class="block font-bold" for="Rarity">Rarity</label>
            <select id="Rarity" v-model="rarity" class="h-8">
              <option :value="0">Base (3x)</option>
              <option :value="1">Rare (2x)</option>
              <option :value="2">Legendary (1x)</option>
              <option :value="3">Mythic (1x)</option>
            </select>
          </div>
          <div class="col-span-4">
            <label class="block font-bold" for="MadeBy">Made by</label>
            <input
              id="MadeBy"
              v-model="madeBy"
              class="max-w-full form-input"
              type="text"
            />
          </div>

          <div class="col-span-4">
            <label class="block font-bold" for="ImageUrl">Image URL</label>
            <input
              id="ImageUrl"
              v-model="imageUrl"
              class="form-input w-full"
              type="url"
            />
          </div>

          <div class="col-span-12 pt-4">
            <button
              type="button"
              class="btn btn-outline"
              @click="downloadCard"
            >
              Download
            </button>
            <p v-if="downloadError" class="mt-2 text-red-600">{{ downloadError }}</p>
            <p class="mt-4">Cards are NOT saved, keep your downloads!</p>

            <h1 class="mt-12 text-lg">Share your card</h1>
            <p>
              Post in the
              <a
                class="underline"
                href="https://boardgamegeek.com/thread/3266039/skytear-horde-player-made-cards"
                target="_blank"
                >BGG thread</a
              >
              or on
              <a
                class="underline"
                href="https://discord.gg/gS5WUqGYbE"
                target="_blank"
                >Discord</a
              >.
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
