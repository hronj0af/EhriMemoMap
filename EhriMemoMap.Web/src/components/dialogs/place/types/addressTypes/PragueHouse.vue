<script setup lang="ts">
// Components/Dialogs/Place/Types/AddressTypes/PragueHouse.razor – souhrn osudů obyvatel domu a tabulka
// přítomných/deportovaných k jednotlivým datům (při vybraném bodu časové osy jen 1. 10. 1941 a ten bod).
import { computed, ref } from 'vue'
import type { AddressWithVictims } from '../../../../../models/api'
import { useMapStore } from '../../../../../stores/map'
import { addressStatistics, dateKey, formatDate } from '../../../../../models/helpers'
import { t } from '../../../../../i18n'
import RzRow from '../../../../ui/RzRow.vue'
import RzColumn from '../../../../ui/RzColumn.vue'
import RzCard from '../../../../ui/RzCard.vue'

const props = defineProps<{ house: AddressWithVictims | null }>()
const store = useMapStore()
const statisticsIsOpen = ref(false)

const statistics = computed(() => (props.house ? addressStatistics(props.house) : []))
const hasPresent = computed(() => statistics.value.some((a) => a.numberPresent != null))
const hasAbsent = computed(() => statistics.value.some((a) => a.numberAbsent != null))
const visibleStatistics = computed(() => {
  const point = store.timelinePoint
  if (point == null) return statistics.value
  return statistics.value.filter((a) => dateKey(a.date) === '1941-10-01' || dateKey(a.date) === point)
})

const showSurvivorsNote = computed(() => {
  const p = props.house?.pragueAddress
  if (!p) return false
  return ((p.fateUnknown ?? 0) > 0 || (p.survived ?? 0) > 0) && (!props.house?.victims || props.house.victims.length === 0)
})
</script>

<template>
  <template v-if="house && house.pragueAddress">
    <RzRow gap="1rem" justify-content="left">
      <RzColumn :size="12">
        <RzCard style="background-color: var(--Beige); padding: 1rem; color: var(--Grey)">
          <RzRow align-items="start" justify-content="space-between" style="cursor: pointer" @click="statisticsIsOpen = !statisticsIsOpen">
            <RzColumn :size="11" style="font-size: 14px">
              <span v-if="house.pragueAddress.murdered != null" class="rz-mr-2">{{ t('murdered') }}: {{ house.pragueAddress.murdered }}</span>
              <span v-if="house.pragueAddress.survived != null" class="rz-mr-2">{{ t('survived') }}: {{ house.pragueAddress.survived }}</span>
              <span v-if="house.pragueAddress.fateUnknown != null" class="rz-mr-2">
                {{ t('fateUnknown') }}: {{ house.pragueAddress.fateUnknown }}
              </span>
            </RzColumn>
            <RzColumn :size="1">
              <img v-if="!statisticsIsOpen" style="cursor: pointer" src="images/arrow-right.png" :title="t('showStatistics') + '...'" />
              <img v-else style="cursor: pointer" src="images/arrow-down.png" :title="t('hideStatistics') + '...'" />
            </RzColumn>
          </RzRow>
          <table v-if="statisticsIsOpen && statistics.length > 0" class="rz-grid-table rz-grid-table-fixed" style="width: 100%">
            <tr>
              <th :style="store.isMobileView ? 'width:30%' : 'width:50%'"></th>
              <th v-if="hasPresent" style="text-align: left; font-family: 'Barlow'; font-size: 14px">{{ t('present') }}</th>
              <th v-if="hasAbsent" style="text-align: left; font-family: 'Barlow'; font-size: 14px">{{ t('absent') }}</th>
            </tr>
            <tr v-for="statistic in visibleStatistics" :key="statistic.date">
              <td>{{ formatDate(statistic.date) }}</td>
              <td v-if="hasPresent">{{ statistic.numberPresent }}</td>
              <td v-if="hasAbsent">{{ statistic.numberAbsent }}</td>
            </tr>
          </table>
        </RzCard>
      </RzColumn>
    </RzRow>
    <p v-if="showSurvivorsNote">
      <b>{{ t('survivorsNotDisplayed') }}</b>
    </p>
  </template>
</template>
