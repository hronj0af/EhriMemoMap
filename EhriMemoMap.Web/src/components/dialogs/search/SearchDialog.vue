<script setup lang="ts">
// Components/Dialogs/Search/Search.razor – fulltextové hledání (Solr), výsledky seskupené podle typu.
import { computed, ref } from 'vue'
import { useMapStore } from '../../../stores/map'
import type { SolrPlace } from '../../../models/api'
import { fromSolrPlace } from '../../../map/leafletModel'
import { mapController } from '../../../map/controller'
import { formatDate } from '../../../models/helpers'
import { useI18n } from '../../../i18n'
import RzRow from '../../ui/RzRow.vue'
import RzColumn from '../../ui/RzColumn.vue'
import RzStack from '../../ui/RzStack.vue'
import RzButton from '../../ui/RzButton.vue'
import IconHelpSmall from '../../icons/IconHelpSmall.vue'

const store = useMapStore()
const { t, culture } = useI18n()

const openedTypes = ref<string[]>([])
const showLoading = ref(false)

/** Řazení jako LINQ OrderBy(string) – ordinal */
function compareKeys(a: string, b: string) {
  return a < b ? -1 : a > b ? 1 : 0
}

function groupBy<T>(items: T[], key: (item: T) => string): [string, T[]][] {
  const map = new Map<string, T[]>()
  for (const item of items) {
    const k = key(item)
    const group = map.get(k)
    if (group) group.push(item)
    else map.set(k, [item])
  }
  return [...map.entries()]
}

interface PlaceGroup {
  key: string
  places: SolrPlace[][]
}

const groups = computed<PlaceGroup[]>(() => {
  const places = (store.searchedPlaces ?? []).filter((a) => !!a.mapLocation)
  return groupBy(places, (a) => a.type ?? '')
    .sort(([a], [b]) => compareKeys(a, b))
    .map(([key, items]) => ({
      key,
      places: groupBy(items, (a) => a.labelCs ?? '')
        .sort(([a], [b]) => compareKeys(a, b))
        .map(([, group]) => group),
    }))
})

function distinctByPlaceCs(places: SolrPlace[]): SolrPlace[] {
  const seen = new Set<string | null>()
  return places.filter((p) => (seen.has(p.placeCs) ? false : (seen.add(p.placeCs), true)))
}

async function showPlace(places: SolrPlace[]) {
  if (places.length > 0) mapController().addObjects(places.map(fromSolrPlace), 'AdditionalObjects')
  else mapController().removeAdditionalObjects()

  const detailId = places[0]?.detailId ?? null
  const placeType = places[0]?.type ?? null
  if (placeType === 'Address' && detailId != null) await store.showVictim(detailId)
}

async function loadData() {
  store.searchedPlaces = null
  showLoading.value = true
  try {
    store.searchedPlaces = await store.getSolrPlaces({ query: store.searchedPlaceString, city: store.city })
  } catch (error) {
    console.error('Hledání selhalo', error)
    store.searchedPlaces = []
  } finally {
    showLoading.value = false
  }
  openedTypes.value = [...new Set((store.searchedPlaces ?? []).map((a) => a.type ?? ''))]
}

function togglePlaceType(placeType: string) {
  const i = openedTypes.value.indexOf(placeType)
  if (i >= 0) openedTypes.value.splice(i, 1)
  else openedTypes.value.push(placeType)
}
</script>

<template>
  <div class="rz-px-6 rz-pt-12">
    <h1 class="rz-pb-4" style="color: var(--Wine-Red)">
      {{ t('search') }}
      &nbsp;&nbsp;
      <span class="tooltip">
        <IconHelpSmall />
        <span class="tooltiptext tooltiptext-right">{{ t('help_search_' + store.city) }}</span>
      </span>
    </h1>
    <RzStack orientation="vertical" align-items="end">
      <input
        v-model="store.searchedPlaceString"
        class="rz-textbox"
        type="text"
        style="width: 100%; box-sizing: border-box"
        @change="loadData"
      />
      <RzButton button-style="primary" variant="outlined" @click="loadData">{{ t('search') }}</RzButton>
    </RzStack>

    <div v-if="showLoading" class="spinner"></div>

    <template v-if="store.searchedPlaces != null">
      <div class="rz-py-4" style="font-size: 18px; color: var(--Wine-Red); font-family: 'Barlow Bold'">
        {{ t('resultsFor') }}: "{{ store.searchedPlaceString }}"
      </div>
      <template v-for="group in groups" :key="group.key">
        <h2 class="rz-py-5" style="cursor: pointer; color: var(--Wine-Red)" @click="togglePlaceType(group.key)">
          {{ t('featureType_' + group.key) }}
          <img
            v-if="!openedTypes.includes(group.key)"
            style="height: 17px; cursor: pointer"
            class="rz-pl-3"
            src="images/arrow-right.png"
            :title="t('showStatistics') + '...'"
          />
          <img v-else style="width: 17px; cursor: pointer" class="rz-pl-2" src="images/arrow-down.png" :title="t('hideStatistics') + '...'" />
        </h2>
        <div :style="!openedTypes.includes(group.key) ? 'display:none' : ''">
          <RzRow
            v-for="(place, index) in group.places"
            :key="index"
            class="rz-pt-4"
            row-gap="0"
            style="cursor: pointer"
            @click="showPlace(place)"
          >
            <RzColumn :size="1">
              <img style="height: 25px" :src="'images/' + place[0]?.type + '-wine.svg'" />
            </RzColumn>
            <RzColumn :size="11">
              <h3 class="rz-mb-1">{{ culture === 'cs-CZ' ? place[0]?.labelCs : place[0]?.labelEn }}</h3>
              <h5 v-if="place[0]?.date">{{ formatDate(place[0].date) }}</h5>
              <div v-for="(address, i) in distinctByPlaceCs(place)" :key="i">
                <span v-if="culture === 'cs-CZ' && address.placeCs" style="font-size: 14px">{{ address.placeCs }}</span>
                <span v-else-if="address.placeEn" style="font-size: 14px">{{ address.placeEn }}</span>
                <template v-if="address.placeDe">
                  / <span style="font-size: 14px">{{ address.placeDe }}</span>
                </template>
              </div>
            </RzColumn>
          </RzRow>
        </div>
      </template>
    </template>
  </div>
</template>
