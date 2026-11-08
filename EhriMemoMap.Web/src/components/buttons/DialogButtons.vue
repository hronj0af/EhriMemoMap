<script setup lang="ts">
// Components/Buttons/DialogButtons.razor – zpět / celá obrazovka (mobil) / zavřít v hlavičce panelu.
import { computed } from 'vue'
import { useMapStore } from '../../stores/map'
import { DialogType, MapType } from '../../models/enums'
import { mapController } from '../../map/controller'
import RzRow from '../ui/RzRow.vue'
import RzStack from '../ui/RzStack.vue'
import RzButton from '../ui/RzButton.vue'

const store = useMapStore()

const buttonStyle = computed(
  () =>
    'z-index:2000;position:fixed;background-color:var(--Beige-LIGHT);width:' +
    (store.isMobileView ? '100%' : store.widthOfDialogPercent),
)

async function onClose() {
  if (store.dialogType === DialogType.StoryMap) await store.setMapType(MapType.Normal)
  store.isDialogFullScreen = false
  await store.setDialog(DialogType.None)
}

function onFullscreen() {
  store.isDialogFullScreen = !store.isDialogFullScreen
  mapController().setDialogFullScreen(store.isDialogFullScreen)
}

async function onBack() {
  await store.setLastDialog()
}
</script>

<template>
  <RzRow justify-content="space-between" :style="buttonStyle">
    <RzStack orientation="horizontal">
      <RzButton v-if="store.dialogHistory.length > 1" icon="arrow_back" button-style="light" variant="flat" @click="onBack" />
    </RzStack>
    <RzStack orientation="horizontal">
      <RzButton
        v-if="store.isMobileView && store.dialogType !== DialogType.Help"
        icon="fullscreen"
        button-style="light"
        variant="flat"
        @click="onFullscreen"
      />
      <RzButton icon="close" button-style="light" variant="flat" @click="onClose" />
    </RzStack>
  </RzRow>
</template>
