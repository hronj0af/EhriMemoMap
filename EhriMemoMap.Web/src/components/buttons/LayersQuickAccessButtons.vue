<script setup lang="ts">
// Components/Buttons/LayersQuickAccessButtons.razor – rychlé přepínače vrstev vlevo nahoře (desktop).
import { computed } from 'vue'
import { useMapStore } from '../../stores/map'
import { DialogType, FontColor, MapType } from '../../models/enums'
import type { LayerModel } from '../../models/map'
import { t } from '../../i18n'

const store = useMapStore()

const quickLayers = computed(() =>
  store
    .getNotBaseLayers()
    .filter((a) => a.quickAccess)
    .sort((a, b) => a.order - b.order),
)
const storyMaps = computed(() => store.map?.initialVariables?.storyMaps ?? false)
const visible = computed(() => quickLayers.value.length > 0 || storyMaps.value)
const storyMapSelected = computed(() => store.dialogType === DialogType.StoryMap)

function iconFilter(layer: LayerModel) {
  return layer.fontColor === FontColor.Dark
    ? 'filter: brightness(30%);max-width:22px;max-height:22px;width: auto;height: auto;'
    : 'max-width:22px;max-height:22px;width: auto;height: auto;'
}
function fontColor(layer: LayerModel) {
  return layer.fontColor === FontColor.Light ? 'var(--Beige-LIGHT)' : 'var(--Grey)'
}
</script>

<template>
  <div v-if="visible" style="position: absolute; top: 20px; left: 20px; z-index: 2000">
    <div style="display: flex; gap: 10px">
      <template v-if="store.mapType === MapType.Normal">
        <div
          v-for="item in quickLayers"
          :key="item.name ?? ''"
          :style="`cursor:pointer;background-color: ${item.backgroundColor}; color: ${fontColor(item)}; border-radius: 6px; order:${item.order}; padding: 0 5px 0 5px`"
          @click="store.toggleLayer(item)"
        >
          <div :style="(item.selected ? 'opacity:1' : 'opacity:0.6') + ';height: 38px;'" class="center">
            <img :style="iconFilter(item)" class="rz-px-1" :src="item.selected ? 'images/checkbox-true.svg' : 'images/checkbox-false.svg'" />
            <div class="rz-px-1" style="margin-bottom: 2px; font-size: 16px; font-family: Barlow; font-weight: 700">
              {{ t('layer_' + item.name + '_title_short') }}
            </div>
            <img class="rz-px-2" :style="iconFilter(item)" :src="'images/' + item.name + '.png'" />
          </div>
        </div>
      </template>
      <div
        v-if="storyMaps"
        style="cursor: pointer; background-color: var(--Wine-Red); color: white; border-radius: 6px; order: 999; padding: 0 5px 0 5px"
        @click="store.switchStoryMap()"
      >
        <div :style="(storyMapSelected ? 'opacity:1' : 'opacity:0.6') + ';height: 38px;'" class="center">
          <img
            style="max-width: 22px; max-height: 22px; width: auto; height: auto"
            class="rz-px-1"
            :src="storyMapSelected ? 'images/checkbox-true.svg' : 'images/checkbox-false.svg'"
          />
          <div class="rz-px-1" style="margin-bottom: 2px; font-size: 16px; font-family: Barlow; font-weight: 700">
            {{ t('storyMaps') }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
