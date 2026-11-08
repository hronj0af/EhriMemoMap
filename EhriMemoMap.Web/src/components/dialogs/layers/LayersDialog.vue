<script setup lang="ts">
// Components/Dialogs/Layers/Layers.razor – seznam vrstev s popisem a přepínačem.
import { computed } from 'vue'
import { useMapStore } from '../../../stores/map'
import { FontColor } from '../../../models/enums'
import type { LayerModel } from '../../../models/map'
import { t } from '../../../i18n'
import RzRow from '../../ui/RzRow.vue'
import RzColumn from '../../ui/RzColumn.vue'
import RzStack from '../../ui/RzStack.vue'
import RzCard from '../../ui/RzCard.vue'
import RzButton from '../../ui/RzButton.vue'

const store = useMapStore()

const layers = computed(() => [...store.getNotBaseLayers()].sort((a, b) => a.order - b.order))

function iconFilter(layer: LayerModel) {
  return layer.fontColor === FontColor.Dark ? 'filter: brightness(30%);height:24px;width:auto;' : 'height:24px;width:auto;'
}
function cardStyle(layer: LayerModel) {
  const fontColor = layer.fontColor === FontColor.Light ? 'var(--Beige-LIGHT)' : 'var(--Grey)'
  return `padding:5px;background-color:${layer.backgroundColor};color:${fontColor}`
}
/** <img onerror="this.remove()"> – ikona vrstvy nemusí existovat */
function removeImage(event: Event) {
  ;(event.target as HTMLElement).remove()
}
</script>

<template>
  <div v-if="layers.length > 0" class="rz-px-6 rz-pt-12" style="display: flex; gap: 10px; flex-direction: column">
    <h1 class="rz-pb-4" style="color: var(--Wine-Red)">{{ t('layers') }}</h1>
    <p>{{ t('layerHelp') }}</p>
    <RzButton
      v-if="store.appState === 'Development'"
      button-style="secondary"
      style="margin-bottom: 10px"
      @click="store.setShowLayersForce(!store.showLayersForce)"
    >
      {{ store.showLayersForce ? 'Skrýt vrstvy bez ohledu na zoom mapy' : 'Zobrazit vrstvy bez ohledu na zoom mapy' }}
    </RzButton>

    <RzCard v-for="item in layers" :key="item.name ?? ''" :style="cardStyle(item)" class="rz-p-3 layerInfo">
        <RzRow align-items="center" justify-content="start" row-gap="0">
          <RzColumn :size="1">
            <img
              :style="iconFilter(item) + 'cursor:pointer;'"
              :src="item.selected ? 'images/checkbox-true.svg' : 'images/checkbox-false.svg'"
              @click="store.toggleLayer(item)"
            />
          </RzColumn>
          <RzColumn :size="11">
            <RzStack justify-content="space-between" orientation="horizontal">
              <div style="font-size: 14px; font-family: 'Barlow'; font-weight: 700">
                {{ t('layer_' + item.name + '_title_' + store.city) }}
              </div>
              <img :style="iconFilter(item)" :src="'images/' + item.name + '.svg'" @error="removeImage" />
            </RzStack>
          </RzColumn>
        </RzRow>
        <RzRow justify-content="start" row-gap="0">
          <RzColumn :size="11" :offset="1">
            <div class="layerInfoInner">
              <div style="font-size: 12px; font-family: 'Noto Serif'">
                {{ t('layer_' + item.name + '_abstract_' + store.city) }}
              </div>
            </div>
          </RzColumn>
        </RzRow>
    </RzCard>
  </div>
</template>
