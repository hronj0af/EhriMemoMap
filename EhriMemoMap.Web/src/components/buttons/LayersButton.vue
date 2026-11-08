<script setup lang="ts">
// Components/Buttons/LayersButton.razor
import { useMapStore } from '../../stores/map'
import { DialogType, MapType } from '../../models/enums'
import { t } from '../../i18n'
import RzStack from '../ui/RzStack.vue'
import IconLayers from '../icons/IconLayers.vue'
import IconLayersMobile from '../icons/IconLayersMobile.vue'

const props = withDefaults(defineProps<{ mobileVersion?: boolean; border?: boolean; disabled?: boolean }>(), {
  mobileVersion: false,
  border: false,
  disabled: false,
})
const store = useMapStore()

async function showLayersList() {
  if (props.disabled) return
  await store.setDialog(store.dialogType === DialogType.Layers ? DialogType.None : DialogType.Layers)
  if (store.mapType !== MapType.Normal) await store.setMapType(MapType.Normal)
}
</script>

<template>
  <div
    v-if="!mobileVersion"
    :class="['icon', { 'icon-selected': store.dialogType === DialogType.Layers, 'icon-disabled': disabled }]"
    :title="disabled ? '' : t('layers') + '...'"
    @click="showLayersList"
  >
    <IconLayers :style="disabled ? 'opacity: 0.4;' : ''" />
  </div>
  <RzStack
    v-else
    orientation="horizontal"
    justify-content="center"
    :class="[
      store.dialogType === DialogType.Layers ? 'mobile-menu-icon-selected' : 'mobile-menu-icon',
      { 'mobile-menu-icon-border': border, 'mobile-menu-icon-disabled': disabled },
    ]"
    :title="disabled ? '' : t('layers') + '...'"
    @click="showLayersList"
  >
    <IconLayersMobile :style="disabled ? 'opacity: 0.4;' : ''" />
  </RzStack>
</template>
