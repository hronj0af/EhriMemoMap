<script setup lang="ts">
// Components/Dialogs/Welcome/StatisticsTable.razor – počty míst v úvodním dialogu.
import { computed, onMounted, shallowRef } from 'vue'
import { useMapStore } from '../../../stores/map'
import type { WelcomeDialogStatistics } from '../../../models/api'
import { t } from '../../../i18n'
import RzSpinner from '../../ui/RzSpinner.vue'

const store = useMapStore()
const statistics = shallowRef<WelcomeDialogStatistics | null>(null)
const loaded = shallowRef(false)

onMounted(async () => {
  try {
    statistics.value = await store.getWelcomeDialogStatistics()
  } catch (error) {
    console.error('Načtení statistik selhalo', error)
  } finally {
    loaded.value = true
  }
})

interface Row {
  value: number | null | undefined
  icon: string
  iconWidth: number
  iconCellStyle?: string
  label: string
  help: string
}

const rows = computed<Row[]>(() => {
  const s = statistics.value
  if (!s) return []
  const city = store.city
  return [
    { value: s.victims, icon: 'images/home-welcome.svg', iconWidth: 28, label: 'countJews', help: 'help_victims_' + city },
    { value: s.incidents, icon: 'images/incident-welcome.svg', iconWidth: 28, label: 'countIncidents', help: 'help_incident_' + city },
    { value: s.interests, icon: 'images/interest-welcome.svg', iconWidth: 28, label: 'countPointsOfInterest', help: 'help_interest_' + city },
    { value: s.inaccessibles, icon: 'images/inaccessible-welcome.svg', iconWidth: 28, label: 'countInaccessiblePlaces', help: 'help_inaccessible_' + city },
    { value: s.placesOfMemory, icon: 'images/memory-welcome.svg', iconWidth: 28, label: 'countPlacesOfMemory', help: 'help_memory_' + city },
    { value: s.memorials, icon: 'images/memorial-welcome.svg', iconWidth: 28, label: 'countMemorials', help: 'help_memorials_' + city },
    {
      value: s.storyMaps,
      icon: 'images/storymap-icon.png',
      iconWidth: 20,
      iconCellStyle: 'text-align:center',
      label: 'countStoryMaps',
      help: 'help_storymap_' + city,
    },
  ].filter((r) => r.value != null && r.value > 0)
})
</script>

<template>
  <RzSpinner v-if="!loaded" />
  <table v-else style="padding-bottom: 15px">
    <tr v-for="row in rows" :key="row.label">
      <td class="cell firstCell" :style="row.iconCellStyle">
        <img :src="row.icon" :width="row.iconWidth" />
      </td>
      <td class="cell secondCell">
        {{ t(row.label) }}
        <span class="tooltip">
          <img src="/images/help-small.svg" class="icon-help" />
          <span class="tooltiptext tooltiptext-top">{{ t(row.help) }}</span>
        </span>
      </td>
      <td class="cell thirdCell">{{ row.value }}</td>
    </tr>
  </table>
</template>
