<script setup lang="ts">
// Components/Dialogs/Place/Place.razor – detail míst, na která uživatel klikl (víc typů → záložky).
import { computed, nextTick, onUpdated, shallowRef, watch } from 'vue'
import { useMapStore } from '../../../stores/map'
import { DialogType, PlaceType } from '../../../models/enums'
import type { PlacesParameters, PlacesResult } from '../../../models/api'
import { t } from '../../../i18n'
import { resizePhotoHeight } from '../../../utils/dom'
import RzTabs from '../../ui/RzTabs.vue'
import PlaceContentRouter from './PlaceContentRouter.vue'

const store = useMapStore()
const placesToShow = shallowRef<PlacesResult | null>(null)

/** Typy vybraných míst v pořadí výskytu (GroupBy(a => a.PlaceType)) */
const placeTypes = computed(() => {
  const types: string[] = []
  for (const place of store.dialogParameters.places ?? []) {
    if (place.placeType && !types.includes(place.placeType)) types.push(place.placeType)
  }
  return types
})

let requestId = 0
async function fetchData() {
  const places = store.dialogParameters.places
  if (store.dialogType !== DialogType.Place || !places) return

  placesToShow.value = null
  const ids = (type: string) => {
    const selected = places.filter((a) => a.placeType === type).map((a) => Number(a.id ?? '0'))
    return selected.length > 0 ? selected : undefined
  }
  const parameters: PlacesParameters = {
    city: store.city,
    addressesLastResidenceIds: ids(PlaceType.AddressLastResidence),
    incidentsIds: ids(PlaceType.Incident),
    placesOfInterestIds: ids(PlaceType.Interest),
    inaccessiblePlacesIds: ids(PlaceType.Inaccessible),
    addressesIds: ids(PlaceType.Address),
    placesOfMemoryIds: ids(PlaceType.Memory),
    memorialsIds: ids(PlaceType.Memorial),
  }

  const current = ++requestId
  try {
    const result = await store.getPlaces(parameters)
    if (current === requestId) placesToShow.value = result
  } catch (error) {
    console.error('Načtení detailu míst selhalo', error)
  }
}

watch(() => store.dialogParameters, fetchData, { immediate: true })

onUpdated(() => void nextTick(resizePhotoHeight))
</script>

<template>
  <div class="rz-pt-12">
    <template v-if="placesToShow">
      <div v-if="placeTypes.length === 1" style="padding: 0 2.5rem 0 2.5rem">
        <h2 class="rz-mb-6" style="color: var(--Wine-Red)">{{ t('featureType_' + placeTypes[0]) }}</h2>
        <PlaceContentRouter :place-type="placeTypes[0]" :places-to-show="placesToShow" />
      </div>
      <div v-else class="rz-pt-6">
        <RzTabs :titles="placeTypes.map((type) => t('featureType_' + type))">
          <template #default="{ index }">
            <div style="padding: 1.5rem">
              <PlaceContentRouter :place-type="placeTypes[index]" :places-to-show="placesToShow" />
            </div>
          </template>
        </RzTabs>
      </div>
    </template>
  </div>
</template>
