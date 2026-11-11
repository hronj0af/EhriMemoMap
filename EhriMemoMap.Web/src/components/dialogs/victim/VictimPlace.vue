<script setup lang="ts">
// Components/Dialogs/Victim/VictimPlace.razor – řádek tabulky s místem daného typu (narození, bydliště…).
import { computed } from 'vue'
import { useMapStore } from '../../../stores/map'
import {
  addressText,
  addressWithDates,
  firstLetterToUpper,
  residenceYears,
  sortResidencesChronologically,
} from '../../../models/helpers'
import { useI18n } from '../../../i18n'

const props = withDefaults(defineProps<{ type: number; resourceNameType?: string; showDates?: boolean }>(), {
  resourceNameType: undefined,
  showDates: true,
})
const store = useMapStore()
const { t, byLang, culture } = useI18n()

/** Typ vztahu „bydliště“ (relationship_types 36) */
const RESIDENCE = 36

const places = computed(() => (store.victimLongInfo?.places ?? []).filter((a) => a.type === props.type))

/**
 * Bydliště chronologicky, každé na vlastním řádku, s roky v závorce – na mapách s osobními trajektoriemi
 * (Velké Meziříčí), kde mají osoby hodně adres.
 */
const chronological = computed(() => (store.map?.initialVariables?.personalTrajectories ?? false) && props.type === RESIDENCE)
const chronologicalPlaces = computed(() => sortResidencesChronologically(places.value))

const nameType = computed(() => {
  if (places.value.length === 0) return ''
  if (props.resourceNameType) return t(props.resourceNameType)
  return firstLetterToUpper(byLang(places.value[0].typeCs, places.value[0].typeEn))
})

const multipleText = computed(() =>
  [...places.value]
    .sort((a, b) => a.id - b.id)
    .map((a) => addressWithDates(a, culture.value))
    .join(', '),
)
</script>

<template>
  <template v-if="chronological && places.length > 0">
    <tr>
      <td>{{ places.length === 1 ? nameType : firstLetterToUpper(byLang(places[0].typeCs, places[0].typeEn)) }}</td>
      <td>
        <template v-for="(place, i) in chronologicalPlaces" :key="place.id">
          {{ addressText(place, culture) }} {{ residenceYears(place.dateFrom, place.dateTo)
          }}<template v-if="i < chronologicalPlaces.length - 1">,<br /></template>
        </template>
      </td>
    </tr>
  </template>
  <template v-else-if="places.length === 1">
    <tr>
      <td>{{ nameType }}</td>
      <td>{{ showDates ? addressWithDates(places[0], culture) : addressText(places[0], culture) }}</td>
    </tr>
  </template>
  <template v-else-if="places.length > 1">
    <tr>
      <td>{{ firstLetterToUpper(byLang(places[0].typeCs, places[0].typeEn)) }}</td>
      <td>{{ multipleText }}</td>
    </tr>
  </template>
</template>
