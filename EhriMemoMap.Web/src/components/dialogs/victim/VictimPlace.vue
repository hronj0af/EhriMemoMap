<script setup lang="ts">
// Components/Dialogs/Victim/VictimPlace.razor – řádek tabulky s místem daného typu (narození, bydliště…).
import { computed } from 'vue'
import { useMapStore } from '../../../stores/map'
import type { AddressInfo } from '../../../models/api'
import {
  addressDates,
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
 * Mapy s osobními trajektoriemi (Velké Meziříčí): místa jsou odkazy na bod v mapě, každé místo na vlastním
 * řádku a datace se nezalamuje (když se na řádek nevejde, přejde celá na další).
 */
const trajectoryMap = computed(() => store.map?.initialVariables?.personalTrajectories ?? false)
/** Bydliště chronologicky s roky v závorce (osoby tu mají hodně adres) */
const chronological = computed(() => trajectoryMap.value && props.type === RESIDENCE)

interface Item {
  place: AddressInfo
  text: string | null
  dates: string
}

const items = computed<Item[]>(() => {
  const list = chronological.value ? sortResidencesChronologically(places.value) : [...places.value].sort((a, b) => a.id - b.id)
  return list.map((place) => ({
    place,
    text: addressText(place, culture.value),
    dates: chronological.value
      ? residenceYears(place.dateFrom, place.dateTo)
      : props.showDates
        ? addressDates(place, culture.value)
        : '',
  }))
})

const nameType = computed(() => {
  if (places.value.length === 0) return ''
  if (props.resourceNameType && (places.value.length === 1 || !trajectoryMap.value)) return t(props.resourceNameType)
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
  <template v-if="trajectoryMap && places.length > 0">
    <tr>
      <td>{{ nameType }}</td>
      <td>
        <template v-for="(item, i) in items" :key="item.place.id + '_' + i">
          <button
            v-if="item.place.mapPoint"
            type="button"
            class="victim-place-link"
            :title="t('showOnMap')"
            @click="store.showPlaceOnMap(item.place.mapPoint)"
          >
            {{ item.text
            }}<svg class="victim-place-link-icon" viewBox="0 0 24 24" aria-hidden="true">
              <path
                d="M12 2C8.1 2 5 5.1 5 9c0 5.3 7 13 7 13s7-7.7 7-13c0-3.9-3.1-7-7-7zm0 9.5a2.5 2.5 0 1 1 0-5 2.5 2.5 0 0 1 0 5z"
              />
            </svg>
          </button>
          <template v-else>{{ item.text }}</template>
          {{ ' ' }}<span v-if="item.dates || i < items.length - 1" class="victim-place-dates"
            >{{ item.dates }}<template v-if="i < items.length - 1">,</template></span
          ><br v-if="i < items.length - 1" />
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
