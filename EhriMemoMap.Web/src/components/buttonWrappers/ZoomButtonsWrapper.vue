<script setup lang="ts">
// Components/ButtonWrappers/ZoomButtonsWrapper.razor – moje poloha, přiblížit, oddálit.
import { computed, ref } from 'vue'
import { useMapStore } from '../../stores/map'
import { MapType } from '../../models/enums'
import { mapController } from '../../map/controller'
import { t } from '../../i18n'
import RzRow from '../ui/RzRow.vue'
import RzColumn from '../ui/RzColumn.vue'
import IconMyLocation from '../icons/IconMyLocation.vue'
import IconZoomIn from '../icons/IconZoomIn.vue'
import IconZoomOut from '../icons/IconZoomOut.vue'

const store = useMapStore()
const myLocationIsVisible = ref(false)

const visible = computed(() => !(store.isMobileView && store.mapType !== MapType.Normal && store.mapType !== MapType.Trajectory))
const size = computed(() => (store.isMobileView ? 4 : 12))
const style = computed(() =>
  store.isMobileView
    ? 'position:absolute;top:54px;left:10px;z-index:1002;'
    : 'position:absolute;bottom:34px;left:15px;z-index:1002;width:0px',
)

function onLocationClick() {
  if (!myLocationIsVisible.value) {
    myLocationIsVisible.value = true
    mapController().turnOnLocationTracking()
  } else {
    mapController().goToMyLocation()
  }
}
</script>

<template>
  <div v-if="visible" :style="style">
    <RzRow gap="5" row-gap="5px" :justify-content="store.isMobileView ? 'space-around' : 'end'">
      <RzColumn :size="size">
        <div class="icon" :title="(myLocationIsVisible ? t('showMyLocation') : t('myLocationOn')) + '...'" @click="onLocationClick">
          <IconMyLocation />
        </div>
      </RzColumn>
      <RzColumn :size="size">
        <div class="icon" :title="t('zoomIn') + ' ...'" @click="mapController().zoomIn()">
          <IconZoomIn />
        </div>
      </RzColumn>
      <RzColumn :size="size">
        <div class="icon" :title="t('zoomOut') + '...'" @click="mapController().zoomOut()">
          <IconZoomOut />
        </div>
      </RzColumn>
    </RzRow>
  </div>
</template>
