<script setup lang="ts">
// Components/ButtonWrappers/ControlButtonsWrapper.razor
// Desktop: svislý sloupec tlačítek vpravo nahoře (posouvá se vlevo od otevřeného panelu).
// Mobil: lišta tlačítek nad spodním panelem (nebo dole, když je panel zavřený).
import { computed } from 'vue'
import { useMapStore } from '../../stores/map'
import { DialogType, MapState, MapType } from '../../models/enums'
import RzRow from '../ui/RzRow.vue'
import RzColumn from '../ui/RzColumn.vue'
import HomeButton from '../buttons/HomeButton.vue'
import SearchButton from '../buttons/SearchButton.vue'
import LayersButton from '../buttons/LayersButton.vue'
import StoryMapButton from '../buttons/StoryMapButton.vue'
import HelpButton from '../buttons/HelpButton.vue'
import JoinButton from '../buttons/JoinButton.vue'
import LanguageButton from '../buttons/LanguageButton.vue'
import TimelineButton from '../buttons/TimelineButton.vue'

const store = useMapStore()

const showStoryMapButton = computed(() => store.map?.initialVariables?.storyMaps ?? false)
const showTimelineButton = computed(() => store.timeline != null && store.mapType === MapType.Normal)

/** GetSize – mobilní lišta: 12 sloupců rozdělených mezi viditelná tlačítka */
const size = computed(() => {
  let columns = 2
  if (showStoryMapButton.value) columns++
  if (showTimelineButton.value) columns++
  return Math.floor(12 / columns)
})

/** GetStyle */
const style = computed(() => {
  if (store.isMobileView) {
    if (store.isDialogFullScreen) return 'display:none;'
    const dialogOpen = store.dialogType !== DialogType.None && store.dialogType !== DialogType.Help
    // u spodního okraje: šedé pozadí sahá až dolů, tlačítka zůstanou nad systémovým pruhem gest
    const bottom = dialogOpen ? `bottom:${store.heightOfDialog}%` : 'bottom:0px;padding-bottom:env(safe-area-inset-bottom, 0px)'
    // fixed (ne absolute): mobilní prohlížeče při schování adresního řádku nezvětší blok, ke kterému se
    // vztahuje absolutní pozice – lišta by zůstala výš a pod ní by byla mezera o výšce adresního řádku.
    // Fixed se drží viditelného spodního okraje stejně jako spodní panel.
    return `position:fixed;${bottom};z-index:10005;background-color:var(--Grey);width:100%;`
  }
  const right = store.dialogType === DialogType.None ? '0px' : store.widthOfDialogPercent
  return `position:absolute;top:30px;right:${right};z-index:1005;width:47px`
})

const visible = computed(() => store.mapState === MapState.Rendered && store.map?.initialVariables != null)
</script>

<template>
  <div v-if="visible" id="controlButtonsWrapper" :style="style">
    <RzRow v-if="!store.isMobileView" gap="0" row-gap="5px" justify-content="end" align-items="center">
      <RzColumn :size="12"><HomeButton /></RzColumn>
      <RzColumn :size="12"><SearchButton :disabled="store.mapType !== MapType.Normal" /></RzColumn>
      <RzColumn :size="12"><LayersButton :disabled="store.mapType !== MapType.Normal" /></RzColumn>
      <RzColumn v-if="showStoryMapButton" :size="12"><StoryMapButton /></RzColumn>
      <RzColumn :size="12"><HelpButton /></RzColumn>
      <RzColumn :size="12"><JoinButton /></RzColumn>
      <RzColumn :size="12"><LanguageButton /></RzColumn>
    </RzRow>
    <RzRow v-else gap="0" justify-content="center" align-items="center">
      <RzColumn :size="size"><SearchButton mobile-version border /></RzColumn>
      <RzColumn v-if="showTimelineButton" :size="size"><TimelineButton mobile-version border /></RzColumn>
      <RzColumn v-if="showStoryMapButton" :size="size"><StoryMapButton mobile-version border /></RzColumn>
      <RzColumn :size="size"><LayersButton mobile-version /></RzColumn>
    </RzRow>
  </div>
</template>
